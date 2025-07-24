using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class CreateReminderDtoValidator : AbstractValidator<CreateReminderDto>
{
    public CreateReminderDtoValidator()
    {
        RuleFor(x => x.MessageTemplate)
            .NotEmpty()
            .WithMessage("Шаблон сообщения обязателен")
            .MaximumLength(1000)
            .WithMessage("Максимальная длина шаблона: 1000");
    }
}