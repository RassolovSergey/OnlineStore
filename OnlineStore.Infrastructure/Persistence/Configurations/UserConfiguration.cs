using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users", tb =>
            {
                tb.HasComment("Пользователи интернет-магазина.");
            });

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                .HasColumnName("email")
                .HasMaxLength(255).
                IsRequired();

            builder.Property(u => u.NormalizedEmail)
                .HasColumnName("normalized_email")
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(u => u.PasswordHash)
                .HasColumnName("password_hash")
                .HasMaxLength(500)
                .IsRequired();

            builder.Property(u => u.CreatedAt)
                .HasColumnName("created_at")
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            builder.Property(u => u.IsAdmin)
                .HasColumnName("is_admin")
                    .HasDefaultValue(false);

            builder.Property(u => u.IsDeleted)
                .HasColumnName("is_deleted")
                .HasDefaultValue(false);

            builder.Property(u => u.DeletedAtUtc)
                .HasColumnName("deleted_at_utc");

            builder.Property(u => u.DeletedBy)
                .HasColumnName("deleted_by");

            // 1:1 с Cart (FK в Cart.UserId)
            builder.HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:N с Order (коллекция Orders есть)
            // Доп. конфигурация FK задана в OrderConfiguration
            builder.HasIndex(u => u.NormalizedEmail)
                .IsUnique();
            builder.HasIndex(u => new { u.IsDeleted, u.IsAdmin });
        }
    }
}
