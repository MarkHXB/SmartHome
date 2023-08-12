using System.Text;

namespace Saturn.Mobile.Services
{
    public class BaseRestRequest
    {
        private const string domain = "localhost:7160";

        public virtual async Task<string> GetRequest(string command, KeyValuePair<string, string>? pair = null)
        {
            using (HttpClient client = new HttpClient())
            {
                string uri = $"{domain}/{command}";

                if (pair is not null)
                {
                    uri += $"?{pair?.Key}={pair?.Value}";
                }

                return await client.GetStringAsync(uri);
            }
        }
        public virtual async Task<HttpResponseMessage> PostRequest(string uri, string? jsonContent = "")
        {
            using (HttpClient client = new HttpClient())
            {
                var data = new StringContent(jsonContent.ToString(), Encoding.UTF8, "application/json");

                return await client.PostAsync(uri, data);
            }
        }
    }
}
