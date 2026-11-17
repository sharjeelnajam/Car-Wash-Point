using Blazored.Toast;
using CarWashApp;
using CarWashApp.Components;
using CarWashApp.Repositories;
using CarWashApp.Services;
using CarWashApp.Services.HttpClientServices;
using CarWashApp.Services.Scanner;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using TransactionWashApp.Repositories;
using Microsoft.AspNetCore.StaticFiles;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<CarWashAppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<StaffService>();
builder.Services.AddScoped(typeof(InvoiceService));
builder.Services.AddScoped<ScannerService>();
builder.Services.AddBlazoredToast();



//////// Helper so both clients share the exact same handler logic
//////HttpClientHandler BuildPinnedHandler(IConfiguration cfg)
//////{
//////    var baseUrl = cfg["Api:BaseUrl"] ?? throw new ArgumentNullException("Api:BaseUrl");
//////    var uri = new Uri(baseUrl, UriKind.Absolute);
//////    var allowedHost = uri.Host;
//////    var allowedPort = uri.IsDefaultPort ? (int?)null : uri.Port;

//////    var pinnedThumbprintRaw = cfg["Api:PinnedThumbprint"];
//////    var pinnedThumbprint = string.IsNullOrWhiteSpace(pinnedThumbprintRaw)
//////        ? null
//////        : pinnedThumbprintRaw.Replace(" ", "", StringComparison.Ordinal).ToUpperInvariant();

//////    return new HttpClientHandler
//////    {
//////        ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
//////        {
//////            bool hostMatches =
//////                string.Equals(request.RequestUri.Host, allowedHost, StringComparison.OrdinalIgnoreCase) &&
//////                (!allowedPort.HasValue || request.RequestUri.Port == allowedPort.Value);

//////            if (!hostMatches)
//////                return errors == SslPolicyErrors.None;

//////            if (errors == SslPolicyErrors.None)
//////                return true;

//////            if (string.IsNullOrEmpty(pinnedThumbprint) || cert is null)
//////                return false;

//////            bool thumbprintMatches = string.Equals(
//////                cert.GetCertHashString()?.ToUpperInvariant(),
//////                pinnedThumbprint,
//////                StringComparison.Ordinal);

//////            if (!thumbprintMatches)
//////                return false;

//////            var now = DateTimeOffset.UtcNow;
//////            if (now < cert.NotBefore || now > cert.NotAfter)
//////                return false;

//////            using var customChain = new X509Chain
//////            {
//////                ChainPolicy =
//////                {
//////                    RevocationMode = X509RevocationMode.NoCheck,
//////                    VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
//////                }
//////            };

//////            if (customChain.Build(cert))
//////                return true;

//////            bool onlyUnknownRootOrPartial =
//////                customChain.ChainStatus.Length > 0 &&
//////                Array.TrueForAll(customChain.ChainStatus, s =>
//////                    s.Status == X509ChainStatusFlags.UntrustedRoot ||
//////                    s.Status == X509ChainStatusFlags.PartialChain ||
//////                    s.Status == X509ChainStatusFlags.RevocationStatusUnknown);

//////            return onlyUnknownRootOrPartial;
//////        }
//////    };
//////}

//////// 1) Named client for token calls
//////builder.Services.AddHttpClient("AuthClient", (sp, client) =>
//////{
//////    var cfg = sp.GetRequiredService<IConfiguration>();
//////    var baseUrl = cfg["Api:BaseUrl"] ?? throw new ArgumentNullException("Api:BaseUrl");
//////    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
//////    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//////})
//////.ConfigurePrimaryHttpMessageHandler(sp =>
//////{
//////    var cfg = sp.GetRequiredService<IConfiguration>();
//////    return BuildPinnedHandler(cfg);
//////});

//////// 2) Typed client for your API (CWP)
//////builder.Services.AddHttpClient<CWPClient>((sp, client) =>
//////{
//////    var cfg = sp.GetRequiredService<IConfiguration>();
//////    var baseUrl = cfg["Api:BaseUrl"] ?? throw new ArgumentNullException("Api:BaseUrl");
//////    client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
//////    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
//////})
//////.ConfigurePrimaryHttpMessageHandler(sp =>
//////{
//////    var cfg = sp.GetRequiredService<IConfiguration>();
//////    return BuildPinnedHandler(cfg);
//////})
//////.AddHttpMessageHandler<TokenHandler>();   // attaches your TokenHandler





