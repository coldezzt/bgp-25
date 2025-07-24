using FluentResults;

namespace Reglamentator.Application.Errors;

/// <summary>
/// Ошибка, связанная с созданием пользователя.
/// </summary>
/// <remarks>
/// Возникает при нарушении правил создания нового пользователя.
/// </remarks>
public class CreateUserError(string message) : Error(message)
{
    public const string TelegramUserAlreadyExist = "Пользователь с текущим id уже зарегистрирован";
}