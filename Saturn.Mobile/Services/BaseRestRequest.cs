using System.Text;

namespace Saturn.Mobile.Services
{
    public class BaseRestRequest
    {
        private static string FeaturesUrl = "https://localhost:5001/api/Features/GetAll";

        public HttpClientHandler GetInsecureHandler()
        {
            HttpClientHandler handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) =>
            {
                return true;
            };
            return handler;
        }

        public virtual async Task<string> GetRequest(string command, KeyValuePair<string, string>? pair = null)
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    var response = await client.GetAsync(FeaturesUrl);

                    return await response.Content.ReadAsStringAsync();
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
