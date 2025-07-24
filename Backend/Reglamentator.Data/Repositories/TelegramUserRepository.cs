using Microsoft.EntityFrameworkCore;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Data.Repositories;

/// <summary>
/// Реализация репозитория <see cref="ITelegramUserRepository"/> для работы с пользователями.
/// </summary>
/// <remarks>
/// Использует <see cref="AppDbContext"/> - Контекст базы данных.
/// </remarks>
public class TelegramUserRepository(AppDbContext appDbContext) : Repository<TelegramUser>(appDbContext), ITelegramUserRepository
{
    /// <summary>
    /// Проверяет существование пользователя по идентификатору Telegram.
    /// </summary>
    /// <param name="id">Идентификатор пользователя в Telegram.</param>
    /// <param name="cancellationToken">Токен отмены.</param>
    /// <returns>
    /// true - если пользователь с указанным Telegram ID существует в базе,
    /// false - в противном случае.
    /// </returns>
    public override async Task<bool> IsExistAsync(long id, CancellationToken cancellationToken = default) =>
        await AppDbContext.TelegramUsers
            .AnyAsync(e => e.TelegramId == id, cancellationToken);
}