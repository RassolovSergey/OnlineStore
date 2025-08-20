using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OnlineStore.Client.Http
{
    /// <summary>
    /// DelegatingHandler:
    ///  - добавляет Bearer access-токен;
    ///  - при 401 один раз пытается получить новый access через /api/auth/refresh (refresh в HttpOnly cookie);
    ///  - повторяет исходный запрос.
    /// </summary>
    public class BearerAuthHandler : DelegatingHandler
    {
        private readonly ITokenStore _tokenStore;
        private readonly string _baseUrl; // например, "https://localhost:5232"
        private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

        public BearerAuthHandler(ITokenStore tokenStore, string baseUrl)
        {
            _tokenStore = tokenStore;
            _baseUrl = baseUrl.TrimEnd('/');
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            // 1) Подставить текущий access
            var access = _tokenStore.GetToken();
            if (!string.IsNullOrWhiteSpace(access))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", access);
            }

            // 2) Обычный запрос
            var response = await base.SendAsync(request, cancellationToken);
            if (response.StatusCode != HttpStatusCode.Unauthorized)
                return response;

            // 3) Одна попытка рефреша
            response.Dispose();

            var refreshed = await TryRefreshAsync(cancellationToken);
            if (!refreshed)
            {
                _tokenStore.Clear();
                // Возвращаем 401 наверх — клиент решит (редирект на логин и т.п.)
                return new HttpResponseMessage(HttpStatusCode.Unauthorized)
                {
                    RequestMessage = request
                };
            }

            // 4) Повтор исходного запроса с новым access
            var retry = await CloneRequestAsync(request, cancellationToken);
            var newAccess = _tokenStore.GetToken();
            if (!string.IsNullOrWhiteSpace(newAccess))
            {
                retry.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newAccess);
            }
            return await base.SendAsync(retry, cancellationToken);
        }

        private async Task<bool> TryRefreshAsync(CancellationToken ct)
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, $"{_baseUrl}/api/auth/refresh");
            // ТЕЛА НЕТ — refresh возьмётся из HttpOnly cookie (CookieContainer внутри HttpClientHandler)
            using var resp = await base.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode) return false;

            // ожидаем { email, token, refreshToken:null }
            using var stream = await resp.Content.ReadAsStreamAsync(ct);
            var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty("token", out var tokenProp))
                return false;

            var newAccess = tokenProp.GetString();
            if (string.IsNullOrWhiteSpace(newAccess)) return false;

            _tokenStore.SetToken(newAccess);
            return true;
        }

        private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original, CancellationToken ct)
        {
            var clone = new HttpRequestMessage(original.Method, original.RequestUri)
            {
                Version = original.Version,
                VersionPolicy = original.VersionPolicy
            };

            if (original.Content is not null)
            {
                // читаем тело асинхронно
                var bytes = await original.Content.ReadAsByteArrayAsync(ct);
                clone.Content = new ByteArrayContent(bytes);

                // переносим заголовки контента
                foreach (var h in original.Content.Headers)
                    clone.Content.Headers.TryAddWithoutValidation(h.Key, h.Value);
            }

            // переносим заголовки запроса
            foreach (var h in original.Headers)
                clone.Headers.TryAddWithoutValidation(h.Key, h.Value);

            return clone;
        }
    }
}
