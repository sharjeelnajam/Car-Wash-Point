using Microsoft.JSInterop;

namespace CarWashApp.Services.Scanner
{
    /// <summary>
    /// Uses the scanner on the PC where the browser is open. The page talks to the small
    /// "scanner helper" program running on that PC (see the ScannerHelper project), so this
    /// works even when the web app itself runs on a server.
    /// </summary>
    public class ScannerService(IJSRuntime js)
    {
        public record HelperReply(
            bool Ok,
            bool Connected,
            int Count,
            string? Name,
            bool Open,
            string? Error,
            string? ImageBase64,
            bool HelperMissing);

        private async Task<HelperReply> CallAsync(string function)
        {
            var reply = await js.InvokeAsync<HelperReply>($"scannerBridge.{function}");
            if (reply.HelperMissing)
                throw new InvalidOperationException(reply.Error ?? "The scanner helper is not running on this PC.");
            return reply;
        }

        public async Task<bool> IsScannerConnectedAsync() => (await CallAsync("status")).Connected;

        public async Task<string?> GetScannerNameAsync() => (await CallAsync("status")).Name;

        public async Task<bool> OpenScannerAsync() => (await CallAsync("open")).Ok;

        public async Task<bool> CloseScannerAsync() => (await CallAsync("close")).Ok;

        /// <summary>Scans on the shop PC and saves the picture to <paramref name="savePath"/> on the server.</summary>
        public async Task<string?> ScanInvoiceAsync(string savePath)
        {
            var reply = await CallAsync("scan");
            if (!reply.Ok || string.IsNullOrEmpty(reply.ImageBase64))
                throw new InvalidOperationException(reply.Error ?? "The scanner did not return an image.");

            await File.WriteAllBytesAsync(savePath, Convert.FromBase64String(reply.ImageBase64));
            return savePath;
        }
    }
}