// The cash machine is reached through the scanner helper on the shop PC (via the browser),
// so the server needs no direct connection or certificate handling for it.
builder.Services.AddScoped<CWPClient>();



//builder.Services.AddHttpClient<CWPClient>(client =>
//{
//    var baseUrl = builder.Configuration["Api:BaseUrl"]
//                  ?? throw new ArgumentNullException("Api:BaseUrl cannot be null");

//    // Ensure the URI is well-formed (with scheme)
//    var baseUri = new Uri(baseUrl, UriKind.Absolute);

//    client.BaseAddress = baseUri;
//    client.DefaultRequestHeaders.Accept.Add(
//        new MediaTypeWithQualityHeaderValue("application/json"));
//})
//.ConfigurePrimaryHttpMessageHandler(() =>
//{
//    var baseUrl = builder.Configuration["Api:BaseUrl"]
//                  ?? throw new ArgumentNullException("Api:BaseUrl cannot be null");

//    var uri = new Uri(baseUrl, UriKind.Absolute);
//    var allowedHost = uri.Host;                        // e.g. "151.242.28.16" or "api.mycompany.local"
//    var allowedPort = uri.IsDefaultPort ? (int?)null : uri.Port;

//    // Optional (recommended): pin the leaf cert used by that endpoint
//    // Put thumbprint in config WITHOUT spaces, e.g. "A1B2C3...".
//    var pinnedThumbprintRaw = builder.Configuration["Api:PinnedThumbprint"];
//    var pinnedThumbprint = string.IsNullOrWhiteSpace(pinnedThumbprintRaw)
//        ? null
//        : pinnedThumbprintRaw.Replace(" ", "", StringComparison.Ordinal)
//                             .ToUpperInvariant();

//    return new HttpClientHandler
//    {
//        ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
//        {
//            // Only ever consider bypass logic for the single configured endpoint
//            bool hostMatches =
//                string.Equals(request.RequestUri.Host, allowedHost, StringComparison.OrdinalIgnoreCase) &&
//                (!allowedPort.HasValue || request.RequestUri.Port == allowedPort.Value);

//            // If the request isn't to our target, use default strict validation.
//            if (!hostMatches)
//                return errors == SslPolicyErrors.None;

//            // If there are no TLS errors, we're good.
//            if (errors == SslPolicyErrors.None)
//                return true;

//            // If we didn't configure pinning, do NOT bypass.
//            if (string.IsNullOrEmpty(pinnedThumbprint) || cert is null)
//                return false;

//            // Pin to the exact leaf cert (safer than "accept all").
//            bool thumbprintMatches =
//                string.Equals(cert.GetCertHashString()?.ToUpperInvariant(), pinnedThumbprint, StringComparison.Ordinal);

//            if (!thumbprintMatches)
//                return false;

//            // Leaf cert date check (don�t accept expired/not-yet-valid).
//            var now = DateTimeOffset.UtcNow;
//            if (now < cert.NotBefore || now > cert.NotAfter)
//                return false;

//            // Build chain but allow unknown root (useful for self-signed/private CA).
//            using var customChain = new X509Chain
//            {
//                ChainPolicy =
//                {
//                    RevocationMode = X509RevocationMode.NoCheck,
//                    VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
//                }
//            };

//            // If the only remaining problem is name mismatch or untrusted root, allow it because we pinned.
//            // (We already verified thumbprint + validity window.)
//            bool chainBuildOk = customChain.Build(cert);

//            // Accept when:
//            //  - chain builds OR (it fails only due to unknown root), AND
//            //  - we pinned the cert
//            // Name mismatch is implicitly tolerated due to pinning (IP vs SAN hostname).
//            if (chainBuildOk)
//                return true;

//            // If build failed, see if the failures are exclusively due to unknown root/intermediates.
//            bool onlyUnknownRootOrPartial =
//                customChain.ChainStatus.Length > 0 &&
//                Array.TrueForAll(customChain.ChainStatus, s =>
//                    s.Status == X509ChainStatusFlags.UntrustedRoot ||
//                    s.Status == X509ChainStatusFlags.PartialChain ||
//                    s.Status == X509ChainStatusFlags.RevocationStatusUnknown);

