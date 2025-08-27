using Blazored.Toast;
using CarWashApp;
using CarWashApp.Components;
using CarWashApp.Repositories;
using CarWashApp.Services;
using CarWashApp.Services.HttpClientServices;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddDbContext<CarWashAppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Connection")));
builder.Services.AddScoped<ICarRepository, CarRepository>();
builder.Services.AddScoped<ICarService, CarService>();

builder.Services.AddBlazoredToast();

builder.Services.AddHttpClient<CWPClient>(client =>
{
    var baseUrl = builder.Configuration["Api:BaseUrl"] ?? throw new ArgumentNullException("BaseUrl cannot be null");
    client.BaseAddress = new Uri(baseUrl);
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
