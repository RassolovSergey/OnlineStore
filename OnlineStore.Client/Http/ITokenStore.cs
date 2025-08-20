namespace OnlineStore.Client.Http
{
    /// <summary>
    /// Память для access-токена. В проде можно заменить на IDistributedCache/IMemoryCache, 
    /// но хранить access на клиенте: ТОЛЬКО в памяти.
    /// </summary>
    public interface ITokenStore
    {
        string? GetToken();
        void SetToken(string token);
        void Clear();
    }

    public class InMemoryTokenStore : ITokenStore
    {
        private string? _token;
        private readonly object _lock = new();

        public string? GetToken()
        {
            lock (_lock) return _token;
        }

        public void SetToken(string token)
        {
            lock (_lock) _token = token;
        }

        public void Clear()
        {
            lock (_lock) _token = null;
        }
    }
}
