namespace OnlineStore.Client.Dtos
{
    public sealed class AuthResponseDto
    {
        public string Email { get; set; } = default!;
        public string Token { get; set; } = default!;
        public string? RefreshToken { get; set; }
    }
}