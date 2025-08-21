using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using OnlineStore.Api.Controllers;
using OnlineStore.Api.Security;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Interfaces.Services;
using OnlineStore.Infrastructure.Security;

namespace OnlineStore.Auth.Tests;

// Тестируем поведение контроллера: refresh в HttpOnly cookie, чтение/очистка.
public class AuthControllerTests
{
    private static AuthController Build(out Mock<IAuthService> svc, bool prod = false)
    {
        svc = new Mock<IAuthService>(MockBehavior.Strict);
        var env = TestHelpers.FakeEnv(prod);
        return new AuthController(svc.Object, env);
    }

    private static string? GetSetCookie(ControllerBase ctrl)
        => ctrl.Response.Headers.TryGetValue("Set-Cookie", out var v) ? v.ToString() : null;

    [Fact]
    public async Task Register_sets_refresh_cookie_and_hides_refresh_in_body()
    {
        var ctrl = Build(out var svc);
        ctrl.ControllerContext = new ControllerContext { HttpContext = TestHelpers.HttpWithCookies() };

        svc.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>()))
           .ReturnsAsync(new AuthResponse { Email = "e@e", Token = "access", RefreshToken = "raw-rt" });

        var result = await ctrl.Register(new RegisterRequest { Email = "e@e", Password = "x" });
        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var dto = Assert.IsType<AuthResponse>(ok.Value);

        dto.RefreshToken.Should().BeNull(); // refresh удалён из тела

        var setCookie = GetSetCookie(ctrl)!;
        setCookie.Should().Contain($"{CookieNames.RefreshToken}=");
        // В некоторых средах флаги пишутся строчными: проверяем без учёта регистра
        setCookie.Should().ContainEquivalentOf("HttpOnly");
    }

    [Fact]
    public async Task Login_sets_refresh_cookie_and_returns_access()
    {
        var ctrl = Build(out var svc);
        ctrl.ControllerContext = new ControllerContext { HttpContext = TestHelpers.HttpWithCookies() };

        svc.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>()))
           .ReturnsAsync(new AuthResponse { Email = "e@e", Token = "t", RefreshToken = "raw" });

        var res = await ctrl.Login(new LoginRequest { Email = "e@e", Password = "p" });
        var ok = Assert.IsType<OkObjectResult>(res.Result);
        var dto = Assert.IsType<AuthResponse>(ok.Value);

        dto.RefreshToken.Should().BeNull();
        GetSetCookie(ctrl)!.Should().Contain($"{CookieNames.RefreshToken}=");
    }

    [Fact]
    public async Task Refresh_reads_cookie_and_sets_new_cookie()
    {
        var ctrl = Build(out var svc);
        var http = TestHelpers.HttpWithCookies();
        http.Request.Headers["Cookie"] = $"{CookieNames.RefreshToken}=old";
        ctrl.ControllerContext = new ControllerContext { HttpContext = http };

        svc.Setup(s => s.RefreshAsync(
                       It.Is<RefreshRequest>(r => r.RefreshToken == "old"),
                       It.IsAny<string?>(),
                       It.IsAny<string?>()))
           .ReturnsAsync(new AuthResponse { Email = "e@e", Token = "new-access", RefreshToken = "new-raw" });

        var res = await ctrl.Refresh();
        var ok = Assert.IsType<OkObjectResult>(res.Result);
        var dto = Assert.IsType<AuthResponse>(ok.Value);

        dto.RefreshToken.Should().BeNull();
        GetSetCookie(ctrl)!.Should().Contain($"{CookieNames.RefreshToken}=new-raw");
    }

    [Fact]
    public async Task Logout_clears_cookie_when_present()
    {
        var ctrl = Build(out var svc);
        var http = TestHelpers.HttpWithCookies();
        http.Request.Headers["Cookie"] = $"{CookieNames.RefreshToken}=abc";
        ctrl.ControllerContext = new ControllerContext { HttpContext = http };

        svc.Setup(s => s.LogoutAsync("abc", It.IsAny<string?>())).Returns(Task.CompletedTask);

        var res = await ctrl.Logout();
        Assert.IsType<NoContentResult>(res);
        GetSetCookie(ctrl)!.Should().Contain($"{CookieNames.RefreshToken}=;");
    }
}