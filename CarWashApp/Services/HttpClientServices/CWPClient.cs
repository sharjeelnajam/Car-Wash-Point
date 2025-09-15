using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using CarWashApp.Responses.Status;

namespace CarWashApp.Services.HttpClientServices
{
    public class CWPClient
    {
        HttpClient client;
        readonly string username;
        readonly string password;

        public CWPClient(HttpClient httpClient, IConfiguration configuration)
        {
            var baseUrl = configuration["Auth:BaseUrl"] ?? throw new ArgumentNullException("Auth:BaseUrl");
            username = configuration["Auth:Username"] ?? throw new ArgumentNullException("Auth:Username");
            password = configuration["Auth:Password"] ?? throw new ArgumentNullException("Auth:Password");
            httpClient.BaseAddress = new Uri(baseUrl);
            client = httpClient;
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

        public async Task<StatusResponse> CheckPaymentStatus()
        {
            // The TokenHandler will add the Authorization header
            var json = await client.GetAsync("/status?eventVersion=2");
            var responseString = await json.Content.ReadAsStringAsync();
            var response = JsonSerializer.Deserialize<StatusResponse>(responseString);
            return response;
        }

        public async Task<HttpResponseMessage> CancelPayment()
        {
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