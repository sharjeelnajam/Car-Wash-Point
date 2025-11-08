using System.Text;

namespace CarWashApp.Services.Scanner
{
    public class ScannerService
    {
        public bool IsScannerConnected()
        {
            try
            {
                int count = ScannerInterop.GetDeviceCount();
                return count > 0;
            }
            catch (System.Exception ex)
            {
                // Log and rethrow with more context
                throw new InvalidOperationException($"Failed to check scanner connection: {ex.Message}", ex);
            }
        }

        public string GetScannerName(int index = 0)
        {
            try
            {
                var nameBuffer = new StringBuilder(256);
                int result = ScannerInterop.GetDeviceName(index, nameBuffer);
                return result == 0 ? null : nameBuffer.ToString();
            }
            catch (System.Exception ex)
            {
                throw new InvalidOperationException($"Failed to get scanner name: {ex.Message}", ex);
            }
        }

        public bool OpenScanner(int index = 0)
        {
            try
            {
                // 1280x720 is a safe default resolution — can be changed later
                int result = ScannerInterop.OpenDevice(index, 1280, 720, IntPtr.Zero, false);
                return result == 0;
            }
            catch (System.Exception ex)
            {
                throw new InvalidOperationException($"Failed to open scanner: {ex.Message}", ex);
            }
        }

        public bool CloseScanner()
        {
            try
            {
                int result = ScannerInterop.CloseDevice();
                return result == 1;
            }
            catch (System.Exception ex)
            {
                throw new InvalidOperationException($"Failed to close scanner: {ex.Message}", ex);
            }
        }

        public string ScanInvoice(string savePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(savePath))
                {
                    throw new ArgumentException("Save path cannot be null or empty", nameof(savePath));
                }

                IntPtr pointer = ScannerInterop.CaptureFromPreview(savePath, 1);
                if (pointer == IntPtr.Zero)
                    return null;

                return savePath;
            }
            catch (System.Exception ex)
            {
                throw new InvalidOperationException($"Failed to scan invoice: {ex.Message}", ex);
            }
        }
    }
}
