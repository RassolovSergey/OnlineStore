using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Configurations;

/// <summary>
/// Конфигурация сущности User:
/// - уникальность Email
/// - длины строк
/// - дефолт для CreatedAt
/// - связь 1:1 с Cart (часть 1: указываем навигацию со стороны User)
/// </summary>
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Таблица для сущности User
        // Используем явное указание имени таблицы, если нужно
        builder.ToTable("Users");

        // Обозначаем первичный ключ
        builder.HasKey(u => u.Id);

        // Настраиваем свойства
        builder.Property(u => u.Email)
            .IsRequired()       // Email обязателен
            .HasMaxLength(255); // Максимальная длина Email

        builder.Property(u => u.NormalizedEmail)
            .IsRequired()       // Нормализованный email обязателен
            .HasMaxLength(255); // Максимальная длина нормализованного email

        // Настраиваем уникальность Email
        builder.HasIndex(u => u.Email)
            .IsUnique(); // уникальный Email

        // Настраиваем свойства для пароля
        builder.Property(u => u.PasswordHash)
            .IsRequired()       // Пароль обязателен
            .HasMaxLength(500); // Максимальная длина хеша пароля

        // Дата регистрации по умолчанию — на стороне БД
        builder.Property(u => u.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Используем SQL для установки текущей даты

        // Связь 1:1 с корзиной (часть 1): у пользователя может быть одна корзина
        builder.HasOne(u => u.Cart)
            .WithOne(c => c.User) // У корзины есть один пользователь
            .HasForeignKey<Cart>(c => c.UserId) // Указываем внешний ключ в корзине
            .OnDelete(DeleteBehavior.Cascade);  // При удалении пользователя удаляется и корзина
    }
}
