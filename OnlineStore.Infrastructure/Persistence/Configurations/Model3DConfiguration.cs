using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
    public class Model3DConfiguration : IEntityTypeConfiguration<Model3D>
    {
        public void Configure(EntityTypeBuilder<Model3D> builder)
        {
            builder.ToTable("models", tb =>
            {
                tb.HasCheckConstraint("CK_Models_Price_NonNegative", "price >= 0");
                tb.HasComment("3D-модели и их метаданные.");
            });

            builder.HasKey(m => m.Id);

            builder.Property(m => m.Name).HasMaxLength(200).IsRequired();
            builder.Property(m => m.Description).HasMaxLength(2000);

            builder.Property(m => m.Price)
                   .HasColumnName("price")
                   .HasColumnType("numeric(18,2)")
                   .IsRequired();

            builder.Property(m => m.CompanyUrl)
                   .HasColumnName("company_url")
                   .HasMaxLength(500);

            builder.Property(m => m.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            builder.HasOne(m => m.Project)
                   .WithMany(p => p.Models)
                   .HasForeignKey(m => m.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(m => m.Images)
                   .WithOne(i => i.Model3D)
                   .HasForeignKey(i => i.Model3DId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Коллекции m.OrderItems / m.CartItems существуют в сущности — связи зададим с зависимых
            builder.HasIndex(m => m.CreatedAt);
        }
    }
}
