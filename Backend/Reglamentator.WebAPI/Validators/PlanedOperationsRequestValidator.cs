using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class PlanedOperationsRequestValidator : AbstractValidator<PlanedOperationsRequest>
{
    public PlanedOperationsRequestValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("Telegram ID должен быть положительным числом");
            
        RuleFor(x => x.Range)
            .IsInEnum()
            .WithMessage("Неверный диапазон времени");
    }
}
