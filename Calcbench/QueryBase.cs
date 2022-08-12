using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Calcbench
{
    [DataContract]
    public abstract class QueryBase
    {
        [DataMember(Name = "companiesParameters")]
        public CompaniesParameters CompaniesParameters { get; set; }

        [DataMember(Name = "periodParameters")]
        public PeriodParameters PeriodParameters { get; set; }

    }
}
