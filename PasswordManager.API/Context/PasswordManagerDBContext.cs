using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Configurations;
using PasswordManager.API.Objects;

namespace PasswordManager.API.Context;

public class PasswordManagerDBContext : DbContext
{
    public DbSet<AppUser> Users { get; set; }
    public DbSet<Vault> Vaults { get; set; }
    public DbSet<VaultEntry> VaultEntries { get; set; }


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

        // Ajouter un utilisateur factice pour les tests
        modelBuilder.Entity<AppUser>().HasData(
            new AppUser
            {
                Identifier = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                entraId = Guid.Parse("00000000-0000-0000-0000-000000000001")
            }
        );
    }
    //protected override void OnModelCreating(ModelBuilder modelBuilder)
    //{
    //    base.OnModelCreating(modelBuilder);

    //    // --- AppUser ---
    //    modelBuilder.Entity<AppUser>(entity =>
    //    {
    //        //Clef primaire
    //        entity.HasKey(e => e.Identifier);

    //        //Relation plusieurs à plusieurs entre AppUser et Vault
    //        entity.HasMany(e => e.SharedVaults).WithMany(e => e.SharedUsers);
    //    });

    //    // --- Vault ---
    //    modelBuilder.Entity<Vault>(entity =>
    //    {
    //        entity.HasKey(e => e.Identifier);

    //        // Relation 1→N : Vault → VaultEntries
    //        entity.HasMany(e => e.Entries).WithOne(e => e.Vault).HasForeignKey(e => e.VaultIdentifier).OnDelete(DeleteBehavior.Cascade);
    //        // Relation 1→N : AppUser (creator) → Vaults
    //        entity.HasOne(v => v.Creator).WithMany(u => u.Vaults).HasForeignKey(v => v.CreatorIdentifier).OnDelete(DeleteBehavior.Restrict);

    //        // Contraintes
    //        entity.Property(e => e.Name).HasMaxLength(100);

    //        // Index unique (même utilisateur ne peut pas avoir deux coffres du même nom)
    //        entity.HasIndex(e => new { e.CreatorIdentifier, e.Name }).IsUnique();
    //    });

    //    // --- VaultEntry ---
    //    modelBuilder.Entity<VaultEntry>(entity =>
    //    {
    //        entity.HasKey(e => e.Identifier);

    //        // Relation 1→N : AppUser (creator) → VaultEntries
    //        entity.HasOne(e => e.Creator).WithMany(u => u.Entries).HasForeignKey(e => e.CreatorIdentifier).OnDelete(DeleteBehavior.Restrict);
    //    });

    //}
}