using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PasswordManager.API.Objects;

namespace PasswordManager.API.Configurations
{
    public class VaultLogConfiguration : IEntityTypeConfiguration<VaultLog>
    {
        public void Configure(EntityTypeBuilder<VaultLog> entity)
        {
            entity.HasKey(e => e.Identifier);

            entity.Property(e => e.EncryptedData)
                .IsRequired()
                .HasMaxLength(4096); // Assez large pour contenir le JSON chiffré

            entity.HasOne(e => e.Vault)
                .WithMany(v => v.Logs)
                .HasForeignKey(e => e.VaultIdentifier)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