//            return onlyUnknownRootOrPartial;
//        }
//    };
//})
//.AddHttpMessageHandler<TokenHandler>();


//builder.Services.AddHttpClient<CWPClient>(client =>
//{
//    var baseUrl = builder.Configuration["Api:BaseUrl"]
//                  ?? throw new ArgumentNullException("Api:BaseUrl cannot be null");

//    client.BaseAddress = new Uri(baseUrl);
//    client.DefaultRequestHeaders.Accept.Add(
//        new MediaTypeWithQualityHeaderValue("application/json"));
//})
//.ConfigurePrimaryHttpMessageHandler(() =>
//{
//    var baseUrl = builder.Configuration["Api:BaseUrl"]
//                  ?? throw new ArgumentNullException("Api:BaseUrl cannot be null");
//    var uri = new Uri(baseUrl);
//    var allowedHost = uri.Host;                 // e.g. "151.242.28.16"
//    var allowedPort = uri.IsDefaultPort ? (int?)null : uri.Port;  // e.g. 44333
//    //var pinnedThumbprint = builder.Configuration["Api:PinnedThumbprint"]; // optional

//    return new HttpClientHandler
//    {
//        ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
//        {
//            // match host, and (if present) match port too
//            var hostMatches = string.Equals(request.RequestUri.Host, allowedHost, StringComparison.OrdinalIgnoreCase)
//                              && (!allowedPort.HasValue || request.RequestUri.Port == allowedPort.Value);

//            if (hostMatches)
//            {
//                // Dev-only fallback: trust this endpoint even with name/chain errors
//                return true;
//            }

//            // All other hosts use normal validation
//            return errors == SslPolicyErrors.None;
//        }
//    };
//})
//.AddHttpMessageHandler<TokenHandler>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = builder.Environment.IsDevelopment();
    })
    // Scanned pictures come back from the shop PC through the browser, so allow bigger messages.
    .AddHubOptions(options => options.MaximumReceiveMessageSize = 20 * 1024 * 1024);

var app = builder.Build();

// Use UK formatting everywhere so amounts show as £ whatever the server's regional settings are.
var ukCulture = new System.Globalization.CultureInfo("en-GB");
System.Globalization.CultureInfo.DefaultThreadCurrentCulture = ukCulture;
System.Globalization.CultureInfo.DefaultThreadCurrentUICulture = ukCulture;
app.UseRequestLocalization(new Microsoft.AspNetCore.Builder.RequestLocalizationOptions()
    .SetDefaultCulture("en-GB")
    .AddSupportedCultures("en-GB")
    .AddSupportedUICultures("en-GB"));

// Create the database if it does not exist, then apply pending EF Core migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CarWashAppDbContext>();
    var startupLogger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

    try
    {
        var existed = db.Database.CanConnect();
        startupLogger.LogInformation(existed
            ? "Database found. Applying any pending migrations."
            : "Database not found. Creating it and applying migrations.");

        // Migrate() creates the database when it is missing, then builds/updates the tables.
        db.Database.Migrate();
        startupLogger.LogInformation("Database is ready.");

        // Default logins (admin / staff) the first time the Staff table is empty.
        StaffService.SeedAsync(db, builder.Configuration).GetAwaiter().GetResult();
    }
    catch (Exception ex)
    {
        startupLogger.LogCritical(ex, "Could not create or update the database. Check the connection string and that the SQL login may create databases.");
        throw;
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Stream invoice files directly without loading into Blazor circuit
app.MapGet("/invoices/{fileName}", (string fileName, IConfiguration config) =>
{
    var root = CarWashApp.Services.InvoicePaths.Resolve(config);
    if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
    {
        return Results.NotFound();
    }

    var safeName = Path.GetFileName(fileName);
    var fullPath = Path.Combine(root, safeName);
    if (!File.Exists(fullPath))
    {
        return Results.NotFound();
    }

    var provider = new FileExtensionContentTypeProvider();
    if (!provider.TryGetContentType(fullPath, out var contentType))
    {
        contentType = "application/octet-stream";
    }

    return Results.File(fullPath, contentType, enableRangeProcessing: true);
});

app.Run();
