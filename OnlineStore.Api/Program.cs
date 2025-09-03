using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net;
using OnlineStore.Infrastructure.Persistence;    // AppDbContext
using OnlineStore.Infrastructure.Extensions;    // AddInfrastructure, AddJwtAuthentication
using OnlineStore.Application.Extensions;       // AddMappings
using OnlineStore.Api.Middlewares;              // ExceptionHandlingMiddleware
using OnlineStore.Api.Extensions;
using System.Text.Json.Serialization;               // AddAuthRateLimiting, AddSwaggerWithJwt

var builder = WebApplication.CreateBuilder(args);

// 1) БД
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
           .UseSnakeCaseNamingConvention());

// 2) Сервисы слоя Infrastructure (репозитории, фабрики и т.п.)
builder.Services.AddInfrastructure();

// 3) JWT-аутентификация (ЕДИНСТВЕННАЯ точка регистрации схемы "Bearer")
builder.Services.AddJwtAuthentication(builder.Configuration);

// 4) Авторизация (политики/роли)
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", p => p.RequireRole("Admin"));
});

// 5) Контроллеры + маппинги. Так же прячем значения равные null из файла json
builder.Services
    .AddControllers()
    .AddJsonOptions(opst =>
    {
        opst.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

builder.Services.AddMappings(); // AutoMapper профили из Application

// 6) Rate Limiting и Swagger
builder.Services.AddAuthRateLimiting();
builder.Services.AddSwaggerWithJwt();

// 7) CORS (AllowCredentials для refresh-cookie). Источники — из конфигурации.
builder.Services.AddCors(opt =>
{
    var origins = builder.Configuration["Cors:Origins"]?
        .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        ?? new[] { "http://localhost:5173" }; // DEV fallback

    opt.AddPolicy("Frontend", p =>
        p.WithOrigins(origins)
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// Опции для AdminSeed (см. ниже)
builder.Services.Configure<AdminSeedOptions>(builder.Configuration.GetSection("AdminSeed"));

var app = builder.Build();

// 8) Авто-миграция БД при старте (в Docker-контейнере или при локальном запуске)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// 8.1) Сидирование админа (если настроено). В отдельном scope, т.к. внутри
//     могут быть другие сервисы с временем жизни Scoped.
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;

    // авто-миграции
    var db = sp.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // сидирование админа (если настроено)
    await DbSeeder.SeedAdminAsync(sp);
}


// 9) За обратным прокси / nginx — корректно получаем реальный IP и схему
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownProxies = { IPAddress.Parse("10.0.0.2") } // <-- IP вашего nginx/прокси
    // или KnownNetworks = { new(IPAddress.Parse("10.0.0.0"), 8) }
});

// 10) Глобальный обработчик ошибок — как можно выше в пайплайне
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 11) Swagger — в Dev/Local
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseHttpsRedirection();
    app.UseHsts();
}

// 12) CORS (до аутентификации)
app.UseCors("Frontend");

// 13) Лимитер, аутентификация и авторизация
app.UseAuthRateLimiting(); // внутри вызывает UseRateLimiter()
app.UseAuthentication();
app.UseAuthorization();

// 14) Маршруты
app.MapControllers();

app.Run();
