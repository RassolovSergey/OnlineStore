using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> b)
        {
            b.ToTable("refresh_tokens", tb =>
            {
                tb.HasComment("Refresh-токены пользователей (храним хеш).");
            });

            b.HasKey(x => x.Id);

            b.Property(x => x.TokenHash).IsRequired().HasMaxLength(256);
            b.Property(x => x.CreatedAt).IsRequired();
            b.Property(x => x.ExpiresAt).IsRequired();

            b.Property(x => x.CreatedByIp).HasColumnName("created_by_ip").HasMaxLength(64);
            b.Property(x => x.CreatedByUa).HasColumnName("created_by_ua").HasMaxLength(512);
            b.Property(x => x.RevokedByIp).HasColumnName("revoked_by_ip").HasMaxLength(64);

            b.Property(x => x.ReplacedByTokenId).HasColumnName("replaced_by_token_id");

            b.HasIndex(x => x.TokenHash).IsUnique();

            // У пользователя нет коллекции RefreshTokens — используем WithMany() без нав. свойства
            b.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
