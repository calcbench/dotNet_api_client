using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Calcbench
{
    [DataContract]
    public class Filing
    {
        [DataMember(Name = "is_xbrl")]
        public bool is_xbrl { get; set; }

        [DataMember(Name = "is_wire", EmitDefaultValue = false)]
        public bool? is_wire { get; set; }

        [DataMember(Name = "calcbench_id", EmitDefaultValue = false)]
        public int calcbench_id { get; set; }

        [DataMember(Name = "sec_accession_id", EmitDefaultValue = false)]
        public string sec_accession_id { get; set; }

        [DataMember(Name = "sec_html_url", EmitDefaultValue = false)]
        public string sec_html_url { get; set; }

        [DataMember(Name = "document_type", EmitDefaultValue = false)]
        /// <summary>
        /// 10-K, 10-Q, etc
        /// </summary>
        public string document_type { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        [DataMember(Name = "filing_type", EmitDefaultValue = false)]
        public FilingType filing_type { get; set; }

        [DataMember(Name = "filing_sub_type", EmitDefaultValue = false)]
        public string filing_sub_type { get; set; }

        [DataMember(Name = "filing_date", EmitDefaultValue = false)]
        public DateTime filing_date { get; set; }

        [DataMember(Name = "fiscal_period", EmitDefaultValue = false)]
        public int? fiscal_period { get; set; } = default(int?);

        [DataMember(Name = "fiscal_year", EmitDefaultValue = false)]
        public int? fiscal_year { get; set; } = default(int?);

        [DataMember(Name = "calcbench_accepted", EmitDefaultValue = false)]
        public DateTime? calcbench_accepted { get; set; }

        [DataMember(Name = "calcbench_finished_load", EmitDefaultValue = false)]
        public DateTime? calcbench_finished_load { get; set; }

        [DataMember(Name = "entity_id", EmitDefaultValue = false)]
        public int? entity_id { get; set; } = default(int?);

        [DataMember(Name = "ticker", EmitDefaultValue = false)]
        public string ticker { get; set; }

        [DataMember(Name = "entity_name", EmitDefaultValue = false)]
        public string entity_name { get; set; }

        [DataMember(Name = "period_index", EmitDefaultValue = false)]
        public int period_index { get; set; }

        [DataMember(Name = "associated_proxy_SEC_URL", EmitDefaultValue = false)]
        public string associated_proxy_SEC_URL { get; set; }

        [DataMember(Name = "associated_earnings_press_release_SEC_URL", EmitDefaultValue = false)]
        public string associated_earnings_press_release_SEC_URL { get; set; }

        [DataMember(Name = "period_end_date", EmitDefaultValue = false)]
        public DateTime? period_end_date { get; set; } = default(DateTime?);

        [DataMember(Name = "percentage_revenue_change", EmitDefaultValue = false)]
        public decimal? percentage_revenue_change { get; set; } = default(decimal?);

        [DataMember(Name = "this_period_revenue", EmitDefaultValue = false)]
        public decimal? this_period_revenue { get; set; } = default(decimal?);

        [DataMember(Name = "link1", EmitDefaultValue = false)]
        /// <summary>
        /// First link on Filing Detail page on Edgar
        /// </summary>
        public string link1 { get; set; }

        [DataMember(Name = "link2", EmitDefaultValue = false)]
        /// <summary>
        /// Second link on Filing Detail page on Edgar
        /// </summary>
        public string link2 { get; set; }

        [DataMember(Name = "link3", EmitDefaultValue = false)]
        /// <summary>
        /// Third link on the Filing Detail page on Edgar
        /// </summary>
        public string link3 { get; set; }

    }
}
