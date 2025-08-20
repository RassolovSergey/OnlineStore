using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OnlineStore.Client.Dtos;
using OnlineStore.Client.Http;

const string baseUrl = "http://localhost:5232";

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // 1) InMemory хранилище access-токена
        services.AddSingleton<ITokenStore, InMemoryTokenStore>();

        // 2) Общий CookieContainer — HttpOnly cookie 'rt' живёт между вызовами
        services.AddSingleton(new CookieContainer());

        // 3) HttpClient + DelegatingHandler
        services.AddTransient(sp =>
            new BearerAuthHandler(sp.GetRequiredService<ITokenStore>(), baseUrl));

        services.AddHttpClient("api", client =>
        {
            client.BaseAddress = new Uri(baseUrl);
        })
        .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
        {
            CookieContainer = sp.GetRequiredService<CookieContainer>(),
            UseCookies = true,
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            // На время разработки можно доверить дев-сертификат:
            // ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        })
        .AddHttpMessageHandler<BearerAuthHandler>();
    })
    .Build();

// создаём scope и берём клиент
using var scope = host.Services.CreateScope();
var sp = scope.ServiceProvider;
var http = sp.GetRequiredService<IHttpClientFactory>().CreateClient("api");

// === ДЕМО ПОТОК ===

// 1) Login: refresh попадёт в HttpOnly cookie, access вернётся в body
var loginResp = await http.PostAsJsonAsync("/api/auth/login", new
{
    email = "admin@onlinestore.local",
    password = "Admin#12345"
});
loginResp.EnsureSuccessStatusCode();

var loginDto = await loginResp.Content.ReadFromJsonAsync<AuthResponseDto>();
if (loginDto is null) throw new Exception("Не удалось разобрать ответ логина");
sp.GetRequiredService<ITokenStore>().SetToken(loginDto.Token);
Console.WriteLine($"Login OK, email={loginDto.Email}");

// 2) Запрос к защищённому API. Если 401 — Handler сам сделает /auth/refresh и повторит запрос
var me = await http.GetAsync("/api/auth/sessions");
me.EnsureSuccessStatusCode();
Console.WriteLine("GET /api/auth/sessions OK");
