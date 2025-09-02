using System.Net.Http.Headers;

namespace CarWashApp.Services.HttpClientServices
{
    public class TokenHandler : DelegatingHandler
    {
        readonly HttpClient httpClient;

        string username;
        string password;

        string? token;

        public TokenHandler(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            httpClient = httpClientFactory.CreateClient();
            var baseUrl = configuration["Api:BaseUrl"].ToString() ?? throw new ArgumentNullException(nameof(configuration));
            username = configuration["Api:BaseUrl"].ToString();
            password = configuration["Api:BaseUrl"].ToString();
            httpClient.BaseAddress = new Uri(baseUrl);
            //httpClient.Timeout = TimeSpan.FromSeconds(10);
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (token is null)
            {
                try
                {
                    var body = new Dictionary<string, string>()
                    {
                        { "grant_type", "password" },
                        { "username", username },
                        { "password", password }
                    };
                    var content = new FormUrlEncodedContent(body);
                    //content.Headers.Add("content-type", "application/x-www-form-urlencoded");
                    HttpResponseMessage response = await httpClient.PostAsync("/token", new FormUrlEncodedContent(body));
                    response.EnsureSuccessStatusCode();
                    token = await response.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    throw new InvalidOperationException("Failed to acquire token.", ex);
                }
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
