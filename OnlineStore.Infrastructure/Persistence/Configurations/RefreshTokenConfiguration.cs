using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("refresh_tokens", tb =>
            {
                tb.HasComment("Refresh-токены пользователей (храним хеш).");
            });

            builder.HasKey(x => x.Id);

            builder.Property(x => x.TokenHash).IsRequired().HasMaxLength(256);
            builder.Property(x => x.CreatedAt).IsRequired();
            builder.Property(x => x.ExpiresAt).IsRequired();

            builder.Property(x => x.CreatedByIp).HasColumnName("created_by_ip").HasMaxLength(64);
            builder.Property(x => x.CreatedByUa).HasColumnName("created_by_ua").HasMaxLength(512);
            builder.Property(x => x.RevokedByIp).HasColumnName("revoked_by_ip").HasMaxLength(64);

            builder.Property(x => x.ReplacedByTokenId).HasColumnName("replaced_by_token_id");

            builder.HasIndex(x => x.TokenHash).IsUnique();

            builder.HasQueryFilter(rt => rt.User != null && !rt.User.IsDeleted);
            // У пользователя нет коллекции RefreshTokens — используем WithMany() без нав. свойства
            builder.HasOne(x => x.User)
             .WithMany()
             .HasForeignKey(x => x.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
