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

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();

// --- DbContext registration ---
builder.Services.AddDbContext<PasswordManagerDBContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Repositories & Services registration ---
builder.Services.AddScoped<IVaultRepository, VaultRepository>();
builder.Services.AddScoped<IVaultService, VaultService>();
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
