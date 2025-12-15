using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Objects;

namespace PasswordManager.API;

public class PasswordManagerDbContext : DbContext
{
    public DbSet<AppUser> Users { get; set; } = default!;
    public DbSet<Vault> Vaults { get; set; } = default!;
    public DbSet<VaultEntry> VaultEntries { get; set; } = default!;

    public PasswordManagerDbContext() : base()
    {
    }

    public PasswordManagerDbContext(DbContextOptions<PasswordManagerDbContext> options) : base(options)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        //https://learn.microsoft.com/fr-fr/ef/core/cli/dotnet
        //Pour installer dotnet ef en ligne de commande : dotnet tool install --global dotnet-ef

        if (optionsBuilder.IsConfigured == false)
        {
            optionsBuilder.UseSqlite("Data Source=Data/passwordManager.db");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<AppUser>(entity =>
        {
            //Clef primaire
            entity.HasKey(e => e.Identifier);
            //Relation plusieurs à plusieurs entre AppUser et Vault
            entity.HasMany(e => e.SharedVaults).WithMany(e => e.SharedUsers);
        });

        modelBuilder.Entity<Vault>(entity =>
        {
            entity.HasKey(e => e.Identifier);
            //Relation 1 à plusieurs entre Vault et VaultEntry
            entity.HasMany(e => e.Entries).WithOne(e => e.Vault).HasForeignKey(e => e.VaultIdentifier);
            entity.HasOne(e => e.Creator).WithMany(e => e.Vaults).HasForeignKey(e => e.CreatorIdentifier);

            entity.Property(e => e.Name).HasMaxLength(100);

            //Exemple d'un index unique sur le nom du vault par utilisateur
            entity.HasIndex(e => new { e.CreatorIdentifier, e.Name }).IsUnique();
        });

        modelBuilder.Entity<VaultEntry>(entity =>
        {
            entity.HasKey(e => e.Identifier);

            //entity.Property(e => e.Username).HasMaxLength(100);
            //entity.Property(e => e.HashPassword).HasMaxLength(100);
        });

    }
}