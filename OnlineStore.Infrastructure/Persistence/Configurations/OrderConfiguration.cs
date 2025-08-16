using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations;

/// <summary>
/// Конфигурация заказа пользователя (Order).
/// </summary>
public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
       public void Configure(EntityTypeBuilder<Order> builder)
       {
              // Настройка таблицы для заказов
              builder.ToTable("Orders");
              // Уникальный идентификатор заказа
              builder.HasKey(o => o.Id);

              // Связь: User 1 -> N Orders (без каскада — сохраняем историю)
              builder.HasOne(o => o.User)
                     .WithMany(u => u.Orders!)    // Один пользователь может иметь много заказов
                     .HasForeignKey(o => o.UserId)    // Внешний ключ к пользователю
                     .OnDelete(DeleteBehavior.Restrict); // Удаление заказов не приводит к удалению пользователя

              // Время создания — дефолт на стороне БД
              builder.Property(o => o.CreatedAt)
                     .HasDefaultValueSql("CURRENT_TIMESTAMP");    // Устанавливаем текущее время при создании заказа

              // Сумма заказа (опционально). Точность 18,2 также задана глобально в DbContext.
              builder.Property(o => o.TotalAmount)
                     .HasPrecision(18, 2);    // Точность для суммы заказа

              // Индексы для быстрого поиска заказов
              builder.HasIndex(o => o.UserId);
              builder.HasIndex(o => o.CreatedAt);
              // Индекс для выборок истории: сначала по пользователю, затем по дате
              builder.HasIndex(o => new { o.UserId, o.CreatedAt });
       }
}
