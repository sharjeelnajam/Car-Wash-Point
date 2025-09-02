using System.Text.Json;

namespace CarWashApp.Services.HttpClientServices
{
    public class CWPClient
    {
        HttpClient client;
        public CWPClient(IConfiguration config, HttpClient httpClient)
        {
            //var baseUrl = config.GetSection("Api:BaseUrl").ToString() ?? throw new ArgumentNullException("Api:BaseUrl");
            //httpClient.BaseAddress = new Uri(baseUrl);
            client = httpClient;
        }

        async Task<string> GetAuthorizationToken()
        {
            client.DefaultRequestHeaders.Add("content-type", "application/x-www-form-urlencoded");

            var body = new Dictionary<string, string>()
            {
                { "grant_type", "password" },
                { "username", "<username>" },
                { "password", "<password>" }
            };

            HttpResponseMessage response = await client.PostAsync("/token", new FormUrlEncodedContent(body));
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<dynamic> PayAmount(int value)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/session");

            var req = await client.PostAsJsonAsync("/session", new
            {
                request = "PayAmount",
                value = value
            });

            return req;
        }
    }
}
