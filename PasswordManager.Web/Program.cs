using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;
using PasswordManager.Web.Components;
using PasswordManager.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Blazor Components ---
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var apiEndpoint = builder.Configuration.GetValue<string>("WebAPI:Endpoint") ??
                  throw new InvalidOperationException("WebAPI is not configured");
var apiScope = builder.Configuration.GetValue<string>("WebAPI:Scope") ??
               throw new InvalidOperationException("WebAPI is not configured");

// --- 🔐 Authentification Microsoft Entra ID ---
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration, "AzureAd")
    .EnableTokenAcquisitionToCallDownstreamApi([apiScope])
    .AddDownstreamApi("PasswordManager.api", options =>
    {
        options.BaseUrl = apiEndpoint;
        options.Scopes = [apiScope];
    })
    .AddInMemoryTokenCaches();

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

// --- Application Services ---
builder.Services.AddScoped<VaultService>();


var app = builder.Build();

// --- Pipeline HTTP ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapGroup("/authentication").MapLoginAndLogout();

app.Run();