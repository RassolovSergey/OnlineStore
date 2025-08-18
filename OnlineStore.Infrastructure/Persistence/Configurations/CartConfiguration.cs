using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence.Configurations
{
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("carts", tb =>
            {
                tb.HasComment("Корзины пользователей.");
            });

            builder.HasKey(c => c.Id);
            // 1:1 связь с User настроена в UserConfiguration (FK: Cart.UserId)
        }
    }
}
