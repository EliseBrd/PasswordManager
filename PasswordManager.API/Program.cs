using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using PasswordManager.API.Context;
using PasswordManager.API.Middlewares;
using PasswordManager.API.Repositories;
using PasswordManager.API.Repositories.Interfaces;
using PasswordManager.API.Services;
using PasswordManager.API.Services.Interfaces;
using System.Text.Json.Serialization;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// --- Serilog Configuration ---
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    // Logs généraux (tout sauf EF Core)
    .WriteTo.File("Logs/app-.txt", 
        rollingInterval: RollingInterval.Day,
        restrictedToMinimumLevel: LogEventLevel.Information)
    // Logs spécifiques Base de Données (EF Core)
    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(e => e.Properties.ContainsKey("SourceContext") && 
                                     (e.Properties["SourceContext"].ToString().Contains("Microsoft.EntityFrameworkCore") || 
                                      e.Properties["SourceContext"].ToString().Contains("Microsoft.Data.Sqlite")))
        .WriteTo.File("Logs/db-.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// --- DbContext registration ---
builder.Services.AddDbContext<PasswordManagerDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Repositories & Services registration ---
builder.Services.AddScoped<IVaultRepository, VaultRepository>();
builder.Services.AddScoped<IVaultService, VaultService>();
builder.Services.AddScoped<IVaultEntryService, VaultEntryService>();
builder.Services.AddScoped<IPermissionService, PermissionService>();

builder.Services.AddScoped<IUserService, UserService>();

// --- Controllers & Swagger ---
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Configure JSON serializer to ignore cycles (e.g. Vault -> Creator -> Vaults -> ...)
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<EnsureUserMiddleware>();

app.MapControllers();

app.Run();
