using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Calcbench
{
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
}
