using Microsoft.EntityFrameworkCore;
using OnlineStore.Infrastructure.Persistence;            // AppDbContext
using OnlineStore.Infrastructure.Extensions;            // AddInfrastructure, AddJwtAuthentication
using OnlineStore.Application.Extensions;               // AddMappings
using OnlineStore.Api.Middlewares;                      // ExceptionHandlingMiddleware
using OnlineStore.Api.Extensions;                       // RateLimitingExtensions, SwaggerExtensions

var builder = WebApplication.CreateBuilder(args);

// 1) БД
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
    .UseSnakeCaseNamingConvention());
    
// 2) Сервисы слоя Infrastructure
builder.Services.AddInfrastructure();

// 3) JWT-аутентификация
builder.Services.AddJwtAuthentication(builder.Configuration);

// 4) Авторизация (ОБЯЗАТЕЛЬНО, иначе UseAuthorization() упадёт)
builder.Services.AddAuthorization();

// 5) Контроллеры + FluentValidation (валидаторы подхватятся из сборки Application)
builder.Services.AddControllers()
    .AddJsonOptions(o => { /* настройка сериализации при необходимости */ });
builder.Services.AddMappings(); // AutoMapper профили из Application

// 6) Rate Limiting (IP-based) и Swagger с JWT
builder.Services.AddAuthRateLimiting();
builder.Services.AddSwaggerWithJwt();

var app = builder.Build();

// Swagger в Dev/Local
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Глобальный обработчик ошибок — как можно выше (сразу после Swagger)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();
app.UseAuthRateLimiting();
app.UseAuthentication();
app.UseAuthorization();

// Если за обратным прокси (например, Nginx) — корректно обрабатываем X-Forwarded-*
// app.UseForwardedHeaders(new ForwardedHeadersOptions
// {
//     ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
// });

app.MapControllers();
app.Run();
