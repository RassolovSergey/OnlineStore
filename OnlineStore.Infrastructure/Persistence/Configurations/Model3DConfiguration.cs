using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations;

/// <summary>
/// Конфигурация Model3D:
/// - имя таблицы;
/// - обязательные поля и длины;
/// - связь с Project (N:1, опциональная);
/// - связь с ModelImage (1:N).
/// </summary>
public class Model3DConfiguration : IEntityTypeConfiguration<Model3D>
{
    public void Configure(EntityTypeBuilder<Model3D> builder)
    {
        // Таблица для сущности Model3D
        // Используем явное указание имени таблицы, если нужно
        builder.ToTable("Models", tb =>
        {
            // Настройка схемы и других параметров таблицы
            tb.HasComment("Таблица моделей 3D, содержащая информацию о акутальных моделях, их проектах и изображениях.");
            tb.HasCheckConstraint(
                "CK_Models_Price_NonNegative",
                "\"Price\" >= 0"
            );
        });


        // Обозначаем первичный ключ
        builder.HasKey(m => m.Id);

        // Настраиваем свойства Name
        builder.Property(m => m.Name)
            .IsRequired() // Имя модели обязательно
            .HasMaxLength(200); // Максимальная длина имени модели

        // Настраиваем свойства Description
        builder.Property(m => m.Description)
            .HasMaxLength(2000);    // Максимальная длина описания модели

        // Настраиваем свойства CompanyUrl
        builder.Property(m => m.CompanyUrl)
            .HasMaxLength(500); // Максимальная длина URL компании

        builder.Property(m => m.Price)
            .HasPrecision(18, 2); // Точность для цены модели

        // Модель -> Проект (многие к одному, FK опционален)
        builder.HasOne(m => m.Project)
            .WithMany(p => p.Models)    // Указываем навигационное свойство в Project
            .HasForeignKey(m => m.ProjectId)    // Указываем внешний ключ в Model3D
            .OnDelete(DeleteBehavior.Cascade);  // При удалении проекта удаляются и все модели, связанные с ним

        // Модель -> Изображения модели (1:N)
        builder.HasMany(m => m.Images)
            .WithOne(i => i.Model3D)    // Указываем навигационное свойство в ModelImage
            .HasForeignKey(i => i.Model3DId)    // Указываем внешний ключ в ModelImage
            .OnDelete(DeleteBehavior.Cascade);  // При удалении модели удаляются и все изображения, связанные с ней

        // Индекс для быстрого поиска моделей по дате создания
        builder.HasIndex(m => m.CreatedAt);
    }
}
