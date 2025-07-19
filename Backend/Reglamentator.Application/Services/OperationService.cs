using FluentResults;
using NCrontab;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Errors;
using Reglamentator.Application.Extensions;
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
        
        var operation = await operationRepository.GetWithDetailsForProcessJobAsync(
            op => op.Id == operationDto.Id, cancellationToken);
        
        if (operation == null)
            return Result.Fail(new NotFoundError(NotFoundError.OperationNotFound));

        if (operation.TelegramUserId != telegramId)
            return Result.Fail(new PermissionError(PermissionError.UserNotAllowedToOperation));

        UpdateOperation(operation, operationDto);
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
        var now = DateTime.UtcNow;
        var operationInstance = new OperationInstance
        {
            ScheduledAt = now,
            Result = null,
            ExecutedAt = null
        };
        var operation = new Operation
        {
            Theme = operationDto.Theme,
            Description = operationDto.Description,
            StartDate = operationDto.StartDate,
            Cron = operationDto.Cron.ToCronExpression(operationDto.StartDate),
            TelegramUserId = telegramId,
            NextOperationInstance = operationInstance,
            History = [operationInstance]
        };
        if (now <= operationDto.StartDate) 
            return operation;
        
        ProcessPastOperation(operationInstance, operationDto);
        
        if (operation.Cron == null) 
            return operation;
        
        ProcessCronOperationCreation(operation);
        
        return operation;
    }
    
    private void UpdateOperation(Operation operation, UpdateOperationDto operationDto)
    {
        var now = DateTime.UtcNow;
        
        operation.Theme = operationDto.Theme;
        operation.Description = operationDto.Description;
        operation.Cron = operationDto.Cron.ToCronExpression(operationDto.StartDate);
        
        if (now > operationDto.StartDate)
        {
            ProcessPastOperation(operation.NextOperationInstance, operationDto);
            if (operation.Cron != null)
            {
                ProcessCronOperationUpdate(operation);
            }
        }
        
        operation.StartDate = operationDto.StartDate;
    }
    
    private void ProcessPastOperation(OperationInstance operationInstance, CreateOperationDto operationDto)
    {
        operationInstance.ScheduledAt = operationDto.StartDate;
        operationInstance.Result = "Done";
        operationInstance.ExecutedAt = operationDto.StartDate;
    }
    
    private void ProcessPastOperation(OperationInstance operationInstance, UpdateOperationDto operationDto)
    {
        operationInstance.ScheduledAt = operationDto.StartDate;
        operationInstance.Result = "Done";
        operationInstance.ExecutedAt = operationDto.StartDate;
    }

    private void ProcessCronOperationCreation(Operation operation)
    {
        var now = DateTime.UtcNow;
        operation.StartDate = GetNextOccurrence(operation);

        var newOperationInstance = new OperationInstance
        {
            ScheduledAt = now,
            Result = null,
            ExecutedAt = null
        };
        
        operation.NextOperationInstance = newOperationInstance;
        operation.History.Add(newOperationInstance);
    }
    
    private void ProcessCronOperationUpdate(Operation operation)
    {
        var now = DateTime.UtcNow;
        operation.StartDate = GetNextOccurrence(operation);

        var newOperationInstance = new OperationInstance
        {
            ScheduledAt = now,
            OperationId = operation.Id,
            Result = null,
            ExecutedAt = null
        };
        
        operation.NextOperationInstance = newOperationInstance;
    }

    private DateTime GetNextOccurrence(Operation operation)
    {
        var cronExpression = CrontabSchedule.Parse(operation.Cron);
        var nextOccurrence = cronExpression.GetNextOccurrence(operation.StartDate);
        
        return nextOccurrence;
    }
}