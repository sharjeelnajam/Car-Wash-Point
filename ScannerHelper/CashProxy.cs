using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ScannerHelper
{
    /// <summary>
    /// Talks to the cash machine's API from the shop PC. The web app (on the VPS) sends its requests
    /// here through the browser, so the cash machine never has to be reachable from the internet.
    /// Also logs in to the machine and keeps the access token.
    /// </summary>
    public sealed class CashProxy
    {
        private readonly HttpClient _http;
        private readonly string _tokenPath;
        private readonly string _username;
        private readonly string _password;
        private readonly SemaphoreSlim _gate = new(1, 1);
        private string? _token;
        private DateTimeOffset _expiresAtUtc = DateTimeOffset.MinValue;

        public bool IsConfigured { get; }
        public string BaseUrl { get; }

        public CashProxy(IConfiguration config)
        {
            BaseUrl = (config["Cash:BaseUrl"] ?? "").TrimEnd('/');
            _tokenPath = config["Cash:TokenPath"] ?? "/token";
            _username = config["Cash:Username"] ?? "";
            _password = config["Cash:Password"] ?? "";
            IsConfigured = BaseUrl.Length > 0 && _username.Length > 0 && _password.Length > 0;

            var pinned = (config["Cash:PinnedThumbprint"] ?? "").Replace(" ", "").Replace(":", "").ToUpperInvariant();
            var host = IsConfigured ? new Uri(BaseUrl).Host : "";

            var handler = new HttpClientHandler
            {
                // The cash machine uses a self-signed certificate. Trust it for its own address only
                // (or only the pinned certificate, if a thumbprint is configured).
                ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
                {
                    if (errors == System.Net.Security.SslPolicyErrors.None) return true;
                    if (!string.Equals(request.RequestUri?.Host, host, StringComparison.OrdinalIgnoreCase)) return false;
                    if (pinned.Length == 0) return true;
                    return string.Equals(cert?.GetCertHashString(), pinned, StringComparison.OrdinalIgnoreCase);
                }
            };

            _http = new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(20) };
        }

        /// <summary>Sends one request to the cash machine and returns its status code and body.</summary>
        public async Task<(int Status, string Body)> SendAsync(string method, string path, string? json)
        {
            await EnsureTokenAsync(force: false);
            var result = await SendOnceAsync(method, path, json);

            // Token expired or was invalidated: log in again once and retry.
            if (result.Status == (int)HttpStatusCode.Unauthorized)
            {
                await EnsureTokenAsync(force: true);
                result = await SendOnceAsync(method, path, json);
            }

            return result;
        }

        private async Task<(int Status, string Body)> SendOnceAsync(string method, string path, string? json)
        {
            using var request = new HttpRequestMessage(new HttpMethod(method), BaseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            if (json != null)
                request.Content = new StringContent(json, Encoding.UTF8, "application/json");

            using var response = await _http.SendAsync(request);
            return ((int)response.StatusCode, await response.Content.ReadAsStringAsync());
        }

        private async Task EnsureTokenAsync(bool force)
        {
            if (!force && !string.IsNullOrEmpty(_token) && DateTimeOffset.UtcNow < _expiresAtUtc.AddSeconds(-30))
                return;

            await _gate.WaitAsync();
            try
            {
                if (!force && !string.IsNullOrEmpty(_token) && DateTimeOffset.UtcNow < _expiresAtUtc.AddSeconds(-30))
                    return;

                var form = new Dictionary<string, string>
                {
                    ["grant_type"] = "password",
                    ["username"] = _username,
                    ["password"] = _password
                };

                using var response = await _http.PostAsync(BaseUrl + _tokenPath, new FormUrlEncodedContent(form));
                var payload = await response.Content.ReadAsStringAsync();
                if (!response.IsSuccessStatusCode)
                    throw new InvalidOperationException($"The cash machine refused the login ({(int)response.StatusCode}).");

                if (TryParseToken(payload, out var token, out var expiry))
                {
                    _token = token;
                    _expiresAtUtc = expiry;
                }
                else
                {
                    _token = payload.Trim();
                    _expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(5);
                }
            }
            finally
            {
                _gate.Release();
            }
        }

        private static bool TryParseToken(string json, out string token, out DateTimeOffset expiryUtc)
        {
            token = "";
            expiryUtc = DateTimeOffset.UtcNow.AddMinutes(5);
            try
            {
                using var doc = JsonDocument.Parse(json);
                if (!doc.RootElement.TryGetProperty("access_token", out var t) || string.IsNullOrWhiteSpace(t.GetString()))
                    return false;

                token = t.GetString()!.Trim();
                if (doc.RootElement.TryGetProperty("expires_in", out var e) && e.TryGetInt32(out var seconds) && seconds > 0)
                    expiryUtc = DateTimeOffset.UtcNow.AddSeconds(seconds);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
