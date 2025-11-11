using System;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        using var client = new NamedPipeClientStream(".", "ScannerPipe", PipeDirection.InOut);

        Console.WriteLine("Connecting to pipe...");
        await client.ConnectAsync();

        using var reader = new StreamReader(client);
        using var writer = new StreamWriter(client) { AutoFlush = true };

        // Send command
        string command = "GetDeviceCount";
        Console.WriteLine("Sending command: " + command);
        await writer.WriteLineAsync(command);

        // Read response
        string response = await reader.ReadLineAsync();
        Console.WriteLine("Response: " + response);
        Console.ReadLine();
    }
}
