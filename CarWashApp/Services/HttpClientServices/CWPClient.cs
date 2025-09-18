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
            // First, try to close any existing session to ensure clean state
            try
            {
                await CloseSession();
            }
            catch
            {
                // Ignore errors when closing session - we'll proceed with new payment
            }

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
            
            // Always try to close session after cancellation attempt
            try
            {
                await CloseSession();
            }
            catch
            {
                // Ignore errors when closing session after cancellation
            }
            
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

        /// <summary>
        /// Safely starts a new payment by closing any existing session first
        /// </summary>
        public async Task<(bool Success, string Message, HttpResponseMessage Response)> StartPaymentSafely(int value)
        {
            try
            {
                // First, ensure any existing session is closed
                await CloseSession();
                
                // Small delay to ensure session is properly closed
                await Task.Delay(500);
                
                // Now start the new payment
                var response = await PayAmount(value);
                
                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (responseData.TryGetProperty("responseCode", out var codeElement) && 
                        codeElement.GetInt32() == 0)
                    {
                        return (true, "Payment started successfully", response);
                    }
                    else
                    {
                        var errorMessage = responseData.TryGetProperty("responseData", out var messageElement)
                            ? messageElement.GetString()
                            : "Payment initiation failed";
                        return (false, errorMessage, response);
                    }
                }
                else
                {
                    return (false, $"Payment failed with status: {response.StatusCode}", response);
                }
            }
            catch (Exception ex)
            {
                // Ensure session is closed on any error
                try
                {
                    await CloseSession();
                }
                catch
                {
                    // Ignore errors when closing session after an exception
                }
                
                return (false, $"Payment error: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Safely cancels payment and closes session
        /// </summary>
        public async Task<(bool Success, string Message)> CancelPaymentSafely()
        {
            try
            {
                // First try to cancel the payment
                var cancelResponse = await CancelPayment();
                
                if (cancelResponse.IsSuccessStatusCode)
                {
                    var responseContent = await cancelResponse.Content.ReadAsStringAsync();
                    var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
                    
                    if (responseData.TryGetProperty("responseCode", out var codeElement) && 
                        codeElement.GetInt32() == 0)
                    {
                        return (true, "Payment cancelled successfully");
                    }
                    else
                    {
                        // Even if cancel failed, session should be closed
                        return (false, "Payment cancellation failed, but session closed");
                    }
                }
                else
                {
                    // Even if cancel failed, session should be closed
                    return (false, "Payment cancellation failed, but session closed");
                }
            }
            catch (Exception ex)
            {
                // Ensure session is closed on any error
                try
                {
                    await CloseSession();
                }
                catch
                {
                    // Ignore errors when closing session after an exception
                }
                
                return (false, $"Cancel payment error: {ex.Message}");
            }
        }
    }
}