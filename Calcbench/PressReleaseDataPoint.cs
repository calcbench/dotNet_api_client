using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Calcbench
{

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
