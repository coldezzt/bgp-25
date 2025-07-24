using FluentResults;

namespace Reglamentator.Application.Errors;

/// <summary>
/// Ошибка, связанная с отсутствием запрашиваемого ресурса.
/// </summary>
/// <remarks>
/// Возникает при попытке доступа к несуществующим или удаленным данным.
/// </remarks>
public class NotFoundError(string message) : Error(message)
{
    public const string UserNotFound = "Пользователь с текущим id не найден";
    public const string ReminderNotFound = "Напоминание с текущим id не найдено";
    public const string OperationNotFound = "Операция с текущим id не найдена";
}