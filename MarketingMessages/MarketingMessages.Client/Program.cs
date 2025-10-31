using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Serilog;
using Serilog.Exceptions;
using Serilog.Sinks.SystemConsole;
using Serilog.Sinks.MSSqlServer;
using MarketingMessages.Shared.Extensions;
using MarketingMessages.Shared.Services;
using MarketingMessages.Client.Services;
using MarketingMessages.Shared.Services.LoadingService;
using MarketingMessages.Client.Services.HttpService;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
var logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .Enrich.WithExceptionDetails()
    .CreateLogger();
Log.Logger = logger;

builder.Services.AddAuthorizationCore(options =>
{
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
});
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthenticationStateDeserialization();
builder.Services.AddHttpClient("default", client => client.BaseAddress = new(builder.HostEnvironment.BaseAddress));
builder.Services.AddScoped<HttpClient>(services => services.GetRequiredService<IHttpClientFactory>().CreateClient("default"));
builder.Services.AddCommonServices();

// UI
builder.Services.AddSingleton<ILoadingService, LoadingService>();

// HTTP
builder.Services.AddTransient<IHTTPService, HTTPService>();
builder.Services.AddTransient<ContentService>();
builder.Services.AddTransient<AudienceService>();
builder.Services.AddTransient<CampaignService>();
builder.Services.AddTransient<UnsubscribeGroupService>();

await builder.Build().RunAsync();
