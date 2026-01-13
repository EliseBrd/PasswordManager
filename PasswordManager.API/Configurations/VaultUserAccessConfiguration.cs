using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.API.Objects;

namespace PasswordManager.API.Configurations
{
    public class VaultUserAccessConfiguration : IEntityTypeConfiguration<VaultUserAccess>
    {
        public void Configure(EntityTypeBuilder<VaultUserAccess> entity)
        {
            entity.HasKey(e => new { e.VaultIdentifier, e.UserIdentifier });

            entity.HasOne(e => e.Vault)
                .WithMany(v => v.UserAccesses)
                .HasForeignKey(e => e.VaultIdentifier)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.User)
                .WithMany(u => u.VaultAccesses)
                .HasForeignKey(e => e.UserIdentifier)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
