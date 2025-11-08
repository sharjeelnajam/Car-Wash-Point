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
            catch
            {
                throw;
            }
        }

        public string GetScannerName(int index = 0)
        {
            var nameBuffer = new StringBuilder(256);
            int result = ScannerInterop.GetDeviceName(index, nameBuffer);
            return result == 0 ? null : nameBuffer.ToString();
        }

        public bool OpenScanner(int index = 0)
        {
            // 1280x720 is a safe default resolution — can be changed later
            int result = ScannerInterop.OpenDevice(index, 1280, 720, IntPtr.Zero, false);
            return result == 0;
        }

        public bool CloseScanner()
        {
            int result = ScannerInterop.CloseDevice();
            return result == 0;
        }

        public string ScanInvoice(string savePath)
        {
            IntPtr pointer = ScannerInterop.CaptureFromPreview(savePath, 1);
            if (pointer == IntPtr.Zero)
                return null;

            return savePath;
        }
    }
}
