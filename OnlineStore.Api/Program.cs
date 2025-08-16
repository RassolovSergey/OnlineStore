using Microsoft.EntityFrameworkCore;
using OnlineStore.Infrastructure.Persistence;        // AppDbContext
using OnlineStore.Infrastructure.Extensions;        // AddInfrastructure, AddJwtAuthentication
using OnlineStore.Application.Extensions;
using OnlineStore.Api.Middlewares;
using FluentValidation.AspNetCore;
using FluentValidation;
using OnlineStore.Application.Validation.Auth;
using System.Threading.RateLimiting;
using OnlineStore.Api.Extensions;           // AddMappings или AddApplication

var builder = WebApplication.CreateBuilder(args);

// ---------- БД (API владеет строкой подключения) ----------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// ---------- Регистрация слоёв ----------
builder.Services.AddInfrastructure();                         // репозитории + инфраструктурные сервисы
builder.Services.AddJwtAuthentication(builder.Configuration); // схема JWT
builder.Services.AddMappings();                               // профили AutoMapper из Application


// ---------- ASP.NET Core сервисы ----------
builder.Services.AddControllers();


// Добавляем валидацию FluentValidation
// Используем AddFluentValidation для автоматической регистрации валидаторов
builder.Services.AddFluentValidationAutoValidation(opt =>
{
    // Отключаем автоматическую проверку DataAnnotations,
    // чтобы не дублировались сообщения.
    opt.DisableDataAnnotationsValidation = true;
});
builder.Services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerWithJwt(); // Swagger с JWT

// Добавляем Rate Limiting для защиты от DoS/DDoS атак
builder.Services.AddAuthRateLimiting();

var app = builder.Build();

// ---------- Middleware ----------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Глобальный обработчик ошибок — ставим в начале конвейера (после Swagger)
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseHttpsRedirection();

app.UseRateLimiter();   // Включаем Rate Limiting, для защиты от DoS/DDoS атак

app.UseAuthentication();   // JWT
app.UseAuthorization();

app.MapControllers();

app.Run();
