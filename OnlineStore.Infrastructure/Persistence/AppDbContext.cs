using Microsoft.EntityFrameworkCore;
using OnlineStore.Domain.Entities;

namespace OnlineStore.Infrastructure.Persistence
{
    /// <summary>
    /// Главный контекст EF Core. Хранит DbSet'ы и подтягивает все конфигурации из сборки Infrastructure.
    /// </summary>
    public class AppDbContext : DbContext
    {
        // Конструктор, принимающий параметры конфигурации
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Этот метод вызывается при создании модели базы данных
        // Здесь мы можем настроить связи, ограничения и другие аспекты модели
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Подтягиваем ВСЕ конфигурации IEntityTypeConfiguration<> из сборки Infrastructure
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        // DbSet'ы для каждой сущности
        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Model3D> Models => Set<Model3D>();
        public DbSet<ProjectImage> ProjectImages => Set<ProjectImage>();
        public DbSet<ModelImage> ModelImages => Set<ModelImage>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    }
}