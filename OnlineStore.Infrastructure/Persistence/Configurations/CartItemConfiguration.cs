using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations;

/// <summary>
/// CartItem: позиция корзины. Может ссылаться на модель ИЛИ на проект.
/// </summary>
public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
       // Этот метод вызывается при создании модели базы данных
       // Здесь мы можем настроить связи, ограничения и другие аспекты модели
       public void Configure(EntityTypeBuilder<CartItem> builder)
       {
              // Имя таблицы + CHECK-ограничение через конфигурацию таблицы
              builder.ToTable("CartItems", tb =>
              {
                     // Ровно одна ссылка должна быть задана: либо Model3DId, либо ProjectId.
                     tb.HasCheckConstraint(
                   "CK_CartItem_ExactlyOneRef", // Название ограничения
                   "(CASE WHEN \"Model3DId\" IS NOT NULL THEN 1 ELSE 0 END) + " + // Проверяем, что Model3DId задан
                   "(CASE WHEN \"ProjectId\" IS NOT NULL THEN 1 ELSE 0 END) = 1" // Проверяем, что ProjectId задан
               );
              });

              // Обозначаем первичный ключ
              builder.HasKey(ci => ci.Id);

              // Дата добавления — по умолчанию на стороне БД
              builder.Property(ci => ci.AddedAt)
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");    // Используем SQL для установки текущей даты

              // Cart 1 → N CartItems
              builder.HasOne(ci => ci.Cart)
                     .WithMany(c => c.CartItems)  // Указываем навигационное свойство в Cart
                     .HasForeignKey(ci => ci.CartId) // Указываем внешний ключ в CartItem
                     .OnDelete(DeleteBehavior.Cascade);   // При удалении корзины удаляются и все позиции в ней

              // Опциональная связь с Model3D
              builder.HasOne(ci => ci.Model3D)
                     .WithMany(m => m.CartItems!) // Указываем навигационное свойство в Model3D
                     .HasForeignKey(ci => ci.Model3DId) // Указываем внешний ключ в CartItem
                     .OnDelete(DeleteBehavior.Cascade); // При удалении модели удаляются и все позиции в корзине, связанные с ней

              // Опциональная связь с Project
              builder.HasOne(ci => ci.Project)
                     .WithMany(p => p.CartItems!) // Указываем навигационное свойство в Project
                     .HasForeignKey(ci => ci.ProjectId)   // Указываем внешний ключ в CartItem
                     .OnDelete(DeleteBehavior.Cascade);   // При удалении проекта удаляются и все позиции в корзине, связанные с ним

              // Частично-уникальные индексы (partial indexes, PostgreSQL) — запретить дубль товара в одной корзине
              builder.HasIndex(ci => new { ci.CartId, ci.Model3DId })
                     .IsUnique()  // Уникальный индекс по CartId и Model3DId
                     .HasFilter("\"Model3DId\" IS NOT NULL"); // Фильтр для Model3DId, чтобы не мешать с ProjectId

              builder.HasIndex(ci => new { ci.CartId, ci.ProjectId })
                     .IsUnique()  // Уникальный индекс по CartId и ProjectId
                     .HasFilter("\"ProjectId\" IS NOT NULL"); // Фильтр для ProjectId, чтобы не мешать с Model3DId
                     
               // Быстрая выдача корзины с сортировкой
              builder.HasIndex(ci => new { ci.CartId, ci.AddedAt });
    }
}
