using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.API.Objects;
using System.Reflection.Emit;

namespace PasswordManager.API.Configurations
{
    public class AppUserConfiguration : IEntityTypeConfiguration<AppUser>
    {
        public void Configure(EntityTypeBuilder<AppUser> entity)
        {
            // --- Clé primaire ---
            entity.HasKey(e => e.Identifier); //Clef primaire

            // --- Propriétés ---
            entity.Property(u => u.entraId)
                .IsRequired();

            // --- Relations ---
            entity.HasMany(u => u.Vaults).WithOne(v => v.Creator).HasForeignKey(v => v.CreatorIdentifier);

            entity.HasMany(u => u.Entries).WithOne(e => e.Creator).HasForeignKey(e => e.CreatorIdentifier);
        }
    }
}
