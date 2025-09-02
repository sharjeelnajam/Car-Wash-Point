using Blazored.Toast;
using CarWashApp;
using CarWashApp.Components;
using CarWashApp.Repositories;
using CarWashApp.Services;
using CarWashApp.Services.HttpClientServices;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Headers;
using System.Net.Security;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<CarWashAppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();

builder.Services.AddBlazoredToast();

builder.Services.AddTransient<TokenHandler>();

builder.Services.AddHttpClient<CWPClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
                  ?? throw new ArgumentNullException("Api:BaseUrl cannot be null");

    client.BaseAddress = new Uri(baseUrl);
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
})
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"]
                  ?? throw new ArgumentNullException("Api:BaseUrl cannot be null");
    var uri = new Uri(baseUrl);
    var allowedHost = uri.Host;                 // e.g. "151.242.28.16"
    var allowedPort = uri.IsDefaultPort ? (int?)null : uri.Port;  // e.g. 44333
    //var pinnedThumbprint = builder.Configuration["Api:PinnedThumbprint"]; // optional

    return new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback = (request, cert, chain, errors) =>
        {
            // match host, and (if present) match port too
            var hostMatches = string.Equals(request.RequestUri.Host, allowedHost, StringComparison.OrdinalIgnoreCase)
                              && (!allowedPort.HasValue || request.RequestUri.Port == allowedPort.Value);

            if (hostMatches)
            {
                // Dev-only fallback: trust this endpoint even with name/chain errors
                return true;
            }

            // All other hosts use normal validation
            return errors == SslPolicyErrors.None;
        }
    };
})
.AddHttpMessageHandler<TokenHandler>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

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
