using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class UpdateReminderDtoValidator : AbstractValidator<UpdateReminderDto>
{
    public UpdateReminderDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID напоминания должен быть положительным числом");

        RuleFor(x => x.MessageTemplate)
            .NotEmpty()
            .WithMessage("Шаблон сообщения обязателен");
    }
}