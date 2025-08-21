using FluentAssertions;
using FluentValidation.TestHelper;
using OnlineStore.Application.DTOs.Auth;
using OnlineStore.Application.Validation.Auth;
using Xunit;

namespace OnlineStore.Auth.Tests;

public class ValidatorsTests
{
    [Fact]
    public void RegisterValidator_should_validate_email_and_strong_password()
    {
        var v = new RegisterRequestValidator();

        // Пустой email должен давать ошибку по Email
        v.TestValidate(new RegisterRequest { Email = "", Password = "x" })
         .ShouldHaveValidationErrorFor(x => x.Email);

        // Валидный email и сильный пароль — валидно
        v.TestValidate(new RegisterRequest { Email = "user@example.com", Password = "Strong9!" })
         .IsValid.Should().BeTrue();
    }

    [Fact]
    public void LoginValidator_should_require_email_and_password()
    {
        var v = new LoginRequestValidator();

        // И email, и пароль пустые — обе ошибки должны присутствовать
        var bad = v.TestValidate(new LoginRequest { Email = "", Password = "" });
        bad.ShouldHaveValidationErrorFor(x => x.Email);
        bad.ShouldHaveValidationErrorFor(x => x.Password);

        // Валидный кейс
        v.TestValidate(new LoginRequest { Email = "user@example.com", Password = "x" })
         .IsValid.Should().BeTrue();
    }

    [Fact]
    public void ChangePasswordValidator_should_require_different_passwords()
    {
        var v = new ChangePasswordRequestValidator();

        // Текущий и новый одинаковые — форма невалидна
        var same = v.TestValidate(new ChangePasswordRequest { CurrentPassword = "Aaa1!", NewPassword = "Aaa1!" });
        same.IsValid.Should().BeFalse();

        // Валидный кейс (разные пароли, новый — «сильный»)
        v.TestValidate(new ChangePasswordRequest { CurrentPassword = "Aaa1!Aaa", NewPassword = "NewPass9!" })
         .IsValid.Should().BeTrue();
    }
}