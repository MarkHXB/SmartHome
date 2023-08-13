using System.Text;

namespace Saturn.Mobile.Services
{
    public class BaseRestRequest
    {
        private const string domain = "https://10.0.2.2:7160/api/Features";

        public HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                if (cert.Issuer.Equals("CN=localhost"))
                    return true;
                return errors == System.Net.Security.SslPolicyErrors.None;
            };
            return handler;
        }

        public virtual async Task<string> GetRequest(string command, KeyValuePair<string, string>? pair = null)
        {
            try
            {
                using (HttpClient client = new HttpClient(GetInsecureHandler()))
                {
                    string uri = $"{domain}/{command}";

                    if (pair is not null)
                    {
                        uri += $"?{pair?.Key}={pair?.Value}";
                    }

                    return await client.GetStringAsync(uri);
                }
            }
            catch (Exception ex)
            {
                // log somewhere
            }

            return string.Empty;
        }
        public virtual async Task<HttpResponseMessage> PostRequest(string uri, string? jsonContent = "")
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var data = new StringContent(jsonContent.ToString(), Encoding.UTF8, "application/json");

                    return await client.PostAsync(uri, data);
                }
            }
            catch (Exception ex)
            {
                // log somewhere
                return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
            }        
        }
    }
}
