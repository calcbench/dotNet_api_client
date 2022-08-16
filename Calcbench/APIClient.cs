using System.Collections.Generic;
using System.Threading.Tasks;
using RestSharp;

namespace Calcbench
{
    public class APIClient
    {
        private readonly RestClient client;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="URLBase">Point at a different server</param>
        public APIClient(string URLBase = "https://www.calcbench.com/api")
        {
            var options = new RestClientOptions(URLBase)
            {
                ThrowOnAnyError = true,
            };
            if (URLBase != "https://www.calcbench.com/api")
            {
                options.RemoteCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

            }

            client = new RestClient(options);

            // client.UseNewtonsoftJson();
        }

        public async Task<IEnumerable<Filing>> Filings(IEnumerable<string> companyIdentifiers)
        {
            var query = new FilingQuery { CompaniesParameters = new CompaniesParameters { companyIdentifiers = companyIdentifiers } };
            var request = new RestRequest("/filingsv2").AddJsonBody(query);
            var response = await client.ExecutePostAsync<IEnumerable<Filing>>(request);
            if (!response.IsSuccessful)
            {
                throw response.ErrorException;
            }

            return response.Data;

        }
    }
}
