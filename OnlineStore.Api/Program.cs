using Microsoft.EntityFrameworkCore;
using OnlineStore.Infrastructure.Persistence;
using OnlineStore.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Добавляем EF Core и PostgreSQL
// Используем строку подключения из конфигурации
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddInfrastructure();   // Добавляем инфраструктурные зависимости
builder.Services.AddJwtAuthentication(builder.Configuration); // Добавляем JWT-аутентификацию


builder.Services.AddControllers();  // Добавляем контроллеры
builder.Services.AddEndpointsApiExplorer(); // Добавляем поддержку Swagger для документации API
builder.Services.AddSwaggerGen(); // Добавляем Swagger для UI

var app = builder.Build();

// Настройка Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   // отдаёт swagger.json (документ API)
    app.UseSwaggerUI(); // отображает UI по адресу /swagger
}

app.UseHttpsRedirection();

// Middleware для аутентификации и авторизации
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
