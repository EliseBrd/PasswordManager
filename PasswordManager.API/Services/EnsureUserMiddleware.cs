using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.Graph;
using PasswordManager.API;
using Microsoft.Extensions.Logging;
using PasswordManager.API.Objects;


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

    public async Task InvokeAsync(HttpContext context, PasswordManagerDbContext db)
    {
        // Vérifie que l'utilisateur est authentifié
        if (context.User.Identity?.IsAuthenticated == true)
        {
            // Récupère le claim objectidentifier
            var objectIdClaim = context.User.FindFirst("http://schemas.microsoft.com/identity/claims/objectidentifier")?.Value;
            if (!string.IsNullOrEmpty(objectIdClaim))
            {
                Guid entraIdGuid = Guid.Parse(objectIdClaim);
                
                
                // Cherche l'utilisateur en base
                var user = await db.Users.FirstOrDefaultAsync(u => u.entraId == entraIdGuid);

                if (user == null)
                {
                    // Si pas trouvé, crée un nouvel utilisateur
                    user = new AppUser
                    {
                        Identifier = Guid.NewGuid(),
                        entraId = entraIdGuid
                    };
                    
                    db.Users.Add(user);
                    await db.SaveChangesAsync();
                    
                    _logger.LogInformation("Nouveau utilisateur créé : {entraId}", entraIdGuid);
                }
                else
                {
                    _logger.LogInformation("ℹUtilisateur chargé depuis la base de donnée : {entraId}", entraIdGuid);
                }

                // Optionnel : tu peux stocker l'user en Items pour y accéder dans les controllers
                context.Items["CurrentUser"] = user;
            }
        }

        // Passe au middleware suivant
        await _next(context);
    }
}