// From https://docs.microsoft.com/en-us/azure/service-bus-messaging/service-bus-dotnet-how-to-use-topics-subscriptions

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Runtime.Serialization;
using System.Net.Http;

using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Configuration;
using Filings;
using Microsoft.Azure.ServiceBus;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Azure.ServiceBus.Primitives;

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

    [DataContract]
    public class CompaniesParameters
    {
        [DataMember(Name = "companyIdentifiers")]
        public IEnumerable<string> companyIdentifiers;
        [DataMember(Name = "entireUniverse")]
        public bool EntireUniverse { get; set; }
        [DataMember(Name = "calcbenchEntityIDs")]
        public IEnumerable<int> CalcbenchEntityIDs { get; set; } = null;
    }

    [DataContract]
    public class PeriodParameters
    {
        [DataMember(Name = "year")]
        public int? Year { get; set; } = default(int?);
        [DataMember(Name = "period")]
        public virtual Period? Period { get; set; } = default(Period?);
        [DataMember(Name = "endYear")]
        public int? EndYear { get; set; } = default(int?);
        [DataMember(Name = "endPeriod")]
        public int? EndPeriod { get; set; } = default(int?);
        [DataMember(Name = "periodType")]
        public string PeriodType { get; set; } = null;
        [DataMember(Name = "useFiscalPeriod")]
        public bool UseFiscalPeriod { get; set; }

        [DataMember(Name = "allHistory")]
        public bool AllHistory { get; set; } = false;
        [DataMember(Name = "updateDate")]
        public DateTime? UpdateDate { get; set; } = default(DateTime?);
        [DataMember(Name = "updatedFrom")]
        public DateTime? UpdatedFrom { get; set; } = default(DateTime?);
        [DataMember(Name = "asOriginallyReported")]
        public bool AsOriginallyReported { get; set; }
    }


    public enum Period
    {
        unset = -1,
        y = 0,
        q1 = 1,
        q2 = 2,
        q3 = 3,
        q4 = 4,
        h1 = 5,
        Q3Cum = 6,
        TTM = 10,
        MRQ = 11,
        MRQ_fiscal = 12,
        MRCumulative = 13,
        year_fiscal = 20,
        combined = 21,
        allHistory = 22,
        MostRecentPeriod = 23
    }


    [DataContract]
    public class PressReleaseDataPoint
    {
        [DataMember(Name = "fact_id")]
        public int FactID { get; set; }
        [DataMember(Name = "sec_filing_id")]
        public int SECFilingID { get; set; }
        [DataMember(Name = "effective_value")]
        public decimal? EffectiveValue { get; set; }
        [DataMember(Name = "reported_value")]
        public decimal? ReportedValue { get; set; }
        [DataMember(Name = "entity_id")]
        public int EntityID { get; set; }
        [DataMember(Name = "fiscal_year")]
        public int? FiscalYear { get; set; }
        [DataMember(Name = "fiscal_period")]
        public string FiscalPeriod { get; set; }
        [DataMember(Name = "UOM")]
        public string UnitOfMeasure { get; set; }
        [DataMember(Name = "qname_id_min")]
        public int? QnameIDMin { get; set; }
        [DataMember(Name = "period_start")]
        public DateTime? PeriodStart { get; set; }
        [DataMember(Name = "period_end")]
        public DateTime? PeriodEnd { get; set; }
        [DataMember(Name = "period_instant")]
        public DateTime? PeriodInstant { get; set; }
        [DataMember(Name = "presentation_order")]
        public int? PresentationOrder { get; set; }
        [DataMember(Name = "statement_type")]
        public string StatementType { get; set; }
        [DataMember(Name = "table_id")]
        public string TableID { get; set; }
        [DataMember(Name = "label")]
        public string Label { get; set; }
        [DataMember(Name = "indent_level")]
        public int? IndentLevel { get; set; }
        [DataMember(Name = "column_label")]
        public string ColumnLabel { get; set; }
        [DataMember(Name = "column_label_short")]
        public string ColumnLabelShort { get; set; }
        [DataMember(Name = "presentation_order_original")]
        public int? PresentationOrderOriginal { get; set; }
        [DataMember(Name = "column_index")]
        public int? ColumnIndex { get; set; }
        [DataMember(Name = "extract_tag")]
        public string ExtractTag { get; set; }
        [DataMember(Name = "range_low")]
        public bool? RangeLow { get; set; }
        [DataMember(Name = "range_high")]
        public bool? RangeHigh { get; set; }
        [DataMember(Name = "is_guidance")]
        public bool? IsGuidance { get; set; }
        [DataMember(Name = "is_ulp_metric")]
        public bool? ISUlpMetric { get; set; }
        [DataMember(Name = "is_strict")]
        public bool? IsStrict { get; set; }
        [DataMember(Name = "filing_section")]
        public int? FilingSection { get; set; }
        [DataMember(Name = "is_non_gaap")]
        public bool? IsNonGAAP { get; set; }
        [DataMember(Name = "is_change_amount")]
        public bool? IsChangeAmount { get; set; }
        [DataMember(Name = "is_segment")]
        public bool? IsSegment { get; set; }
        [DataMember(Name = "is_bop_value")]
        public bool? IsBalanceOfPaymentAmount { get; set; }
        [DataMember(Name = "table_tag")]
        public string TableTag { get; set; }
        [DataMember(Name = "trace_link")]
        public string TraceLink { get; set; }
        [DataMember(Name = "matching_extract_tag")]
        public string MatchingExtractTag { get; set; }
        [DataMember(Name = "matching_fact_id_q")]
        public int? MatchingFactID { get; set; }
        [DataMember(Name = "matching_fact_id_y")]
        public int? MatchingFactIDY { get; set; }
        [DataMember(Name = "exact_match")]
        public bool? ExactMatch { get; set; }
        [DataMember(Name = "matching_trace_link")]
        public string MatchingTraceLink { get; set; }
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
