using FluentResults;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Errors;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Services;

public class OperationService(
    ITelegramUserRepository telegramUserRepository,
    IOperationRepository operationRepository,
    IOperationInstanceRepository operationInstanceRepository,
    IHangfireOperationJobHelper hangfireOperationJobHelper
    ): IOperationService
{
    public async Task<Result<List<OperationInstance>>> GetPlanedOperationsAsync(
        long telegramId, 
        TimeRange range, 
        CancellationToken cancellationToken = default)
    {
        if (await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));
        
        var planedOperations = await operationInstanceRepository
            .GetPlanedUserOperationsAsync(telegramId, range, cancellationToken);
        
        return Result.Ok(planedOperations);
    }

    public async Task<Result<List<OperationInstance>>> GetOperationHistoryAsync(
        long telegramId, 
        CancellationToken cancellationToken = default)
    {
        if (await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));

        var history = await operationInstanceRepository
            .GetExecutedUserOperationsAsync(telegramId, cancellationToken);
        
        return Result.Ok(history);
    }

    public async Task<Result<Operation>> CreateOperationAsync(
        long telegramId, 
        CreateOperationDto operationDto, 
        CancellationToken cancellationToken = default)
    {
        if (await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));

        var operation = CreateNewOperation(telegramId, operationDto);
        await operationRepository.InsertEntityAsync(operation, cancellationToken);
        
        hangfireOperationJobHelper.CreateJobsForOperation(operation);
        
        return Result.Ok(operation);
    }

    public async Task<Result<Operation>> UpdateOperationAsync(
        long telegramId, 
        UpdateOperationDto operationDto, 
        CancellationToken cancellationToken = default)
    {
        if (await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));
        
        var operation = await operationRepository.GetEntityByFilterAsync(
            op => op.Id == operationDto.Id, cancellationToken);
        
        if (operation == null)
            return Result.Fail(new NotFoundError(NotFoundError.OperationNotFound));

        if (operation.TelegramUserId != telegramId)
            return Result.Fail(new PermissionError(PermissionError.UserNotAllowedToOperation));

        UpdateOperationByDto(operation, operationDto);
        await operationRepository.UpdateEntityAsync(operation, cancellationToken);
        
        hangfireOperationJobHelper.UpdateJobsForOperation(operation);
        
        return Result.Ok(operation);
    }

    public async Task<Result<Operation>> DeleteOperationAsync(
        long telegramId, 
        long operationId, 
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

        await operationRepository.DeleteEntityAsync(operation, cancellationToken);

        hangfireOperationJobHelper.DeleteJobsForOperation(operation);
        
        return Result.Ok(operation);
    }

    private Operation CreateNewOperation(long telegramId, CreateOperationDto operationDto)
    {
        var operationInstance = new OperationInstance
        {
            ScheduledAt = DateTime.UtcNow,
            Result = null,
            ExecutedAt = null
        };
        var operation = new Operation
        {
            Theme = operationDto.Theme,
            Description = operationDto.Description,
            StartDate = operationDto.StartDate,
            Cron = operationDto.Cron,
            TelegramUserId = telegramId,
            Reminders = operationDto.Reminders.Select(r => new Reminder
            {
                MessageTemplate = r.MessageTemplate,
                OffsetBeforeExecution = TimeSpan.FromMinutes(r.OffsetMinutes)
            }).ToList(),
            NextOperationInstance = operationInstance,
            History = [operationInstance]
        };
        return operation;
    }

    private void UpdateOperationByDto(Operation operation, UpdateOperationDto operationDto)
    {
        operation.Theme = operationDto.Theme;
        operation.Description = operationDto.Description;
        operation.StartDate = operationDto.StartDate;
        operation.Cron = operationDto.Cron;
    }
}