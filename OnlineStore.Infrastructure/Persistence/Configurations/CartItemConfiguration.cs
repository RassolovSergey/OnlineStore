using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
       public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
       {
              public void Configure(EntityTypeBuilder<CartItem> builder)
              {
                     builder.ToTable("cart_items", tb =>
                     {
                            tb.HasCheckConstraint(
                       "CK_CartItem_ExactlyOneRef",
                       "(CASE " +
                       "WHEN model3d_id IS NOT NULL THEN project_id IS NULL " +
                       "WHEN project_id IS NOT NULL THEN model3d_id IS NULL " +
                       "ELSE FALSE END)"
                   );

                            tb.HasComment("Позиции в корзинах (модель или проект).");
                     });

                     builder.HasKey(ci => ci.Id);

                     builder.Property(ci => ci.AddedAt)
                            .HasColumnName("added_at")
                            .HasDefaultValueSql("CURRENT_TIMESTAMP");

                     builder.HasOne(ci => ci.Cart)
                            .WithMany(c => c.CartItems) // ⚠️ корректное имя коллекции
                            .HasForeignKey(ci => ci.CartId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasOne(ci => ci.Model3D)
                            .WithMany(m => m.CartItems) // коллекция есть в Model3D
                            .HasForeignKey(ci => ci.Model3DId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasOne(ci => ci.Project)
                            .WithMany(p => p.CartItems) // коллекция есть в Project
                            .HasForeignKey(ci => ci.ProjectId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasIndex(ci => new { ci.CartId, ci.Model3DId })
                            .IsUnique()
                            .HasFilter("model3d_id IS NOT NULL");

                     builder.HasIndex(ci => new { ci.CartId, ci.ProjectId })
                            .IsUnique()
                            .HasFilter("project_id IS NOT NULL");

                     builder.HasIndex(ci => new { ci.CartId, ci.AddedAt });
              }
       }
}
