using System.Net;
using System.Text;
using System.Text.Json;
using CarWashApp.Responses.Status;
using Microsoft.JSInterop;

namespace CarWashApp.Services.HttpClientServices
{
    /// <summary>
    /// Talks to the cash machine. The machine is in the shop, so the calls do not go from the server:
    /// the page sends them through the browser to the helper program on the shop PC, which calls the machine.
    /// </summary>
    public class CWPClient(IJSRuntime js)
    {
        private sealed record CashReply(int Status, string? Body, string? Error, bool HelperMissing);

        private async Task<HttpResponseMessage> SendAsync(string method, string path, string? json = null)
        {
            var reply = await js.InvokeAsync<CashReply>("cashBridge.request", method, path, json);

            if (reply.HelperMissing)
                throw new InvalidOperationException(reply.Error ?? "The scanner helper is not running on this PC.");

            if (reply.Status == 0)
                throw new InvalidOperationException(reply.Error ?? "Could not reach the cash machine.");

            return new HttpResponseMessage((HttpStatusCode)reply.Status)
            {
                Content = new StringContent(reply.Body ?? "", Encoding.UTF8, "application/json")
            };
        }

        private Task<HttpResponseMessage> PostSessionAsync(object request) =>
            SendAsync("POST", "/session", JsonSerializer.Serialize(request));

        public async Task<HttpResponseMessage> PayAmount(decimal value)
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

            return await PostSessionAsync(new { request = "PayAmount", value = Math.Truncate(value) });
        }

        public async Task<StatusResponse> CheckPaymentStatus()
        {
            var response = await SendAsync("GET", "/status?eventVersion=2");
            var responseString = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<StatusResponse>(responseString);
        }

        public async Task<HttpResponseMessage> CancelPayment()
        {
            var response = await PostSessionAsync(new { request = "CancelPayment" });

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

        public Task<HttpResponseMessage> StopPayment() =>
            PostSessionAsync(new { request = "StopPayment" });

        public Task<HttpResponseMessage> RefundAmount(decimal value) =>
            PostSessionAsync(new { request = "RefundAmount", value = Math.Truncate(value) });

        public Task<HttpResponseMessage> CloseSession() =>
            PostSessionAsync(new { request = "CloseSession" });

        /// <summary>
        /// Safely starts a new payment by closing any existing session first
        /// </summary>
        public async Task<(bool Success, string Message, HttpResponseMessage Response)> StartPaymentSafely(decimal value)
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
