using FluentResults;
using Reglamentator.Application.Dtos;
using Reglamentator.Domain.Entities;

namespace Reglamentator.Application.Abstractions;

public interface IReminderService
{
    Task<Result<Reminder>> AddReminderAsync(long telegramId, long operationId, CreateReminderDto reminderDto, CancellationToken cancellationToken = default);
    Task<Result<Reminder>> UpdateReminderAsync(long telegramId, long operationId, UpdateReminderDto reminderDto, CancellationToken cancellationToken = default);
    Task<Result<Reminder>> DeleteReminderAsync(long telegramId, long operationId, long reminderId, CancellationToken cancellationToken = default);
}