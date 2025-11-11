using System.IO.Pipes;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Text;

namespace ScannerBridge
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("ScannerBridgeService started and waiting for commands...");

            while (!stoppingToken.IsCancellationRequested)
            {
                using var server = new NamedPipeServerStream("ScannerPipe", PipeDirection.InOut, 1,
                            PipeTransmissionMode.Message, PipeOptions.Asynchronous);

                _logger.LogInformation("Waiting for client (Blazor) connection...");
                await server.WaitForConnectionAsync(stoppingToken);
                _logger.LogInformation("Client connected.");

                using var reader = new StreamReader(server);
                using var writer = new StreamWriter(server) { AutoFlush = true };

                try
                {
                    // Handle multiple commands from this client
                    while (server.IsConnected && !stoppingToken.IsCancellationRequested)
                    {
                        string request = await reader.ReadLineAsync();
                        if (string.IsNullOrWhiteSpace(request))
                            continue;

                        _logger.LogInformation($"Received command: {request}");

                        object response = HandleCommand(request);
                        await writer.WriteLineAsync(response?.ToString() ?? "null");

                        _logger.LogInformation($"Sent response: {response}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in client handling");
                }
            }
        }
        private object HandleCommand(string command)
        {
            try
            {
                // Split command into method name and parameters
                var parts = command.Split('|', StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 0)
                    return "InvalidCommand";

                string func = parts[0];

                switch (func) // handle case-insensitive
                {
                    case "OpenDevice":
                        int devIndex = 0;       // default device index
                        int width = 1280;        // default width
                        int height = 720;       // default height
                        bool isDisplay = false; // default display flag

                        if (parts.Length >= 2) int.TryParse(parts[1], out devIndex);
                        if (parts.Length >= 3) int.TryParse(parts[2], out width);
                        if (parts.Length >= 4) int.TryParse(parts[3], out height);
                        if (parts.Length >= 5) bool.TryParse(parts[4], out isDisplay);

                        return ScannerNative.OpenDevice(devIndex, width, height, IntPtr.Zero, isDisplay);

                    case "CloseDevice":
                        return ScannerNative.CloseDevice();

                    case "GetDeviceCount":
                        return ScannerNative.GetDeviceCount();

                    case "CaptureFromPreview":
                        string path = parts.Length >= 2 ? parts[1] : "C:\\Temp\\preview.jpg";
                        int isBarcode = 0; // default
                        if (parts.Length >= 3) int.TryParse(parts[2], out isBarcode);

                        var resultPtr = ScannerNative.CaptureFromPreview(path, isBarcode);
                        return resultPtr == IntPtr.Zero ? null : Marshal.PtrToStringAnsi(resultPtr);

                    case "GetDeviceName":
                        int index = 0; // default index
                        if (parts.Length >= 2 && !string.IsNullOrWhiteSpace(parts[1]) && int.TryParse(parts[1], out int parsed))
                            index = parsed;

                        var nameBuf = new StringBuilder(256);
                        var status = ScannerNative.GetDeviceName(index, nameBuf);
                        return nameBuf.ToString();

                    default:
                        return "UnknownCommand";
                }
            }
            catch (Exception ex)
            {
                return $"Error={ex.Message} \n Excepton = {ex.ToString()}";
            }
        }


    }
}
