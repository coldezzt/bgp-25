using FluentResults;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Errors;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Services;

public class ReminderService(
    ITelegramUserRepository telegramUserRepository,
    IOperationRepository operationRepository,
    IReminderRepository reminderRepository,
    IHangfireReminderJobHelper hangfireReminderJobHelper
    ): IReminderService
{
    public async Task<Result<Reminder>> AddReminderAsync(
        long telegramId, 
        long operationId, 
        CreateReminderDto reminderDto,
        CancellationToken cancellationToken = default)
    {
        if (await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));
        
        var operation = await operationRepository.GetEntityByFilterAsync(
            op => op.Id == operationId, cancellationToken);
        
        if (operation == null)
            return Result.Fail(new NotFoundError(NotFoundError.OperationNotFound));
        
        if (operation.TelegramUserId != telegramId)
            return Result.Fail(new PermissionError(PermissionError.UserNotAllowedToOperation));
        
        var reminder = new Reminder
        {
            MessageTemplate = reminderDto.MessageTemplate,
            OffsetBeforeExecution = TimeSpan.FromMinutes(reminderDto.OffsetMinutes),
            OperationId = operationId,
        };
        await reminderRepository.InsertEntityAsync(reminder, cancellationToken);
        
        hangfireReminderJobHelper.CreateJobForReminder(operation, reminder);
        
        return Result.Ok(reminder);
    }

    public async Task<Result<Reminder>> UpdateReminderAsync(
        long telegramId, 
        long operationId, 
        UpdateReminderDto reminderDto,
        CancellationToken cancellationToken = default)
    {
        if (await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
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
        
        reminder.MessageTemplate = reminderDto.MessageTemplate;
        reminder.OffsetBeforeExecution = TimeSpan.FromMinutes(reminderDto.OffsetMinutes);
        await reminderRepository.UpdateEntityAsync(reminder, cancellationToken);
        
        hangfireReminderJobHelper.UpdateJobForReminder(operation, reminder);
        
        return Result.Ok(reminder);
    }

    public async Task<Result<Reminder>> DeleteReminderAsync(
        long telegramId, 
        long operationId, 
        long reminderId,
        CancellationToken cancellationToken = default)
    {
        if (await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
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
}