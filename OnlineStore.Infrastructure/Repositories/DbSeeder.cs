using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OnlineStore.Domain.Entities;
using OnlineStore.Infrastructure.Persistence;

public class AdminSeedOptions
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public static class DbSeeder
{
    public static async Task SeedAdminAsync(
        IServiceProvider sp,
        CancellationToken ct = default)
    {
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbSeeder");
        var db = sp.GetRequiredService<AppDbContext>();
        var hasher = sp.GetRequiredService<IPasswordHasher<User>>();
        var opts = sp.GetRequiredService<IOptions<AdminSeedOptions>>().Value;

        if (string.IsNullOrWhiteSpace(opts.Email) || string.IsNullOrWhiteSpace(opts.Password))
        {
            logger.LogInformation("AdminSeed: пропущено (нет Email/Password).");
            return;
        }

        var norm = opts.Email.Trim().ToUpperInvariant();
        var user = await db.Users.SingleOrDefaultAsync(u => u.NormalizedEmail == norm, ct);

        if (user is null)
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Email = opts.Email.Trim(),
                NormalizedEmail = norm,
                CreatedAt = DateTime.UtcNow,
                IsAdmin = true
            };
            user.PasswordHash = hasher.HashPassword(user, opts.Password);
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            logger.LogInformation("AdminSeed: создан админ {Email}", opts.Email);
        }
        else if (!user.IsAdmin)
        {
            user.IsAdmin = true;
            await db.SaveChangesAsync(ct);
            logger.LogInformation("AdminSeed: пользователь {Email} повышен до Admin", opts.Email);
        }
    }
}
