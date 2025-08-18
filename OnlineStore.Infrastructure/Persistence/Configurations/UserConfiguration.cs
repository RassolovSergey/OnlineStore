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

            // 1:1 с Cart (FK в Cart.UserId)
            builder.HasOne(u => u.Cart)
                .WithOne(c => c.User)
                .HasForeignKey<Cart>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // 1:N с Order (коллекция Orders есть)
            // Доп. конфигурация FK задана в OrderConfiguration
            builder.HasIndex(u => u.NormalizedEmail)
                .IsUnique();
        }
    }
}
