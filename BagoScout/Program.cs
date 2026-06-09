using BagoScout.Data;
using BagoScout.Models;
using BagoScout.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder("JwtCookieOrBearer")
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
})
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
});

// Configure Custom JWT Authentication Scheme (JwtCookieOrBearer)
builder.Services.AddAuthentication("JwtCookieOrBearer")
    .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, BagoScout.Services.JwtAuthenticationHandler>("JwtCookieOrBearer", null);

// Add CORS for mobile app
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.None; // Allow HTTP for deployment
});

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configure Email Settings
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));

// Register Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Log email config on startup for diagnostics (masks password)
var emailSection = builder.Configuration.GetSection("EmailSettings");
Console.WriteLine($"[EmailConfig] Server={emailSection["SmtpServer"]} Port={emailSection["SmtpPort"]} From={emailSection["SenderEmail"]} User={emailSection["Username"]} HasPassword={!string.IsNullOrEmpty(emailSection["Password"])}");

// Kestrel Environment Configuration
if (builder.Environment.IsDevelopment())
{
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5180); // Local HTTP
        options.ListenAnyIP(7030, listenOptions => listenOptions.UseHttps()); // Local HTTPS
    });
}
else
{
    // Production (Railway) environments will bind cleanly to port 8080 over HTTP
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(8080);
    });
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// REMOVED: app.UseHttpsRedirection() from production to prevent proxy redirect loops.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Enable serving .apk files for Android app download
var contentTypeProvider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
contentTypeProvider.Mappings[".apk"] = "application/vnd.android.package-archive";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = contentTypeProvider
});

app.UseRouting();

// CORS should be placed after UseRouting but before UseSession/UseAuthentication
app.UseCors();

app.UseSession();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();