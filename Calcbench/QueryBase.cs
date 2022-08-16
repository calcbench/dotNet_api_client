using System.Runtime.Serialization;

namespace Calcbench
{
    /// <summary>
    /// Most of the Calcbench end-points companies parameters, period parameters and end-point specific parameters
    /// </summary>
    [DataContract]
    public abstract class QueryBase
    {
        [DataMember(Name = "companiesParameters")]
        public CompaniesParameters CompaniesParameters { get; set; }

        [DataMember(Name = "periodParameters")]
        public PeriodParameters PeriodParameters { get; set; }

    }
}
