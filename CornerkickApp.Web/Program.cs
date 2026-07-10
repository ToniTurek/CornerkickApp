using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;
using CornerkickApp.Shared.Services;
using CornerkickApp.Web.Components;
using CornerkickApp.Web.Components.Account;
using CornerkickApp.Web.Data;
using CornerkickApp.Web.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Radzen;
using SixLabors.ImageSharp;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Add device-specific services used by the CornerkickApp.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IAmazonS3Service, AmazonS3Service>();

// Add Auth services used by the Web app
builder.Services.AddAuthentication(options =>
{
    // Ensure that unauthenticated clients redirect to the login page rather than receive a 401 by default.
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
});

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityUserAccessor>();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

/*
// Add Authorize convention
builder.Services.AddRazorPages(
  options => {
    options.Conventions.AuthorizePage("/member/desk");
    options.Conventions.AuthorizeFolder("/member");
    //options.Conventions.AllowAnonymousToPage("/Private/PublicPage");
    //options.Conventions.AllowAnonymousToFolder("/Private/PublicPages");
  });
*/

string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
connectionString = Environment.GetEnvironmentVariable("SQLSERVER_URI");
if (string.IsNullOrEmpty(connectionString)) connectionString = builder.Configuration.GetConnectionString("SQLSERVER_URI");
#if !DEBUG
#endif

builder.Services.AddDbContext<CornerkickAppContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

//Needed for external clients to log in
builder.Services.AddIdentityApiEndpoints<CornerkickAppUser>(options =>
{
#if DEBUG
  options.SignIn.RequireConfirmedAccount = false;
#else
  options.SignIn.RequireConfirmedAccount = true;
#endif

  // Customize password requirements
  options.Password.RequireDigit = false;
  options.Password.RequiredLength = 4; // minimum length
  options.Password.RequireNonAlphanumeric = false;
  options.Password.RequireUppercase = false;
  options.Password.RequireLowercase = false;
  options.Password.RequiredUniqueChars = 1; // Require unique characters
})
.AddEntityFrameworkStores<CornerkickAppContext>();

builder.Services.AddSingleton<IEmailSender<CornerkickAppUser>, IdentityNoOpEmailSender>();

builder.Services.AddScoped<CornerkickApp.Controllers.Shared.MyAuthenticationStateProvider>();
builder.Services.AddScoped<CornerkickApp.Controllers.App.TriggerService>();
builder.Services.AddSingleton<CornerkickApp.Controllers.Shared.Components.Headline.HeadlineController>();

// Configure e-mail service
builder.Services.AddTransient<CkEmailSender>();

// Radzen
/*
builder.Services.AddScoped<DialogService>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<TooltipService>();
builder.Services.AddScoped<ContextMenuService>();
*/
builder.Services.AddRadzenComponents();

builder.Services
    .AddBlazorise(options => {
      options.Immediate = true;
    })
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

builder.Services.AddHttpClient();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*
builder.Services.Configure<IdentityOptions>(options =>
    options.ClaimsIdentity.UserIdClaimType = ClaimTypes.NameIdentifier);
*/

builder.Services.AddAuthorizationCore();
builder.Services.AddApiAuthorization();
/*
builder.Services.AddAuthorization(options => {
  options.FallbackPolicy = new AuthorizationPolicyBuilder()
      .RequireAuthenticatedUser()
      .Build();
});
*/

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Apply migrations & create database if needed at startup
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<CornerkickAppContext>();
        dbContext.Database.Migrate();
    }
    app.UseMigrationsEndPoint();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

//app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(typeof(CornerkickApp.Components._Imports).Assembly)
    .AddAdditionalAssemblies(typeof(CornerkickApp.Shared.Pages.Home).Assembly);

// Needed for external clients to log in
app.MapGroup("/identity").MapIdentityApi<CornerkickAppUser>();
// Needed for Identity Blazor components
app.MapAdditionalIdentityEndpoints();

/*
//Add the weather API endpoint and require authorization
app.MapGet("/api/weather", async (IWeatherService weatherService) =>
{
    var forecasts = await weatherService.GetWeatherForecastsAsync();
    return Results.Ok(forecasts);
}).RequireAuthorization();
*/

//Add the as3 API endpoint and require authorization
app.MapGet("/api/as3", async (IAmazonS3Service as3service) => {
  var as3 = await as3service.GetAmazonS3CredentialsAsync();
  return Results.Ok(as3);
}).RequireAuthorization();

//app.MapGet("/api/member/desk").RequireAuthorization();

// Get cornerkick root directory
string? sRootPath = AppDomain.CurrentDomain.BaseDirectory;
if (string.IsNullOrEmpty(sRootPath)) sRootPath = Environment.GetEnvironmentVariable("ckRootPath");
if (string.IsNullOrEmpty(sRootPath)) sRootPath = builder.Configuration.GetSection("ckRootPath").Value;
if (string.IsNullOrEmpty(sRootPath)) sRootPath = ".";

// Get cornerkick instance name
string? sCkInstanceName = Environment.GetEnvironmentVariable("ckInstanceName");
if (string.IsNullOrEmpty(sCkInstanceName)) sCkInstanceName = builder.Configuration.GetSection("ckInstanceName").Value;
if (string.IsNullOrEmpty(sCkInstanceName)) sCkInstanceName = "";
CornerkickApp.Shared.Models.CkAppShared.sCkInstanceName = sCkInstanceName == null ? "" : sCkInstanceName;

// Set ck home directory
string sHomeDir = builder.Environment.ContentRootPath;
string? sDeployOnHost = Environment.GetEnvironmentVariable("_DEPLOY_ON_HOST");
if (!string.IsNullOrEmpty(sDeployOnHost) && sDeployOnHost.Equals("true", StringComparison.OrdinalIgnoreCase)) {
  sHomeDir = Path.Combine(sHomeDir, "wwwroot", "_content", "cornerkickapp.components");
} else {
  sHomeDir = Path.Combine(sHomeDir, "..", "CornerkickApp.Components", "wwwroot");
}

// Compose App_Data dir in Cornerkick.Components
string sAppDataDir = Path.Combine(sHomeDir, "Content", "Uploads");

// Start Cornerkick
CornerkickApp.Controllers.App appCk = new CornerkickApp.Controllers.App(
  builder.Configuration,
  sRootPath,
  sAppDataDir
);
appCk.start();

// Set Version
CornerkickApp.Shared.Models.CkAppShared.sVersion = CornerkickApp.Controllers.App.Version;

app.Run();
