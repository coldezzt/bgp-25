using FluentValidation;
using Google.Protobuf.WellKnownTypes;

namespace Reglamentator.WebAPI.Validators;

public class CreateOperationRequestValidator : AbstractValidator<CreateOperationRequest>
{
    public CreateOperationRequestValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("Telegram ID должен быть положительным числом");
            
        RuleFor(x => x.Operation)
            .NotNull()
            .WithMessage("Операция не может быть null")
            .SetValidator(new CreateOperationDtoValidator());
    }
}