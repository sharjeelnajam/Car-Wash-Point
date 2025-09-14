using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Net.Http.Headers;

namespace CarWashApp.Services.HttpClientServices
{
    public class CWPClient
    {
        HttpClient client;
        readonly string username;
        readonly string password;
        string accessToken;
        public CWPClient(HttpClient httpClient, IConfiguration configuration)
        {
            var baseUrl = configuration["Auth:BaseUrl"] ?? throw new ArgumentNullException("Auth:BaseUrl");
            username = configuration["Auth:Username"] ?? throw new ArgumentNullException("Auth:Username");
            password = configuration["Auth:Password"] ?? throw new ArgumentNullException("Auth:Password");
            httpClient.BaseAddress = new Uri(baseUrl);
            client = httpClient;
        }

        async Task<string> GetAuthorizationToken()
        {
            var body = new Dictionary<string, string>()
            {
                { "grant_type", "password" },
                { "username", username },
                { "password", password }
            };

            var content = new FormUrlEncodedContent(body);
            HttpResponseMessage response = await client.PostAsync("/token", content);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Parse the JSON response to extract the access_token
            using var jsonDoc = JsonDocument.Parse(jsonResponse);
            if (jsonDoc.RootElement.TryGetProperty("access_token", out var tokenElement))
            {
                accessToken = tokenElement.GetString() ?? throw new InvalidOperationException("Access token is null");
                return accessToken;
            }

            throw new InvalidOperationException("Access token not found in response");
        }

        public async Task<HttpResponseMessage> PayAmount(int value)
        {
            var requestBody = new
            {
                request = "PayAmount",
                value = value
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/session", content);
            return response;
        }

        public async Task<HttpResponseMessage> CheckPaymentStatus()
        {
            var response = await client.GetAsync("/status");
            return response;
        }

        public async Task<HttpResponseMessage> CancelPayment()
        {
            // Use existing token - don't get a new one
            // The token should already be set from the previous PayAmount call

            var requestBody = new
            {
                request = "CancelPayment"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/session", content);
            return response;
        }

        public async Task<HttpResponseMessage> CloseSession()
        {
            // Use existing token - don't get a new one
            // The token should already be set from the previous PayAmount call

            var requestBody = new
            {
                request = "CloseSession"
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/session", content);
            return response;
        }
    }
}