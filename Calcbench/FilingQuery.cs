using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Calcbench
{
    [DataContract]
    public class FilingQuery : QueryBase
    {

        [DataMember(Name = "pageParameters")]
        public FilingParams EndpointParameters { get; set; }
    }

    [DataContract]
    /// <summary>
    /// Params specific to the filing end-point.
    /// </summary>
    public class FilingParams
    {
        [DataMember(Name = "filingTypes")]
        public IEnumerable<FilingType> FilingTypes { get; set; }
    }
}
