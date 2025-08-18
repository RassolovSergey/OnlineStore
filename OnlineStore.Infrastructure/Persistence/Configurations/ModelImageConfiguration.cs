using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations
{
       public class ModelImageConfiguration : IEntityTypeConfiguration<ModelImage>
       {
              public void Configure(EntityTypeBuilder<ModelImage> builder)
              {
                     builder.ToTable("model_images", tb =>
                     {
                            tb.HasCheckConstraint("CK_ModelImages_Order_NonNegative", "\"order\" >= 0");
                            tb.HasComment("Изображения, привязанные к 3D-моделям.");
                     });

                     builder.HasKey(mi => mi.Id);

                     builder.Property(mi => mi.ImageUrl)
                            .HasColumnName("image_url")
                            .HasMaxLength(500)
                            .IsRequired();

                     builder.Property(mi => mi.Description)
                            .HasMaxLength(1000);

                     builder.Property(mi => mi.IsPreview)
                            .HasColumnName("is_preview")
                            .IsRequired();

                     builder.Property(mi => mi.Order)
                            .HasColumnName("order")
                            .IsRequired();

                     builder.HasOne(mi => mi.Model3D)
                            .WithMany(m => m.Images)
                            .HasForeignKey(mi => mi.Model3DId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasIndex(mi => new { mi.Model3DId, mi.Order });
                     builder.HasIndex(mi => mi.Model3DId)
                            .IsUnique()
                            .HasFilter("is_preview = TRUE");
              }
       }
}
