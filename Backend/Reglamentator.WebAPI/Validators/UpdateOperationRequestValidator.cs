using FluentValidation;
using Google.Protobuf.WellKnownTypes;

namespace Reglamentator.WebAPI.Validators;

public class UpdateOperationRequestValidator : AbstractValidator<UpdateOperationRequest>
{
    public UpdateOperationRequestValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("Telegram ID должен быть положительным числом");

        RuleFor(x => x.Operation)
            .NotNull()
            .WithMessage("Операция не может быть null")
            .SetValidator(new UpdateOperationDtoValidator());
    }
}