using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
       public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
       {
              public void Configure(EntityTypeBuilder<OrderItem> builder)
              {
                     builder.ToTable("order_items", tb =>
                     {
                            tb.HasCheckConstraint(
                       "CK_OrderItem_ExactlyOneRef",
                       "(CASE " +
                       "WHEN model3d_id IS NOT NULL THEN project_id IS NULL " +
                       "WHEN project_id IS NOT NULL THEN model3d_id IS NULL " +
                       "ELSE FALSE END)"
                   );

                            tb.HasCheckConstraint("CK_OrderItem_PriceAtPurchase_NonNegative", "price_at_purchase >= 0");
                            tb.HasComment("Позиции заказа: либо модель, либо проект.");
                     });

                     builder.HasKey(oi => oi.Id);

                     builder.Property(oi => oi.PriceAtPurchase)
                            .HasColumnName("price_at_purchase")
                            .HasPrecision(18, 2)
                            .IsRequired();

                     builder.HasOne(oi => oi.Order)
                            .WithMany(o => o.OrderItems) // ⚠️ корректное имя коллекции
                            .HasForeignKey(oi => oi.OrderId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasOne(oi => oi.Model3D)
                            .WithMany(m => m.OrderItems)  // коллекция есть в Model3D
                            .HasForeignKey(oi => oi.Model3DId)
                            .OnDelete(DeleteBehavior.Restrict);

                     builder.HasOne(oi => oi.Project)
                            .WithMany(p => p.OrderItems)  // коллекция есть в Project
                            .HasForeignKey(oi => oi.ProjectId)
                            .OnDelete(DeleteBehavior.Restrict);

                     builder.HasIndex(oi => new { oi.OrderId, oi.Model3DId })
                            .IsUnique()
                            .HasFilter("model3d_id IS NOT NULL");

                     builder.HasIndex(oi => new { oi.OrderId, oi.ProjectId })
                            .IsUnique()
                            .HasFilter("project_id IS NOT NULL");

                     builder.HasIndex(oi => oi.OrderId);
                     builder.HasIndex(oi => oi.Model3DId).HasFilter("model3d_id IS NOT NULL");
                     builder.HasIndex(oi => oi.ProjectId).HasFilter("project_id IS NOT NULL");

                     builder.HasQueryFilter(oi => oi.Order != null
                          && oi.Order.User != null
                          && !oi.Order.User.IsDeleted);
              }
       }
}
