// From https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions

using Newtonsoft.Json;
using System;
using System.Runtime.Serialization;
using System.Net.Http;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Microsoft.Azure.ServiceBus;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.ServiceBus.Primitives;
using Calcbench;

namespace CalcbenchListener
{
    class Program
    {

        const string CalcbenchFilingsTopic = "filings";
        static ISubscriptionClient subscriptionClient;
        static HttpClient calcbenchClient = new HttpClient();
        static HashSet<string> tickersToTrack = new HashSet<string>(StringComparer.OrdinalIgnoreCase) {
            "msft",
            "ZVZZT",
            "wdc"
        };

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task AsyncSetCalcbenchCredentials()
        {
            calcbenchClient.BaseAddress = new Uri("https://www.calcbench.com");
            var credentials = new {
                email = ConfigurationManager.AppSettings["CalcbenchUserName"],
                password = ConfigurationManager.AppSettings["CalcbenchPassword"]
            };
            var response = await calcbenchClient.PostAsJsonAsync("account/LogOnAjax", credentials);
            response.EnsureSuccessStatusCode();
            var content = await response.Content.ReadAsStringAsync();
            if (content != "true")
            {
                throw new Exception("Incorrect Credentials, use the email and password you use to login to Calcbench.");
            }            
        }

        static async Task MainAsync()
        {

            await AsyncSetCalcbenchCredentials();
            var connectionStringBuilder = new ServiceBusConnectionStringBuilder(ConfigurationManager.AppSettings["AzureServiceBusConnectionString"]);
            Enum.TryParse(ConfigurationManager.AppSettings["AzureServiceBusTransportType"], out TransportType transportType);
            var tokenProvider = TokenProvider.CreateSharedAccessSignatureTokenProvider(connectionStringBuilder.SasKeyName, connectionStringBuilder.SasKey);
            subscriptionClient = new SubscriptionClient(endpoint: connectionStringBuilder.Endpoint,
                topicPath: CalcbenchFilingsTopic,
                subscriptionName: ConfigurationManager.AppSettings["QueueSubscription"],
                tokenProvider: tokenProvider,
                transportType: transportType);
            
            var rules = await subscriptionClient.GetRulesAsync();
            await Task.WhenAll(rules.Select(async rule => await subscriptionClient.RemoveRuleAsync(rule.Name)));

            await subscriptionClient.AddRuleAsync(new RuleDescription
            {
                Filter = new SqlFilter("FilingType = 'eightk_earningsPressRelease'"),
                Name = "PressReleasesOnly"
            });


            Console.WriteLine("======================================================");
            Console.WriteLine("Press ENTER key to exit after receiving all the messages.");
            Console.WriteLine("======================================================");

            // Register subscription message handler and receive messages in a loop.
            RegisterOnMessageHandlerAndReceiveMessages();

            Console.ReadKey();

            await subscriptionClient.CloseAsync();
        }

        static void RegisterOnMessageHandlerAndReceiveMessages()
        {
            // Configure the message handler options in terms of exception handling, number of concurrent messages to deliver, etc.
            var messageHandlerOptions = new MessageHandlerOptions(ExceptionReceivedHandler)
            {
                // Maximum number of concurrent calls to the callback ProcessMessagesAsync(), set to 1 for simplicity.
                // Set it according to how many messages the application wants to process in parallel.
                MaxConcurrentCalls = 1,

                // Indicates whether MessagePump should automatically complete the messages after returning from User Callback.
                // False below indicates the Complete will be handled by the User Callback as in `ProcessMessagesAsync` below.
                AutoComplete = false
            };

            // Register the function that processes messages.
            subscriptionClient.RegisterMessageHandler(ProcessMessagesAsync, messageHandlerOptions);
        }

        static async Task ProcessMessagesAsync(Message message, CancellationToken token)
        {
            // Process the message.
            Console.WriteLine($"Received message: SequenceNumber:{message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            var filing = JsonConvert.DeserializeObject<Filing>(Encoding.UTF8.GetString(message.Body));
            if (tickersToTrack.Contains(filing.ticker))
            {
                await GetPressReleaseData(filing);
            }


            
            // Complete the message so that it is not received again.
            // This can be done only if the subscriptionClient is created in ReceiveMode.PeekLock mode (which is the default).
            await subscriptionClient.CompleteAsync(message.SystemProperties.LockToken);

            // Note: Use the cancellationToken passed as necessary to determine if the subscriptionClient has already been closed.
            // If subscriptionClient has already been closed, you can choose to not call CompleteAsync() or AbandonAsync() etc.
            // to avoid unnecessary exceptions.
        }

        static Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

        static async Task GetPressReleaseData(Filing filing)
        {

            var apiParams = new PressReleaseSearchParams()
            {
                CompaniesParameters = new CompaniesParameters()
                {
                    companyIdentifiers = new[] { filing.ticker }
                },
                PeriodParameters = new PeriodParameters()
                {
                    Period = (Period)filing.fiscal_period,
                    Year = filing.fiscal_year,
                },
                PageParameters = new PressReleaseParams()
                {
                    MatchToPreviousPeriod = true,
                    StandardizeBOPPeriods = true
                }
            };
            var response = await calcbenchClient.PostAsJsonAsync("api/pressReleaseData", apiParams);
            response.EnsureSuccessStatusCode();
            var filingDataPonts = await response.Content.ReadAsAsync<IEnumerable<PressReleaseDataPoint>>();
            foreach (var item in filingDataPonts)
            {
                Console.WriteLine($"{item.Label}, {item.EffectiveValue}");
                // database.writeDatapoint(filingDataPoint);
            }

        }
    }

    [DataContract]
    public class PressReleaseSearchParams
    {
        [DataMember(Name = "pageParameters")]
        public PressReleaseParams PageParameters { get; set; }
        [DataMember(Name = "companiesParameters")]
        public CompaniesParameters CompaniesParameters { get; set; }

        [DataMember(Name = "periodParameters")]
        public PeriodParameters PeriodParameters { get; set; }
    }

    [DataContract]
    public class PressReleaseParams
    {
        [DataMember(Name = "matchToPreviousPeriod")]
        public bool MatchToPreviousPeriod { get; set; }
        [DataMember(Name = "standardizeBOPPeriods")]
        public bool StandardizeBOPPeriods { get; set; } 
    }


}

