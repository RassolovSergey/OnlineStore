using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations;

/// <summary>
/// Конфигурация Project:
/// </summary>
public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    // Этот метод вызывается при создании модели базы данных
    // Здесь мы можем настроить связи, ограничения и другие аспекты модели
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        // Таблица для сущности Project
        builder.ToTable("Projects", tb =>
        {
            // Настройка схемы и других параметров таблицы
            tb.HasComment("Таблица проектов, содержащая информацию об актуальных проектах, их моделях и изображениях.");
            tb.HasCheckConstraint(
                "CK_Projects_Price_NonNegative",
                "\"Price\" >= 0"
            );
        });

        // Обозначаем первичный ключ
        builder.HasKey(p => p.Id);

        // Настраиваем свойства Name
        builder.Property(p => p.Name)
            .IsRequired()   // Имя проекта обязательно
            .HasMaxLength(200); // Максимальная длина имени проекта

        // Настраиваем свойства Description
        builder.Property(p => p.Description)
            .HasMaxLength(2000); // Максимальная длина описания проекта

        // Настраиваем свойства CompanyUrl
        builder.Property(p => p.CompanyUrl)
            .HasMaxLength(500); // Максимальная длина URL компании

        builder.Property(p => p.Price)
            .HasPrecision(18, 2); // Точность для цены проекта

        // Проект -> Модели (1:N)
        builder.HasMany(p => p.Models)
            .WithOne(m => m.Project)    // Указываем навигационное свойство в Model3D 1:1
            .HasForeignKey(m => m.ProjectId)    // Указываем внешний ключ в Model3D
            .OnDelete(DeleteBehavior.Cascade);  // При удалении проекта удаляются и все модели, связанные с ним

        // Проект -> Изображения проекта (1:N)
        builder.HasMany(p => p.Images)
            .WithOne(i => i.Project)    // Указываем навигационное свойство в ModelImage
            .HasForeignKey(i => i.ProjectId)    // Указываем внешний ключ в ModelImage
            .OnDelete(DeleteBehavior.Cascade);  // При удалении проекта удаляются и все изображения, связанные с ним

        // Индекс для быстрого поиска проектов по дате создания
        builder.HasIndex(p => p.CreatedAt);
    }
}
