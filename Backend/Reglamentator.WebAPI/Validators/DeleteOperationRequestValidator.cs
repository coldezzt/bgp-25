using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class DeleteOperationRequestValidator : AbstractValidator<DeleteOperationRequest>
{
    public DeleteOperationRequestValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("Telegram ID должен быть положительным числом");
            
        RuleFor(x => x.OperationId)
            .GreaterThan(0)
            .WithMessage("ID операции должен быть положительным числом");
    }
}