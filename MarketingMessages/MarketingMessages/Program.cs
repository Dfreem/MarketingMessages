var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("MarketingMessagesConnection") ?? throw new InvalidOperationException("Connection string 'MarketingMessagesConnection' not found.");
var userStoreConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
var httpBaseUrl = builder.Configuration["HttpBaseUrl"] ?? throw new InvalidOperationException("Required configuration value missing 'HttpBaseUrl'");


#region Data & Identity

builder.Services.AddDbContextFactory<MarketingMessagesContext>(options =>
        options.UseSqlServer(connectionString));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(userStoreConnection));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy =>
    {
        policy.RequireRole("Admin");
    });

builder.Services.AddIdentityCore<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationClaimsPrincipalFactory>();
#endregion


var server = builder.Configuration["MailServer"] ?? throw new InvalidOperationException("Unable to find MailServer in Configuration");
var port = builder.Configuration["MailPort"] ?? throw new InvalidOperationException("Unable to find MailPort in Configuration");
var username = builder.Configuration["MailUsername"] ?? throw new InvalidOperationException("Unable to find MailUsername in Configuration");
var password = builder.Configuration["MailPassword"] ?? throw new InvalidOperationException("Unable to find MailPassword in Configuration");
builder.Services.Configure<SmtpOptions>(ops =>
{
    ops.Server = server;
    ops.Port = Convert.ToInt32(port);
    ops.Username = username;
    ops.Password = password;
});

builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization();
builder.Services.AddControllers();

#region Logging
//builder.Logging.ClearProviders();
if (!builder.Environment.IsDevelopment())
{

    var logger = new LoggerConfiguration()
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}", restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
        .WriteTo.MSSqlServer(connectionString, new MSSqlServerSinkOptions()
        {
            AutoCreateSqlTable = true,
            TableName = "ApplicationLogs"
        }, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
        .CreateLogger();
    Log.Logger = logger;
    builder.Services.AddSerilog(logger);
    builder.Logging.AddSerilog(logger);
}
else
{

    var logger = new LoggerConfiguration()
        .WriteTo.MSSqlServer(connectionString, new MSSqlServerSinkOptions()
        {
            AutoCreateSqlTable = true,
            TableName = "ApplicationLogs",
        }, restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
        .CreateLogger();
    Log.Logger = logger;
    builder.Logging.AddSerilog(logger);
    builder.Services.AddSerilog(logger);
}

#endregion

#region OpenApi
builder.Services.AddOpenApiDocument();

#endregion

#region Scoped Services
builder.Services.AddHttpClient("default", client => client.BaseAddress = new(httpBaseUrl));
builder.Services.AddScoped<HttpClient>(services => services.GetRequiredService<IHttpClientFactory>().CreateClient("default"));
builder.Services.AddTransient<IHTTPService, HTTPService>();
builder.Services.AddTransient<ContentService>();
builder.Services.AddSingleton<IEmailSender<ApplicationUser>, IdentityNoOpEmailSender>();
builder.Services.AddTransient<ContactsRepository>();
builder.Services.AddTransient<EmailRepository>();
builder.Services.AddTransient<AudienceRepository>();
builder.Services.AddTransient<AnalyticsRepository>();
builder.Services.AddTransient<SuppressionGroupService>();
builder.Services.AddTransient<SmtpService>();
builder.Services.AddCommonServices();
//builder.Services.AddSendGrid(options =>
//{
//    options.ApiKey = builder.Configuration.GetRequiredSection("SendGridApi")["ApiKey"];
//});
#endregion

#region Hosted Services
builder.Services.Configure<HostOptions>(config =>
{
    config.ServicesStartConcurrently = true;
    config.ServicesStopConcurrently = true;
    config.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

if (Convert.ToBoolean(builder.Configuration["EnableWorker"]))
{
    Log.Information("EnableWorker is true, adding hosted service");
    builder.Services.AddHostedService<EmailQueueWorker>();
}
if (builder.Environment.IsDevelopment())
{
    //builder.Services.AddHostedService<WebhookTestWorker>();
}
builder.Services.AddHostedService<LogTableMaintananceWorker>();
#endregion

#region SignalR
builder.Services.AddSignalR();

if (!builder.Environment.IsDevelopment())
{

    builder.Services.AddResponseCompression(opts =>
    {
        opts.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
            ["application/octet-stream"]);
    });
}

#endregion

//#region Sessions

//builder.Services.AddDataProtection()
//    .PersistKeysToDbContext<MarketingMessagesContext>();
//builder.Services.AddSession(options =>
//{
//    options.
//})

//#endregion

builder.Services.AddAntiforgery();
var app = builder.Build();
// Seed development Sender, ignore if running migrations from a command line
if (args.All(a => a != "ef"))
{
    using (var scope = app.Services.CreateAsyncScope())
    {
        var ctx = scope.ServiceProvider.GetRequiredService<MarketingMessagesContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        await SeedData.Seed(ctx, userManager, roleManager);
    }
}

//app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
    app.UseMigrationsEndPoint();
}
if (!app.Environment.IsProduction()) // Beta/Staging & Development
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}
else
{
    app.UseResponseCompression();
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
    app.UseMigrationsEndPoint();
}

app.UseHttpsRedirection();


app.UseAntiforgery();
//app.MapStaticAssets();
app.UseStaticFiles();
app.MapHub<NotificationHub>("/Notifications");
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(MarketingMessages.Client._Imports).Assembly);
app.MapControllers();
// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

app.Run();
