using FluentValidation;

namespace Reglamentator.WebAPI.Validators;

public class UpdateOperationDtoValidator: AbstractValidator<UpdateOperationDto>
{
    public UpdateOperationDtoValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("ID операции должен быть положительным числом");

        RuleFor(x => x.Theme)
            .NotEmpty()
            .WithMessage("Тема операции обязательна");

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Описание операции обязательно");

        RuleFor(x => x.StartDate)
            .NotNull()
            .WithMessage("Дата начала обязательна");
    }
}