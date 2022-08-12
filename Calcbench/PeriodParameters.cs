using System;
using System.Runtime.Serialization;

namespace Calcbench
{

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
}
