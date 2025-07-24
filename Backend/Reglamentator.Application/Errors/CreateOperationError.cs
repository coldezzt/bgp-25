using FluentResults;

namespace Reglamentator.Application.Errors;

/// <summary>
/// Ошибка, связанная с созданием операции.
/// </summary>
/// <remarks>
/// Возникает при нарушении правил создания новой операции.
/// </remarks>
public class CreateOperationError(string message) : Error(message)
{
    public const string OperationStartDateCanNotBeInPast = "Операция должна быть в будущем";
}