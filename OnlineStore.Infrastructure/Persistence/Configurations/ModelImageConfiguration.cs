using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations;

/// <summary>
/// Конфигурация изображений модели.
/// </summary>
public class ModelImageConfiguration : IEntityTypeConfiguration<ModelImage>
{
    public void Configure(EntityTypeBuilder<ModelImage> builder)
    {
       // Настройка таблицы для изображений модели
       builder.ToTable("ModelImages", tb =>
       {
              tb.HasCheckConstraint("CK_ModelImages_Order_NonNegative",
                                   "\"Order\" >= 0"); 
       });


       // Уникальный идентификатор изображения
       builder.HasKey(mi => mi.Id);

       // Настройка свойств изображения
       builder.Property(mi => mi.ImageUrl)
              .IsRequired() // Обязательное поле
              .HasMaxLength(500);  // Максимальная длина URL

       // Настройка свойств Description
       builder.Property(mi => mi.Description)
              .HasMaxLength(1000); // Максимальная длина описания

       // Настройка свойств поля IsPreview
       builder.Property(mi => mi.IsPreview)
              .IsRequired(); // Обязательное поле

       // Настройка свойств поля Order
       builder.Property(mi => mi.Order)
               .IsRequired();      // Обязательное поле

        // Связь: модель 1 -> N изображения
        builder.HasOne(mi => mi.Model3D)
               .WithMany(m => m.Images)   // Один проект может иметь много изображений
               .HasForeignKey(mi => mi.Model3DId)       // Внешний ключ к модели
               .OnDelete(DeleteBehavior.Cascade);       // Удаление изображений при удалении модели

        // Быстрая сортировка галереи внутри модели
        builder.HasIndex(mi => new { mi.Model3DId, mi.Order });

        // Гарантируем ОДНО превью на модель (partial unique index, PostgreSQL)
        builder.HasIndex(mi => mi.Model3DId)
               .IsUnique()  // Уникальный индекс по Model3DId
               .HasFilter("\"IsPreview\" = TRUE");      // Условие для уникальности только для превью
    }
}
