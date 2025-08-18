using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
       public class OrderConfiguration : IEntityTypeConfiguration<Order>
       {
              public void Configure(EntityTypeBuilder<Order> builder)
              {
                     builder.ToTable("orders", tb =>
                     {
                            tb.HasComment("Заказы пользователей.");
                     });

                     builder.HasKey(o => o.Id);

                     builder.Property(o => o.CreatedAt)
                            .HasColumnName("created_at")
                            .HasDefaultValueSql("CURRENT_TIMESTAMP");

                     builder.Property(o => o.TotalAmount)
                            .HasColumnName("total_amount")
                            .HasPrecision(18, 2);

                     builder.HasOne(o => o.User)
                            .WithMany(u => u.Orders)
                            .HasForeignKey(o => o.UserId)
                            .OnDelete(DeleteBehavior.Restrict);

                     // Коллекция OrderItems уже объявлена в Order — связи зададим в конфиге OrderItem
                     builder.HasIndex(o => o.UserId);
                     builder.HasIndex(o => o.CreatedAt);
                     builder.HasIndex(o => new { o.UserId, o.CreatedAt });
              }
       }
}
