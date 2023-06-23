using System.Security.Claims;
using ChatGpt.Areas.Identity;
using ChatGpt.Data;
using ChatGpt.Hubs;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<MessagingContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<MessagingContext>();
builder.Services.AddSignalR();

builder.Services.AddIdentityServer(options => { options.IssuerUri = "https://chatgpt.local"; })
    .AddInMemoryClients(new[]
    {
        new Client
        {
            ClientId = "client",
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris = { "https://localhost:5002/signin-oidc" },
            PostLogoutRedirectUris = { "https://localhost:5002/signout-callback-oidc" },
            FrontChannelLogoutUri = "https://localhost:5002/signout-oidc",
            AllowedScopes = { "openid", "profile", "email", "phone" },
            RequireClientSecret = false,
            RequirePkce = false,
            AllowOfflineAccess = true
        }
    })
    .AddInMemoryIdentityResources(new IdentityResource[]
    {
        new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email(),
        new IdentityResources.Phone()
    })
    .AddAspNetIdentity<IdentityUser>();

builder.Services.AddAuthentication()
    .AddJwtBearer(builder =>
    {
        builder.Authority = "https://localhost:7069";
        builder.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            NameClaimType = ClaimTypes.NameIdentifier,
            ValidIssuer = "https://chatgpt.local"
        };
    });

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services
    .AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseIdentityServer();

app.UseAuthorization();

app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHub<NotificationHub>("/notifications");

app.Run();