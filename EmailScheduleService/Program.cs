using EmailScheduleService;
using TrunkMonkey.Shared.Data;

using Microsoft.EntityFrameworkCore;

using SendGrid.Extensions.DependencyInjection;

using TrunkMonkey.Shared.DTO;
using Serilog;
using TrunkMonkey.Shared.Repository;
using TrunkMonkey.Shared.Services;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("TrunkMonkeyConnection");
builder.Services.Configure<HostOptions>(config =>
{
    config.ServicesStartConcurrently = true;
    config.ServicesStopConcurrently = true;
    //config.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});
builder.Services.Configure<SmtpOptions>(builder.Configuration.GetSection(SmtpOptions.ConfigurationKey));
builder.Services.AddHostedService<Worker>();
builder.Services.AddDbContextFactory<TrunkMonkeyContext>(options =>
{
    options.UseSqlServer(connectionString);
});
builder.Services.AddSerilog(config =>
{
    config.MinimumLevel.Information();
    config.WriteTo.Console(outputTemplate: "[{Level:u3} {hh:MM:ss}]{NewLin}{Message:lj}{NewLine}{Exception}");
    config.WriteTo.MSSqlServer(connectionString, sinkOptions: new()
    {
        AutoCreateSqlTable = true,
        TableName = "ApplicationLogs"
    });
});
builder.Services.AddHttpClient("default", client => client.BaseAddress = new(builder.Configuration["DefaultUrl"]!));
builder.Services.AddScoped<HttpClient>(services => services.GetRequiredService<IHttpClientFactory>().CreateClient());
builder.Services.AddTransient<SmtpService>();
builder.Services.AddTransient<EmailRepository>();
builder.Services.AddTransient<ContactsRepository>();
builder.Services.AddTransient<SendListRepository>();
//builder.Services.AddSendGrid(options =>
//{
//    options.ApiKey = sendGridOptions.ApiKey;

//});

var host = builder.Build();
host.Run();
