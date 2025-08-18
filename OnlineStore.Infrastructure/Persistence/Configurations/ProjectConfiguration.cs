using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
    public class ProjectConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("projects", tb =>
            {
                tb.HasCheckConstraint("CK_Projects_Price_NonNegative", "price >= 0");
                tb.HasComment("Проекты (наборы моделей) и их метаданные.");
            });

            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name).HasMaxLength(200).IsRequired();
            builder.Property(p => p.Description).HasMaxLength(2000);

            builder.Property(p => p.Price)
                   .HasColumnName("price")
                   .HasColumnType("numeric(18,2)")
                   .IsRequired();

            builder.Property(p => p.CompanyUrl)
                   .HasColumnName("company_url")
                   .HasMaxLength(500);

            builder.Property(p => p.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            builder.HasMany(p => p.Models)
                   .WithOne(m => m.Project)
                   .HasForeignKey(m => m.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Images)
                   .WithOne(i => i.Project)
                   .HasForeignKey(i => i.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Навигации-коллекции в Project: OrderItems, CartItems — добавлять связи не требуется,
            // они настроены со стороны зависимых сущностей
            builder.HasIndex(p => p.CreatedAt);
        }
    }
}
