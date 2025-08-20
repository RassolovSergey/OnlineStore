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

// 5) Контроллеры + маппинги
builder.Services.AddControllers()
    .AddJsonOptions(_ => { /* при необходимости — настройки сериализации */ });

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

// Прячем значения равные null из файла json
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });
var app = builder.Build();

// 8) За обратным прокси / nginx — корректно получаем реальный IP и схему
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    KnownProxies = { IPAddress.Parse("10.0.0.2") } // <-- IP вашего nginx/прокси
    // или KnownNetworks = { new(IPAddress.Parse("10.0.0.0"), 8) }
});

// 9) Глобальный обработчик ошибок — как можно выше в пайплайне
app.UseMiddleware<ExceptionHandlingMiddleware>();

// 10) Swagger — в Dev/Local
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
