using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations;

/// <summary>
/// Конфигурация сущности Cart:
/// - явное имя таблицы
/// - связь 1:1 с User (часть 2: ключ FK на UserId)
/// - каскадное удаление
/// </summary>
public class CartConfiguration : IEntityTypeConfiguration<Cart>
{
    public void Configure(EntityTypeBuilder<Cart> builder)
    {
        // Таблица для сущности Cart
        // Используем явное указание имени таблицы, если нужно
        builder.ToTable("Carts");

        // Обозначаем первичный ключ
        builder.HasKey(c => c.Id);
    }
}
