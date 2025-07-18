using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("Telegram ID должен быть положительным числом");
    }
}