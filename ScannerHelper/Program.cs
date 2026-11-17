using ScannerHelper;

// Small local service for the shop PC: lets the web app (running on the VPS) use the scanner
// that is plugged into this PC. It only listens on 127.0.0.1 and only answers the allowed sites.
var builder = WebApplication.CreateBuilder(args);

// Secrets for this PC (for example the cash machine password) live in appsettings.Local.json,
// which the install script creates. It is never part of the published files.
builder.Configuration.AddJsonFile(
    Path.Combine(AppContext.BaseDirectory, "appsettings.Local.json"), optional: true, reloadOnChange: true);

var port = builder.Configuration.GetValue("Helper:Port", 5555);
builder.WebHost.UseUrls($"http://127.0.0.1:{port}");

var allowedOrigins = builder.Configuration.GetSection("Helper:AllowedOrigins").Get<string[]>()
                     ?? new[] { "https://app.drwash.co.uk" };

var app = builder.Build();

var cash = new CashProxy(app.Configuration);
var scanner = new NativeScanner();
var gate = new object();
var isOpen = false;

void Log(string message)
{
    try { File.AppendAllText(Path.Combine(AppContext.BaseDirectory, "helper.log"), $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} | {message}{Environment.NewLine}"); }
    catch { /* logging must never stop the helper */ }
}

// Only the allowed website may use the scanner. Other sites get 403 (this also stops cross-site POSTs).
app.Use(async (ctx, next) =>
{
    var origin = ctx.Request.Headers.Origin.ToString();
    if (origin.Length > 0)
    {
        if (!allowedOrigins.Contains(origin, StringComparer.OrdinalIgnoreCase))
        {
            Log($"Blocked request from origin {origin}");
            ctx.Response.StatusCode = 403;
            return;
        }

        ctx.Response.Headers["Access-Control-Allow-Origin"] = origin;
        ctx.Response.Headers["Vary"] = "Origin";
        ctx.Response.Headers["Access-Control-Allow-Methods"] = "GET, POST, OPTIONS";
        ctx.Response.Headers["Access-Control-Allow-Headers"] = "Content-Type";
        // Chrome asks for this when a public website talks to a program on the same PC.
        ctx.Response.Headers["Access-Control-Allow-Private-Network"] = "true";
    }

    if (HttpMethods.IsOptions(ctx.Request.Method))
    {
        ctx.Response.StatusCode = 204;
        return;
    }

    await next();
});

// The web app's cash-machine calls come through here. Only the two calls the app really uses are allowed.
app.MapPost("/cash", async (CashRequest req) =>
{
    if (!cash.IsConfigured)
        return Results.Json(new { status = 0, error = "The cash machine is not set up in the scanner helper (see appsettings.Local.json)." });

    var method = (req.Method ?? "").ToUpperInvariant();
    var path = req.Path ?? "";
    var allowed = (method == "GET" && path.StartsWith("/status")) || (method == "POST" && path == "/session");
    if (!allowed)
        return Results.Json(new { status = 0, error = "That cash request is not allowed." });

    try
    {
        var (status, body) = await cash.SendAsync(method, path, req.Body);
        return Results.Json(new { status, body });
    }
    catch (Exception ex)
    {
        var message = ex.InnerException?.Message ?? ex.Message;
        Log($"cash {method} {path} failed: {message}");
        return Results.Json(new { status = 0, error = $"Could not reach the cash machine at {cash.BaseUrl}: {message}" });
    }
});

app.MapGet("/status", () =>
{
    lock (gate)
    {
        try
        {
            var count = scanner.DeviceCount();
            var name = count > 0 ? scanner.DeviceName(0) : null;
            return Results.Json(new { ok = true, connected = count > 0, count, name, open = isOpen });
        }
        catch (Exception ex)
        {
            Log($"status failed: {ex.Message}");
            return Results.Json(new { ok = false, connected = false, error = ex.Message });
        }
    }
});

app.MapPost("/open", () =>
{
    lock (gate)
    {
        try
        {
            if (isOpen) return Results.Json(new { ok = true });
            if (scanner.DeviceCount() <= 0)
                return Results.Json(new { ok = false, error = "No scanner device detected. Check the scanner is plugged in." });

            isOpen = scanner.Open(0);
            Log($"open -> {isOpen}");
            return Results.Json(new { ok = isOpen, error = isOpen ? null : "The scanner could not be opened." });
        }
        catch (Exception ex)
        {
            Log($"open failed: {ex.Message}");
            return Results.Json(new { ok = false, error = ex.Message });
        }
    }
});

app.MapPost("/close", () =>
{
    lock (gate)
    {
        try
        {
            if (isOpen) scanner.Close();
            isOpen = false;
            return Results.Json(new { ok = true });
        }
        catch (Exception ex)
        {
            Log($"close failed: {ex.Message}");
            isOpen = false;
            return Results.Json(new { ok = false, error = ex.Message });
        }
    }
});

app.MapPost("/scan", async () =>
{
    var tmp = Path.Combine(Path.GetTempPath(), $"drwash-scan-{Guid.NewGuid():N}.png");
    try
    {
        string? failure = null;
        lock (gate)
        {
            if (!isOpen)
            {
                if (scanner.DeviceCount() <= 0)
                    failure = "No scanner device detected. Check the scanner is plugged in.";
                else if (!(isOpen = scanner.Open(0)))
                    failure = "The scanner could not be opened.";
                else
                    Thread.Sleep(800); // let the camera start
            }

            if (failure == null && !scanner.Capture(tmp))
                failure = "The scanner did not return an image.";
        }

        if (failure != null)
            return Results.Json(new { ok = false, error = failure });

        // The vendor DLL writes the file; wait for it to appear.
        for (var i = 0; i < 30 && !File.Exists(tmp); i++)
            await Task.Delay(150);

        if (!File.Exists(tmp))
            return Results.Json(new { ok = false, error = "The scan finished but no image file was created." });

        // Give the DLL a moment to finish writing before reading.
        await Task.Delay(200);
        var bytes = await File.ReadAllBytesAsync(tmp);
        Log($"scan ok, {bytes.Length} bytes");
        return Results.Json(new { ok = true, imageBase64 = Convert.ToBase64String(bytes) });
    }
    catch (Exception ex)
    {
        Log($"scan failed: {ex}");
        return Results.Json(new { ok = false, error = ex.Message });
    }
    finally
    {
        try { if (File.Exists(tmp)) File.Delete(tmp); } catch { /* ignore */ }
    }
});

Log($"Scanner helper started on http://127.0.0.1:{port}");
app.Run();

record CashRequest(string? Method, string? Path, string? Body);
