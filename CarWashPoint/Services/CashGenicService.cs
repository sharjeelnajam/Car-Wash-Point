using CarWashPoint.Models;
using Newtonsoft.Json;
using System.Text;

namespace CarWashPoint.Services
{
    public class CashGenicService
    {
        private readonly HttpClient _httpClient;
        private string _baseUrl;
        private string _token;

        public CashGenicService(string apiIp, int apiPort = 44333)
        {
            _baseUrl = $"https://{apiIp}:{apiPort}";
            _httpClient = new HttpClient();
        }

        // Authenticate and get token
        //public async Task<TokenResponse> AuthenticateAsync(string username, string password)
        //{
        //    var content = new FormUrlEncodedContent(new[]
        //    {
        //    new KeyValuePair<string, string>("grant_type", "password"),
        //    new KeyValuePair<string, string>("username", username),
        //    new KeyValuePair<string, string>("password", password)
        //});

        //    var response = await _httpClient.PostAsync($"{_baseUrl}/token", content);
        //    response.EnsureSuccessStatusCode();
        //    var json = await response.Content.ReadAsStringAsync();
        //    _token = JsonConvert.DeserializeObject<TokenResponse>(json).AccessToken;
        //    return JsonConvert.DeserializeObject<TokenResponse>(json);
        //}

        // Start a payment session
        public async Task<SessionResponse> PayAmountAsync(int amountInPennies)
        {
            var request = new SessionRequest { Request = "PayAmount", Value = amountInPennies };
            return await PostSessionAsync(request);
        }

        // Refund (change dispensing)
        public async Task<SessionResponse> RefundAmountAsync(int amountInPennies)
        {
            var request = new SessionRequest { Request = "RefundAmount", Value = amountInPennies };
            return await PostSessionAsync(request);
        }

        // Get transaction logs
        public async Task<LogResponse> GetTransactionLogsAsync(int index = 0)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/log?mode=index&value={index}");
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<LogResponse>(await response.Content.ReadAsStringAsync());
        }

        private async Task<SessionResponse> PostSessionAsync(SessionRequest request)
        {
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.PostAsync($"{_baseUrl}/session", content);
            response.EnsureSuccessStatusCode();
            return JsonConvert.DeserializeObject<SessionResponse>(await response.Content.ReadAsStringAsync());
        }
    }
}
