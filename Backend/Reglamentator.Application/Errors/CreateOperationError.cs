using FluentResults;

namespace Reglamentator.Application.Errors;

public class CreateOperationError(string message) : Error(message)
{
    public const string OperationStartDateCanNotBeInPast = "Операция должна быть в будущем";
}