using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OnlineStore.Api.Security;
using OnlineStore.Infrastructure.Options;
using OnlineStore.Infrastructure.Security;
using Microsoft.Extensions.Options;

namespace OnlineStore.Auth.Tests;

public class MiscAuthTests
{
    [Fact]
    public void UserExtensions_GetUserId_should_parse_from_claims_or_throw()
    {
        var uid = Guid.NewGuid();

        // Из NameIdentifier
        var principal1 = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] {
                new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, uid.ToString())
            }));
        principal1.GetUserId().Should().Be(uid);

        // Из sub
        var principal2 = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(new[] {
                new System.Security.Claims.Claim(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub, uid.ToString())
            }));
        principal2.GetUserId().Should().Be(uid);

        // Пусто => исключение
        var bad = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity());
        FluentActions.Invoking(() => bad.GetUserId()).Should().Throw<UnauthorizedAccessException>();
    }

    [Fact]
    public void CookieFactory_should_generate_HttpOnly_cookie_options()
    {
        var ctx = new DefaultHttpContext();
        var opt = CookieFactory.Refresh(ctx.Request);

        opt.HttpOnly.Should().BeTrue();
        opt.SameSite.Should().NotBe(SameSiteMode.None); // в Dev может быть Lax/Strict
    }

    [Fact]
    public void RefreshTokenFactory_Hash_should_be_stable_and_256bits()
    {
        var factory = new RefreshTokenFactory(Options.Create(new JwtOptions
        {
            Issuer = "i",
            Audience = "a",
            Key = new string('K', 64),
            RefreshTokenLifetimeDays = 7
        }));

        var h1 = factory.Hash("abc");
        var h2 = factory.Hash("abc");

        h1.Should().Be(h2); // детерминированно
        Convert.FromBase64String(h1).Length.Should().Be(32); // SHA-256 (32 байта)
    }
}
