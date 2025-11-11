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

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<CarWashAppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();
builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
builder.Services.AddScoped<ITransactionService, TransactionService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped(typeof(InvoiceService));
builder.Services.AddSingleton<ScannerService>();
builder.Services.AddBlazoredToast();

builder.Services.AddTransient<TokenHandler>();


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





HttpClientHandler BuildPinnedHandler(IConfiguration cfg)
{
    var baseUrl = cfg["Auth:BaseUrl"] ?? throw new ArgumentNullException("Api:BaseUrl");
    var uri = new Uri(baseUrl, UriKind.Absolute);
    var allowedHost = uri.Host;
    var allowedPort = uri.IsDefaultPort ? (int?)null : uri.Port;

    var pinnedThumbprint = (cfg["Auth:PinnedThumbprint"] ?? "")
        .Replace(" ", "").Replace(":", "").ToUpperInvariant();

    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (req, cert, chain, errors) =>
        {
            // Only your configured host[:port]
            bool hostMatches =
                string.Equals(req.RequestUri.Host, allowedHost, StringComparison.OrdinalIgnoreCase) &&
                (!allowedPort.HasValue || req.RequestUri.Port == allowedPort.Value);

            if (hostMatches)
            {
                return true; // <--- force trust for your test server
            }

            if (!hostMatches) return errors == SslPolicyErrors.None; // normal validation elsewhere
            if (errors == SslPolicyErrors.None) return true;         // already valid

            if (cert is null || string.IsNullOrEmpty(pinnedThumbprint)) return false;

            // Pin **leaf** cert
            var leaf = cert.GetCertHashString()?.Replace(" ", "").Replace(":", "").ToUpperInvariant();
            if (!string.Equals(leaf, pinnedThumbprint, StringComparison.Ordinal)) return false;

            // Time validity
            var now = DateTimeOffset.UtcNow;
            if (now < cert.NotBefore || now > cert.NotAfter) return false;

            // Allow private/self-signed chain when pinned
            using var x = new X509Chain
            {
                ChainPolicy =
                {
                    RevocationMode = X509RevocationMode.NoCheck,
                    VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority
                }
            };
            if (x.Build(cert)) return true;

            // Accept only typical private CA issues
            return x.ChainStatus.Length > 0 &&
                   Array.TrueForAll(x.ChainStatus, s =>
                       s.Status == X509ChainStatusFlags.UntrustedRoot ||
                       s.Status == X509ChainStatusFlags.PartialChain ||
                       s.Status == X509ChainStatusFlags.RevocationStatusUnknown);
        }
    };
}

// Named client for token endpoint (used by TokenHandler)
builder.Services.AddHttpClient("AuthClient", (sp, c) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    c.BaseAddress = new Uri(cfg["Auth:BaseUrl"]!, UriKind.Absolute);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(sp => BuildPinnedHandler(sp.GetRequiredService<IConfiguration>()));

// Your main API client
builder.Services.AddHttpClient<CWPClient>((sp, c) =>
{
    var cfg = sp.GetRequiredService<IConfiguration>();
    c.BaseAddress = new Uri(cfg["Auth:BaseUrl"]!, UriKind.Absolute);
    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(sp => BuildPinnedHandler(sp.GetRequiredService<IConfiguration>()))
.AddHttpMessageHandler<TokenHandler>();



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
        options.DetailedErrors = true;   // <-- turn on while debugging
    });

var app = builder.Build();

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

app.Run();
