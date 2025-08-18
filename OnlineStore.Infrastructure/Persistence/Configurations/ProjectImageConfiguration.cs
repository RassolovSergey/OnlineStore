using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
       public class ProjectImageConfiguration : IEntityTypeConfiguration<ProjectImage>
       {
              public void Configure(EntityTypeBuilder<ProjectImage> builder)
              {
                     builder.ToTable("project_images", tb =>
                     {
                            tb.HasCheckConstraint("CK_ProjectImages_Order_NonNegative", "\"order\" >= 0");
                            tb.HasComment("Изображения, привязанные к проектам.");
                     });

                     builder.HasKey(pi => pi.Id);

                     builder.Property(pi => pi.ImageUrl)
                            .HasColumnName("image_url")
                            .HasMaxLength(500)
                            .IsRequired();

                     builder.Property(pi => pi.Description)
                            .HasMaxLength(1000);

                     builder.Property(pi => pi.IsPreview)
                            .HasColumnName("is_preview")
                            .IsRequired();

                     builder.Property(pi => pi.Order)
                            .HasColumnName("order")
                            .IsRequired();

                     builder.HasOne(pi => pi.Project)
                            .WithMany(p => p.Images)
                            .HasForeignKey(pi => pi.ProjectId)
                            .OnDelete(DeleteBehavior.Cascade);

                     builder.HasIndex(pi => new { pi.ProjectId, pi.Order });
                     builder.HasIndex(pi => pi.ProjectId)
                            .IsUnique()
                            .HasFilter("is_preview = TRUE");
              }
       }
}
