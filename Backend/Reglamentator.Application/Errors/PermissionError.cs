using FluentResults;

namespace Reglamentator.Application.Errors;

/// <summary>
/// Ошибка, связанная с недостатком прав доступа.
/// </summary>
/// <remarks>
/// Используется, когда пользователь пытается выполнить действие,
/// на которое у него нет соответствующих прав.
/// </remarks>
public class PermissionError(string message) : Error(message)
{
    public const string UserNotAllowedToOperation = "Нет прав на текущую операцию";
}