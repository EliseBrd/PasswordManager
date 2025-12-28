using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Graph;
using PasswordManager.API;
using Microsoft.Extensions.Logging;
using PasswordManager.API.Objects;
using PasswordManager.API.Context;

namespace PasswordManager.API.Services;

public class EnsureUserMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<EnsureUserMiddleware> _logger;

    public EnsureUserMiddleware(RequestDelegate next, ILogger<EnsureUserMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Resolve the DbContext from the request's service provider
        var db = context.RequestServices.GetRequiredService<PasswordManagerDBContext>();

        // Vérifie que l'utilisateur est authentifié
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Récupère le claim objectidentifier
            var objectIdClaim = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (!string.IsNullOrEmpty(objectIdClaim))
            {
                Guid entraIdGuid = Guid.Parse(objectIdClaim);
                
                // Récupère l'email (preferred_username est souvent le plus fiable pour Entra ID, sinon Name ou Email)
                var email = context.User.FindFirst("preferred_username")?.Value 
                            ?? context.User.FindFirst(ClaimTypes.Email)?.Value 
                            ?? context.User.Identity.Name 
                            ?? string.Empty;

                // Cherche l'utilisateur en base
                var user = await db.Users.FirstOrDefaultAsync(u => u.entraId == entraIdGuid);

                if (user == null)
                {
                    // Si pas trouvé, crée un nouvel utilisateur
                    user = new AppUser
                    {
                        Identifier = Guid.NewGuid(),
                        entraId = entraIdGuid,
                        Email = email
                    };
                    
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                    
                    _logger.LogInformation("Nouveau utilisateur créé : {entraId} ({email})", entraIdGuid, email);
                }
                else
                {
                    // Met à jour l'email si nécessaire
                    if (user.Email != email && !string.IsNullOrEmpty(email))
                    {
                        user.Email = email;
                        await db.SaveChangesAsync();
                    }
                    _logger.LogInformation("ℹUtilisateur chargé depuis la base de donnée : {entraId}", entraIdGuid);
                }

                context.Items["CurrentUser"] = user;
            }
        }

        // Passe au middleware suivant
        await _next(context);
    }
}
