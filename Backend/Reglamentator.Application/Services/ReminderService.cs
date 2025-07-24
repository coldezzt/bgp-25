using FluentResults;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Errors;
using Reglamentator.Application.Extensions;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Services;

/// <summary>
/// Реализация <see cref="IReminderService"/> сервиса для управления напоминаниями пользователей.
/// </summary>
/// <remarks>
/// Использует:
/// <list type="bullet">
///   <item><see cref="ITelegramUserRepository"/> для доступа к данным <see cref="TelegramUser"/>.</item>
///   <item><see cref="IOperationRepository"/> для доступа к данным <see cref="Operation"/></item>
///   <item><see cref="IReminderRepository"/> для доступа к данным <see cref="Reminder"/></item>
///   <item><see cref="IHangfireOperationJobHelper"/> для управления задачами операций в Hangfire.</item>
/// </list>
/// </remarks>
public class ReminderService(
    ITelegramUserRepository telegramUserRepository,
    IOperationRepository operationRepository,
    IReminderRepository reminderRepository,
    IHangfireReminderJobHelper hangfireReminderJobHelper
    ): IReminderService
{
    /// <inheritdoc/>
    public async Task<Result<Reminder>> AddReminderAsync(
        long telegramId, 
        long operationId, 
        CreateReminderDto reminderDto,
        CancellationToken cancellationToken = default)
    {
        if (!await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));
        
        var operation = await operationRepository.GetEntityByFilterAsync(
            op => op.Id == operationId, cancellationToken);
        
        if (operation == null)
            return Result.Fail(new NotFoundError(NotFoundError.OperationNotFound));
        
        if (operation.TelegramUserId != telegramId)
            return Result.Fail(new PermissionError(PermissionError.UserNotAllowedToOperation));
        
        var reminder = CreateNewReminder(operationId, reminderDto);
        await reminderRepository.InsertEntityAsync(reminder, cancellationToken);
        
        hangfireReminderJobHelper.CreateJobForReminder(operation, reminder);
        
        return Result.Ok(reminder);
    }

    /// <inheritdoc/>
    public async Task<Result<Reminder>> UpdateReminderAsync(
        long telegramId, 
        long operationId, 
        UpdateReminderDto reminderDto,
        CancellationToken cancellationToken = default)
    {
        if (!await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));
        
        var operation = await operationRepository.GetEntityByFilterAsync(
            op => op.Id == operationId, cancellationToken);
        
        if (operation == null)
            return Result.Fail(new NotFoundError(NotFoundError.OperationNotFound));
        
        var reminder = await reminderRepository.GetEntityByFilterAsync(
            r => r.Id == reminderDto.Id, cancellationToken);
        
        if (reminder == null)
            return Result.Fail(new NotFoundError(NotFoundError.ReminderNotFound));
        
        if (operation.TelegramUserId != telegramId || operation.Id != reminder.OperationId)
            return Result.Fail(new PermissionError(PermissionError.UserNotAllowedToOperation));
        
        UpdateReminderByDto(reminder, reminderDto);
        await reminderRepository.UpdateEntityAsync(reminder, cancellationToken);
        
        hangfireReminderJobHelper.UpdateJobForReminder(operation, reminder);
        
        return Result.Ok(reminder);
    }

    /// <inheritdoc/>
    public async Task<Result<Reminder>> DeleteReminderAsync(
        long telegramId, 
        long operationId, 
        long reminderId,
        CancellationToken cancellationToken = default)
    {
        if (!await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));
        
        var operation = await operationRepository.GetEntityByFilterAsync(
            op => op.Id == operationId, cancellationToken);
        
        if (operation == null)
            return Result.Fail(new NotFoundError(NotFoundError.OperationNotFound));

        var reminder = await reminderRepository.GetEntityByFilterAsync(
            r => r.Id == reminderId, cancellationToken);
        
        if (reminder == null)
            return Result.Fail(new NotFoundError(NotFoundError.ReminderNotFound));
        
        if (operation.TelegramUserId != telegramId || operation.Id != reminder.OperationId)
            return Result.Fail(new PermissionError(PermissionError.UserNotAllowedToOperation));
        
        await reminderRepository.DeleteEntityAsync(reminder, cancellationToken);
        
        hangfireReminderJobHelper.DeleteJobForReminder(reminder);
        
        return Result.Ok(reminder);
    }

    /// <summary>
    /// Создает новый экземпляр напоминания на основе DTO.
    /// </summary>
    /// <param name="operationId">Идентификатор операции.</param>
    /// <param name="reminderDto">Данные напоминания.</param>
    /// <returns>Новый экземпляр напоминания.</returns>
    private Reminder CreateNewReminder(long operationId, CreateReminderDto reminderDto) =>
        new()
        {
            MessageTemplate = reminderDto.MessageTemplate,
            OffsetBeforeExecution = reminderDto.OffsetBeforeExecution.ToTimeSpan(),
            OperationId = operationId,
        };

    /// <summary>
    /// Обновляет существующее напоминание данными из DTO.
    /// </summary>
    /// <param name="reminder">Напоминание для обновления.</param>
    /// <param name="reminderDto">Данные для обновления.</param>
    private void UpdateReminderByDto(Reminder reminder, UpdateReminderDto reminderDto)
    {
        reminder.MessageTemplate = reminderDto.MessageTemplate;
        reminder.OffsetBeforeExecution = reminderDto.OffsetBeforeExecution.ToTimeSpan();
    }
}