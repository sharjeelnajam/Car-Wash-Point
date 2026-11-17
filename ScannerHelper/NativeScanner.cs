using System.Text;
using CarWashApp.Services.Scanner;

namespace ScannerHelper
{
    /// <summary>Thin wrapper over the vendor's DevCapture.dll.</summary>
    public class NativeScanner
    {
        public int DeviceCount() => ScannerInterop.GetDeviceCount();

        public string? DeviceName(int index = 0)
        {
            var name = new StringBuilder(256);
            return ScannerInterop.GetDeviceName(index, name) == 0 ? null : name.ToString();
        }

        // 1280x720 is the resolution the web app used before.
        public bool Open(int index = 0) => ScannerInterop.OpenDevice(index, 1280, 720, IntPtr.Zero, false) == 0;

        public bool Close() => ScannerInterop.CloseDevice() == 0;

        public bool Capture(string savePath) => ScannerInterop.CaptureFromPreview(savePath, 1) != IntPtr.Zero;
    }
}
