using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using PasswordManager.API;
using PasswordManager.API.Services;

var builder = WebApplication.CreateBuilder(args);

//charge le contexte de la db comme context principale
builder.Services.AddDbContext<PasswordManagerDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));


string? corsFrontEndpoint = builder.Configuration.GetValue<string>("CorsFrontEndpoint");

//CORS permet d'autoriser le front � appeler l'API.
if (string.IsNullOrWhiteSpace(corsFrontEndpoint) == false)
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("WebAssemblyOrigin", policy =>
        {
            policy
                .WithOrigins(corsFrontEndpoint)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        });
    });
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization();
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//Ajout la couche d'authentification
app.UseAuthentication();

//Ajoute la couche d'autorisation
app.UseAuthorization();

//Middleware pour connecter automatiquement le user depuis la base de données 
app.UseMiddleware<EnsureUserMiddleware>();

//add the Core policy configuration
app.UseCors("WebAssemblyOrigin");

//Indique de cr�er les routes pour les Controlles.
app.MapControllers();

app.Run();
