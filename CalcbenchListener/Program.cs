// From https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;

namespace CalcbenchListener
{
    using System;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Azure.ServiceBus;

    class Program
    {
        const string CalcbenchFilingsTopic = "filings";
        static ISubscriptionClient subscriptionClient;

        static void Main(string[] args)
        {
            MainAsync().GetAwaiter().GetResult();
        }

        static async Task MainAsync()
        {
            var connectionString = Properties.Settings.Default.ServiceBusConnectionString;
            var subscription = Properties.Settings.Default.Subscription;
            subscriptionClient = new SubscriptionClient(connectionString, CalcbenchFilingsTopic, subscription);

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
            var filing = JsonConvert.DeserializeObject<Filings.Filing>(Encoding.UTF8.GetString(message.Body));
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
    }
}

namespace Filings
{
    [DataContract]
    public enum FilingType
    {
        BusinessWirePR_replaced = -1,
        BusinessWirePR_filedAfterAn8K = -2,
        proxy = 1,
        annualQuarterlyReport = 2,
        eightk_earningsPressRelease = 3,
        eightk_guidanceUpdate = 4,
        eightk_conferenceCallTranscript = 5,
        eightk_presentationSlides = 6,
        eightk_monthlyOperatingMetrics = 7,
        eightk_earningsPressRelease_preliminary = 8,
        eightk_earningsPressRelease_correction = 9,
        eightk_other = 10, //  ' 2.02 OTHER 
        commentLetter = 11, // ' from sec  UPLOAD
        commentLetterResponse = 12,//  ' from company   CORRESP

        form_3 = 13,
        form_4 = 14,
        form_5 = 15,

        eightk_nonfinancial = 20,// non 2.02's
        NT10KorQ = 25,
        S = 26,
        Four24B = 27,
    }
    [DataContract]
    public class Filing
    {
        [DataMember(Name = "is_xbrl")]
        public bool is_xbrl;
        [DataMember(Name = "is_wire", EmitDefaultValue = false)]
        public bool? is_wire;
        [DataMember(Name = "calcbench_id", EmitDefaultValue = false)]
        public int calcbench_id;
        [DataMember(Name = "sec_accession_id", EmitDefaultValue = false)]
        public string sec_accession_id;
        [DataMember(Name = "sec_html_url", EmitDefaultValue = false)]
        public string sec_html_url;
        [DataMember(Name = "document_type", EmitDefaultValue = false)]
        /// <summary>
        /// 10-K, 10-Q, etc
        /// </summary>
        public string document_type;
        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "filing_type", EmitDefaultValue = false)]
        public FilingType filing_type;
        [DataMember(Name = "filing_sub_type", EmitDefaultValue = false)]
        public string filing_sub_type;
        [DataMember(Name = "filing_date", EmitDefaultValue = false)]
        public DateTime filing_date;
        [DataMember(Name = "fiscal_period", EmitDefaultValue = false)]
        public int? fiscal_period = default(int?);
        [DataMember(Name = "fiscal_year", EmitDefaultValue = false)]
        public int? fiscal_year = default(int?);
        [DataMember(Name = "calcbench_accepted", EmitDefaultValue = false)]
        public DateTime? calcbench_accepted;
        [DataMember(Name = "calcbench_finished_load", EmitDefaultValue = false)]
        public DateTime? calcbench_finished_load;
        [DataMember(Name = "entity_id", EmitDefaultValue = false)]
        public int? entity_id = default(int?);
        [DataMember(Name = "ticker", EmitDefaultValue = false)]
        public string ticker;
        [DataMember(Name = "entity_name", EmitDefaultValue = false)]
        public string entity_name;
        [DataMember(Name = "period_index", EmitDefaultValue = false)]
        public int period_index;
        [DataMember(Name = "associated_proxy_SEC_URL", EmitDefaultValue = false)]
        public string associated_proxy_SEC_URL;
        [DataMember(Name = "associated_earnings_press_release_SEC_URL", EmitDefaultValue = false)]
        public string associated_earnings_press_release_SEC_URL;
        [DataMember(Name = "period_end_date", EmitDefaultValue = false)]
        public DateTime? period_end_date = default(DateTime?);
        [DataMember(Name = "percentage_revenue_change", EmitDefaultValue = false)]
        public decimal? percentage_revenue_change = default(decimal?);
        [DataMember(Name = "this_period_revenue", EmitDefaultValue = false)]
        public decimal? this_period_revenue = default(decimal?);
        [DataMember(Name = "link1", EmitDefaultValue = false)]
        /// <summary>
        /// First link on Filing Detail page on Edgar
        /// </summary>
        public string link1;
        [DataMember(Name = "link2", EmitDefaultValue = false)]
        /// <summary>
        /// Second link on Filing Detail page on Edgar
        /// </summary>
        public string link2;
        [DataMember(Name = "link3", EmitDefaultValue = false)]
        /// <summary>
        /// Third link on the Filing Detail page on Edgar
        /// </summary>
        public string link3;

    }
}
