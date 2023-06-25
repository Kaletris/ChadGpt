using System.Reflection;
using System.Security.Claims;
using ChatGpt.Areas.Identity;
using ChatGpt.Data;
using ChatGpt.Hubs;
using ChatGpt.Services;
using ChatGpt.Swagger;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ??
                       throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<MessagingContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<MessagingContext>();
builder.Services.AddSignalR();

builder.Services.AddIdentityServer(options => { options.IssuerUri = "https://chatgpt.local"; })
    .AddInMemoryClients(new[]
    {
        new Client
        {
            ClientId = "client",
            AllowedGrantTypes = GrantTypes.Code,
            RedirectUris =
                { "https://localhost:5002/signin-oidc", "https://localhost:7069/swagger/oauth2-redirect.html" },
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

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        "Admin",
        policy => policy
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .RequireClaim(ClaimTypes.Role, "admin")
            .Build());
});

builder.Services.AddHealthChecks()
    .AddDbContextCheck<MessagingContext>();

builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services
    .AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<IdentityUser>>();

builder.Services.AddGrpc();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "ChadGpt", Version = "v1" });
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            AuthorizationCode = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri("https://localhost:7069/connect/authorize"),
                TokenUrl = new Uri("https://localhost:7069/connect/token"),
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "OpenId" }
                }
            }
        }
    });

    var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

    options.OperationFilter<AuthorizeCheckOperationFilter>();
});

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGrpcService<ThreadsService>();
app.MapControllers();
app.MapBlazorHub();
app.MapFallbackToPage("/_Host");
app.MapHub<NotificationHub>("/notifications");
app.MapHealthChecks("/healthz");

using (var scope = app.Services.CreateScope())
{
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var admin = await userManager.FindByNameAsync("admin");
    if (admin == null)
    {
        admin = new IdentityUser
        {
            // Username must match email, because the builtin login page uses the email as username
            UserName = "admin@asd.asd",
            Email = "admin@asd.asd"
        };
        await userManager.CreateAsync(admin, "Admin123.");
        await roleManager.CreateAsync(new IdentityRole("admin"));
        await userManager.AddToRoleAsync(admin, "admin");
    }
}

app.Run();