using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Calcbench
{
    internal class FilingQuery : QueryBase
    {

        [DataMember(Name = "pageParameters")]
        public FilingParams EndpointParameters { get; set; }
    }

    public class FilingParams
    {
        [DataMember(Name = "filingTypes")]
        public IEnumerable<FilingType> FilingTypes { get; set; }
    }
}
