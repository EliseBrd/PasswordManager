using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Configurations;
using PasswordManager.API.Objects;

namespace PasswordManager.API.Context;

public class PasswordManagerDBContext : DbContext
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Vault> Vaults { get; set; }
    public DbSet<VaultEntry> VaultEntries { get; set; }
    public DbSet<VaultUserAccess> VaultUserAccesses { get; set; }
    public DbSet<VaultLog> VaultLogs { get; set; }


    public PasswordManagerDBContext(DbContextOptions<PasswordManagerDBContext> options) : base(options)
    {

    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //https://learn.microsoft.com/fr-fr/ef/core/cli/dotnet
        //Pour installer dotnet ef en ligne de commande : dotnet tool install --global dotnet-ef

        if (optionsBuilder.IsConfigured == false)
        {
            optionsBuilder.UseSqlite("Data Source=passwordManager.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // applique les conventions par défaut (par ex. les noms de tables, clés primaires implicites, etc.), et ensuite j’ajoute mes configurations personnalisées

        modelBuilder.ApplyConfiguration(new AppUserConfiguration());
        modelBuilder.ApplyConfiguration(new VaultConfiguration());
        modelBuilder.ApplyConfiguration(new VaultEntryConfiguration());
        modelBuilder.ApplyConfiguration(new VaultUserAccessConfiguration());
        modelBuilder.ApplyConfiguration(new VaultLogConfiguration());

        // Ajouter un utilisateur factice pour les tests
        modelBuilder.Entity<AppUser>().HasData(
            new AppUser
            {
                Identifier = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                entraId = Guid.Parse("00000000-0000-0000-0000-000000000001")
            }
        );
    }
}