using FluentResults;

namespace Reglamentator.Application.Errors;

public class PermissionError(string message) : Error(message)
{
    public const string UserNotAllowedToOperation = "Нет прав на текущую операцию";
}