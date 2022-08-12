using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;
using RestSharp.Serializers.NewtonsoftJson;

namespace Calcbench
{
    public class APIClient
    {
        private readonly RestClient client;
        private readonly string URLBase = "https://www.calcbench.com/api";

        public APIClient()
        {
            client = new RestClient(URLBase);
   //         client.UseNewtonsoftJson();
        }

        public async Task<IEnumerable<Filing>> Filings(IEnumerable<string> companyIdentifiers)
        {
            var request = new FilingQuery { CompaniesParameters = new CompaniesParameters { companyIdentifiers = companyIdentifiers } };
            var response = client.PostJsonAsync<FilingQuery, IEnumerable<Filing>>("/filingsV2", request);
            try
            {
            var data = await response;
                return data;
            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }
            return Array.Empty<Filing>();
        }
    }
}
