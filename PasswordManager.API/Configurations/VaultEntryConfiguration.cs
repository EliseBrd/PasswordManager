using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.API.Objects;

namespace PasswordManager.API.Configurations
{
    public class VaultEntryConfiguration : IEntityTypeConfiguration<VaultEntry>
    {
        public void Configure(EntityTypeBuilder<VaultEntry> entity)
        {
            // --- Clé primaire ---
            entity.HasKey(e => e.Identifier);

            // --- Propriétés ---
            entity.Property(e => e.CypherPassword)
                .IsRequired()
                .HasMaxLength(512);

            entity.Property(e => e.CypherData)
                .IsRequired()
                .HasMaxLength(2048);

            entity.Property(e => e.IVPassword)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.IVData)
                .IsRequired()
                .HasMaxLength(128);

            entity.Property(e => e.TagPasswords).HasMaxLength(256);
            entity.Property(e => e.TagData).HasMaxLength(256);

            // --- Relations ---
            // Relation 1→N : Vault → VaultEntries
            entity.HasOne(e => e.Vault).WithMany(v => v.Entries).HasForeignKey(e => e.VaultIdentifier);

            // Relation 1→N : AppUser (creator) → VaultEntries
            entity.HasOne(e => e.Creator).WithMany(u => u.Entries).HasForeignKey(e => e.CreatorIdentifier).OnDelete(DeleteBehavior.Restrict);


            // --- Index --- si on veut accélérer les recherches
            entity.HasIndex(e => e.VaultIdentifier);
        }
    }
}
