using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations;

/// <summary>
/// Конфигурация изображений проекта.
/// </summary>
public class ProjectImageConfiguration : IEntityTypeConfiguration<ProjectImage>
{
    // Этот метод вызывается при создании модели базы данных
    // Здесь мы можем настроить связи, ограничения и другие аспекты модели
    public void Configure(EntityTypeBuilder<ProjectImage> builder)
    {
        // Таблица для сущности ProjectImage
        builder.ToTable("ProjectImages");

        // Обозначаем первичный ключ
        builder.HasKey(pi => pi.Id);

        // Настраиваем свойства ImageUrl
        builder.Property(pi => pi.ImageUrl)
               .IsRequired()    // URL изображения обязателен
               .HasMaxLength(500);  // Максимальная длина URL изображения

        // Настраиваем свойства Description
        builder.Property(pi => pi.Description)
               .HasMaxLength(1000); // Максимальная длина описания изображения

        // Настраиваем свойства IsPreview
        builder.Property(pi => pi.IsPreview)    
               .IsRequired();   // Флаг превью обязателен

        // Настраиваем свойства Order
        builder.Property(pi => pi.Order)
               .IsRequired();   // Порядок отображения изображения обязателен

        // Связь: проект 1 -> N изображения
        builder.HasOne(pi => pi.Project)
               .WithMany(p => p.Images)   // Указываем навигационное свойство в Project
               .HasForeignKey(pi => pi.ProjectId)   // Указываем внешний ключ в ProjectImage
               .OnDelete(DeleteBehavior.Cascade);   // При удалении проекта удаляются и все изображения, связанные с ним

        // Быстрая сортировка галереи внутри проекта
        // Индекс для ускорения выборки изображений по ProjectId и Order
        // Это поможет при отображении изображений в порядке, заданном пользователем
        builder.HasIndex(pi => new { pi.ProjectId, pi.Order });

        // Настраиваем свойство ProjectId
        builder.HasIndex(pi => pi.ProjectId)
               .IsUnique()  // Уникальный индекс для ProjectId, если нужно
               .HasFilter("\"IsPreview\" = TRUE");  // Фильтр для IsPreview, чтобы не мешать с другими изображениями
    }
}
