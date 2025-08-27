using System.Text.Json;

namespace CarWashApp.Services.HttpClientServices
{
    public class CWPClient
    {
        HttpClient client;
        public CWPClient(HttpClient httpClient)
        {
            httpClient.BaseAddress = new Uri("");
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
    }
}
