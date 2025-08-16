using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations;

/// <summary>
/// Конфигурация элемента заказа (OrderItem).
/// </summary>
public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
       public void Configure(EntityTypeBuilder<OrderItem> builder)
       {
              builder.ToTable("OrderItems", tb =>
              {
                     tb.HasCheckConstraint(
                            "CK_OrderItem_ExactlyOneRef",
                            "(CASE WHEN \"Model3DId\" IS NOT NULL THEN 1 ELSE 0 END) + " +
                            "(CASE WHEN \"ProjectId\" IS NOT NULL THEN 1 ELSE 0 END) = 1"
                     );

                     tb.HasCheckConstraint(
                            "CK_OrderItems_PriceAtPurchase_NonNegative",
                            "\"PriceAtPurchase\" >= 0"
                     );
              });


              // Уникальный идентификатор элемента заказа
              builder.HasKey(oi => oi.Id);

              // Связь: Order 1 -> N OrderItems (каскад при удалении заказа)
              builder.HasOne(oi => oi.Order)
                     .WithMany(o => o.OrderItems) // Один заказ может иметь много элементов
                     .HasForeignKey(oi => oi.OrderId) // Внешний ключ к заказу
                     .OnDelete(DeleteBehavior.Cascade);   // Удаление элементов при удалении заказа

              // Опциональная связь с Model3D (без каскада — сохраняем историю)
              builder.HasOne(oi => oi.Model3D)
                     .WithMany(m => m.OrderItems!)    // Один 3D-модель может иметь много элементов заказа
                     .HasForeignKey(oi => oi.Model3DId)   // Внешний ключ к модели
                     .OnDelete(DeleteBehavior.Restrict);  // Удаление элементов не приводит к удалению модели

              // Опциональная связь с Project (без каскада — сохраняем историю)
              builder.HasOne(oi => oi.Project)
                     .WithMany(p => p.OrderItems!)    // Один проект может иметь много элементов заказа
                     .HasForeignKey(oi => oi.ProjectId)   // Внешний ключ к проекту
                     .OnDelete(DeleteBehavior.Restrict);  // Удаление элементов не приводит к удалению проекта

              // Цена позиции на момент покупки
              builder.Property(oi => oi.PriceAtPurchase)
                     .HasPrecision(18, 2);    // Точность для цены

              // Уникальность внутри одного заказа (частично-уникальные индексы)
              builder.HasIndex(oi => new { oi.OrderId, oi.Model3DId })
                     .IsUnique()  // Уникальный индекс по OrderId и Model3DId
                     .HasFilter("\"Model3DId\" IS NOT NULL"); // Условие для уникальности только для Model3DId

              builder.HasIndex(oi => new { oi.OrderId, oi.ProjectId })
                     .IsUnique()  // Уникальный индекс по OrderId и ProjectId
                     .HasFilter("\"ProjectId\" IS NOT NULL"); // Условие для уникальности только для ProjectId

              // Быстрый доступ к позициям конкретного заказа
              builder.HasIndex(oi => oi.OrderId);

              // История покупок по модели (чтение аналитики)
              builder.HasIndex(oi => oi.Model3DId).HasFilter("\"Model3DId\" IS NOT NULL");

              // История покупок по проекту (чтение аналитики)
              builder.HasIndex(oi => oi.ProjectId).HasFilter("\"ProjectId\" IS NOT NULL");
       }
}
