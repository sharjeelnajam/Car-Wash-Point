//using System.Net.Http.Headers;

//namespace CarWashApp.Services.HttpClientServices
//{
//    public class TokenHandler : DelegatingHandler
//    {
//        readonly HttpClient httpClient;

//        string username;
//        string password;

//        string? token;

//        public TokenHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration)
//        {
//            httpClient = httpClientFactory.CreateClient("AuthClient");
//            var baseUrl = configuration["Api:BaseUrl"].ToString() ?? throw new ArgumentNullException(nameof(configuration));
//            username = configuration["Api:Username"].ToString();
//            password = configuration["Api:Password"].ToString();
//            httpClient.BaseAddress = new Uri(baseUrl);
//            //httpClient.Timeout = TimeSpan.FromSeconds(10);
//        }

//        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
//        {
//            if (token is null)
//            {
//                try
//                {
//                    var body = new Dictionary<string, string>()
//                    {
//                        { "grant_type", "password" },
//                        { "username", username },
//                        { "password", password }
//                    };
//                    var content = new FormUrlEncodedContent(body);
//                    //content.Headers.Add("content-type", "application/x-www-form-urlencoded");
//                    HttpResponseMessage response = await httpClient.PostAsync("/token", new FormUrlEncodedContent(body));
//                    response.EnsureSuccessStatusCode();
//                    token = await response.Content.ReadAsStringAsync();
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine(ex);
//                    throw new InvalidOperationException("Failed to acquire token.", ex);
//                }
//            }

//            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

//            return await base.SendAsync(request, cancellationToken);
//        }
//    }
//}



using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace CarWashApp.Services.HttpClientServices
{
    public sealed class TokenHandler : DelegatingHandler
    {
        private readonly HttpClient _authClient;
        private readonly string _tokenPath;        // "/token" or "/connect/token"
        private readonly string _username;
        private readonly string _password;
        private readonly string? _clientId;
        private readonly string? _clientSecret;
        private readonly string? _scope;           // e.g. "api.read" or "api://resource/.default"

        private readonly SemaphoreSlim _gate = new(1, 1);
        private string? _accessToken;
        private DateTimeOffset _expiresAtUtc = DateTimeOffset.MinValue;

        private sealed record TokenResponse(
            string access_token,
            int? expires_in,
            string? token_type,
            string? scope);

        public TokenHandler(IHttpClientFactory factory, IConfiguration cfg)
        {
            // IMPORTANT: this should be the named client with your cert validation
            _authClient = factory.CreateClient("AuthClient");

            _tokenPath = cfg["Auth:TokenPath"] ?? "/token";
            _username = cfg["Auth:Username"] ?? throw new ArgumentNullException("Auth:Username");
            _password = cfg["Auth:Password"] ?? throw new ArgumentNullException("Auth:Password");
            _clientId = cfg["Auth:ClientId"];      // optional
            _clientSecret = cfg["Auth:ClientSecret"];  // optional
            _scope = cfg["Auth:Scope"];         // optional
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            await EnsureTokenAsync(ct);

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            var resp = await base.SendAsync(request, ct);

            // if token was expired/invalidated, refresh once and retry
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                resp.Dispose();
                await ForceRefreshAsync(ct);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                resp = await base.SendAsync(request, ct);
            }

            return resp;
        }

        private async Task EnsureTokenAsync(CancellationToken ct)
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTimeOffset.UtcNow < _expiresAtUtc.AddSeconds(-30))
                return;

            await _gate.WaitAsync(ct);
            try
            {
                if (!string.IsNullOrEmpty(_accessToken) && DateTimeOffset.UtcNow < _expiresAtUtc.AddSeconds(-30))
                    return;

                await AcquireTokenAsync(ct);
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task ForceRefreshAsync(CancellationToken ct)
        {
            await _gate.WaitAsync(ct);
            try
            {
                await AcquireTokenAsync(ct);
            }
            finally
            {
                _gate.Release();
            }
        }

        private async Task AcquireTokenAsync(CancellationToken ct)
        {
            var form = new Dictionary<string, string>
            {
                { "grant_type", "password" },
                { "username",   _username },
                { "password",   _password }
            };

            if (!string.IsNullOrWhiteSpace(_scope))
                form["scope"] = _scope;

            // Some servers require client credentials:
            // A) in body:
            if (!string.IsNullOrWhiteSpace(_clientId) && !string.IsNullOrWhiteSpace(_clientSecret))
            {
                form["client_id"] = _clientId!;
                form["client_secret"] = _clientSecret!;
            }

            using var content = new FormUrlEncodedContent(form);

            // B) or via Basic header (uncomment if your server expects this instead of body)
            // if (!string.IsNullOrWhiteSpace(_clientId) && !string.IsNullOrWhiteSpace(_clientSecret))
            // {
            //     var basic = Convert.ToBase64String(Encoding.ASCII.GetBytes($"{_clientId}:{_clientSecret}"));
            //     _authClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basic);
            // }

            using var resp = await _authClient.PostAsync(_tokenPath, content, ct);
            var payload = await resp.Content.ReadAsStringAsync(ct);

            if (!resp.IsSuccessStatusCode)
            {
                throw new InvalidOperationException(
                    $"Token request failed: {(int)resp.StatusCode} {resp.ReasonPhrase}. Body: {Truncate(payload, 400)}");
            }

            // Parse JSON token (most common)
            if (TryParseJsonToken(payload, out var tok, out var expiry))
            {
                _accessToken = tok;
                _expiresAtUtc = expiry;
                // Clear Basic header if you set it above
                // _authClient.DefaultRequestHeaders.Authorization = null;
                return;
            }

            // Fallback: token in plain text
            _accessToken = payload.Trim();
            _expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(5);
            // _authClient.DefaultRequestHeaders.Authorization = null;
        }

        private static bool TryParseJsonToken(string json, out string token, out DateTimeOffset expiryUtc)
        {
            token = "";
            expiryUtc = DateTimeOffset.UtcNow.AddMinutes(5);

            try
            {
                var model = JsonSerializer.Deserialize<TokenResponse>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                if (model is null || string.IsNullOrWhiteSpace(model.access_token))
                    return false;

                token = model.access_token.Trim();
                expiryUtc = (model.expires_in is int s && s > 0)
                    ? DateTimeOffset.UtcNow.AddSeconds(s)
                    : DateTimeOffset.UtcNow.AddMinutes(5);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max] + "…";
    }
}

