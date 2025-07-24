using FluentResults;

namespace Reglamentator.Application.Errors;

public class CreateUserError(string message) : Error(message)
{
    public const string TelegramUserAlreadyExist = "Пользователь с текущим id уже зарегистрирован";
}