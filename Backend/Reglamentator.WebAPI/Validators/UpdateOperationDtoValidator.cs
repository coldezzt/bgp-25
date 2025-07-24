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
            .WithMessage("Тема операции обязательна")
            .MaximumLength(200)
            .WithMessage("Максимальная длина темы: 200");
        
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Описание операции обязательно")
            .MaximumLength(2000)
            .WithMessage("Максимальная длина описания: 2000");

        RuleFor(x => x.StartDate)
            .NotNull()
            .WithMessage("Дата начала обязательна");
    }
}