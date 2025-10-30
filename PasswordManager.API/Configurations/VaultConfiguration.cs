using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.API.Objects;

namespace PasswordManager.API.Configurations
{
    public class VaultConfiguration : IEntityTypeConfiguration<Vault>
    {
        public void Configure(EntityTypeBuilder<Vault> entity)
        {
            // --- Clé primaire ---
            entity.HasKey(e => e.Identifier);

            // --- Propriétés ---
            entity.Property(v => v.Name)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(v => v.MasterSalt)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(v => v.Salt)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(v => v.encryptKey)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(v => v.Password)
                .IsRequired()
                .HasMaxLength(512);

            // --- Relations ---
            // Relation 1→N : Vault → VaultEntries
            entity.HasMany(e => e.Entries)
                .WithOne(e => e.Vault)
                .HasForeignKey(e => e.VaultIdentifier)
                .OnDelete(DeleteBehavior.Cascade);

            // Relation 1→N : AppUser (creator) → Vaults
            entity.HasOne(v => v.Creator)
                .WithMany(u => u.Vaults)
                .HasForeignKey(v => v.CreatorIdentifier)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation N→N : SharedUsers → SharedVaults (plusieurs users peut avoir plusieurs vaults
            entity.HasMany(v => v.SharedUsers)
                .WithMany(u => u.SharedVaults);

            // Index unique (même utilisateur ne peut pas avoir deux coffres du même nom)
            entity.HasIndex(e => new { e.CreatorIdentifier, e.Name })
                .IsUnique();
        }
    }
}
