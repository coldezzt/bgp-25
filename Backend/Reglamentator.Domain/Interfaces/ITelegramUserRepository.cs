using Reglamentator.Domain.Entities;

namespace Reglamentator.Domain.Interfaces;

/// <summary>
/// Предоставляет доступ к данным пользователей.
/// </summary>
public interface ITelegramUserRepository: IRepository<TelegramUser>
{
}