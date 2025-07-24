using FluentResults;
using NCrontab;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;
using Reglamentator.Application.Errors;
using Reglamentator.Application.Extensions;
using Reglamentator.Domain.Entities;
using Reglamentator.Domain.Interfaces;

namespace Reglamentator.Application.Services;

/// <summary>
/// Реализация <see cref="IOperationService"/> сервиса для управления операциями пользователей.
/// </summary>
/// <remarks>
/// Использует:
/// <list type="bullet">
///   <item><see cref="ITelegramUserRepository"/> для доступа к данным <see cref="TelegramUser"/>.</item>
///   <item><see cref="IOperationRepository"/> для доступа к данным <see cref="Operation"/></item>
///   <item><see cref="IOperationInstanceRepository"/> для доступа к данным <see cref="OperationInstance"/></item>
///   <item><see cref="IHangfireOperationJobHelper"/> для управления задачами операций в Hangfire.</item>
/// </list>
/// </remarks>
public class OperationService(
    ITelegramUserRepository telegramUserRepository,
    IOperationRepository operationRepository,
    IOperationInstanceRepository operationInstanceRepository,
    IHangfireOperationJobHelper hangfireOperationJobHelper
    ): IOperationService
{
    /// <inheritdoc/>
    public async Task<Result<List<OperationInstance>>> GetPlanedOperationsAsync(
        long telegramId, 
        TimeRange range, 
        CancellationToken cancellationToken = default)
    {
        if (!await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));
        
        var planedOperations = await operationInstanceRepository
            .GetPlanedUserOperationsAsync(telegramId, range, cancellationToken);
        
        return Result.Ok(planedOperations);
    }

    /// <inheritdoc/>
    public async Task<Result<List<OperationInstance>>> GetOperationHistoryAsync(
        long telegramId, 
        CancellationToken cancellationToken = default)
    {
        if (!await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));

        var history = await operationInstanceRepository
            .GetExecutedUserOperationsAsync(telegramId, cancellationToken);
        
        return Result.Ok(history);
    }

    /// <inheritdoc/>
    public async Task<Result<Operation>> GetOperationAsync(long telegramId, long operationId, CancellationToken cancellationToken = default)
    {
        if (!await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));
        
        var operation = await operationRepository.GetWithRemindersAsync(
            op => op.Id == operationId, cancellationToken);
        
        if (operation == null)
            return Result.Fail(new NotFoundError(NotFoundError.OperationNotFound));

        if (operation.TelegramUserId != telegramId)
            return Result.Fail(new PermissionError(PermissionError.UserNotAllowedToOperation));
        
        return Result.Ok(operation);
    }

    /// <inheritdoc/>
    public async Task<Result<Operation>> CreateOperationAsync(
        long telegramId, 
        CreateOperationDto operationDto, 
        CancellationToken cancellationToken = default)
    {
        if (!await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));

        //Remove this check in case to allow past operation start date processing
        if (operationDto.StartDate < DateTime.UtcNow)
            return Result.Fail(new CreateOperationError(CreateOperationError.OperationStartDateCanNotBeInPast));
            
        var operation = CreateNewOperation(telegramId, operationDto);
        await operationRepository.InsertEntityAsync(operation, cancellationToken);
        
        hangfireOperationJobHelper.CreateJobsForOperation(operation);
        
        return Result.Ok(operation);
    }

    /// <inheritdoc/>
    public async Task<Result<Operation>> UpdateOperationAsync(
        long telegramId, 
        UpdateOperationDto operationDto, 
        CancellationToken cancellationToken = default)
    {
        if (!await telegramUserRepository.IsExistAsync(telegramId, cancellationToken))
            return Result.Fail(new NotFoundError(NotFoundError.UserNotFound));
        
        //Remove this check in case to allow past operation start date processing
        if (operationDto.StartDate < DateTime.UtcNow)
            return Result.Fail(new CreateOperationError(CreateOperationError.OperationStartDateCanNotBeInPast));
        
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

    /// <inheritdoc/>
    public async Task<Result<Operation>> DeleteOperationAsync(
        long telegramId, 
        long operationId, 
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

        await operationRepository.DeleteEntityAsync(operation, cancellationToken);

        hangfireOperationJobHelper.DeleteJobsForOperation(operation);
        
        return Result.Ok(operation);
    }

    /// <summary>
    /// Создает новую операцию на основе DTO.
    /// </summary>
    /// <param name="telegramId">Идентификатор пользователя.</param>
    /// <param name="operationDto">Данные операции.</param>
    /// <returns>Новый экземпляр операции.</returns>
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
        operation.NextOperationInstance = null;
        
        if (operation.Cron == null) 
            return operation;
        
        ProcessCronOperationCreation(operation);
        
        return operation;
    }
    
    /// <summary>
    /// Обновляет существующую операцию на основе DTO.
    /// </summary>
    /// <param name="operation">Операция для обновления.</param>
    /// <param name="operationDto">Данные для обновления.</param>
    private void UpdateOperation(Operation operation, UpdateOperationDto operationDto)
    {
        var now = DateTime.UtcNow;
        
        operation.Theme = operationDto.Theme;
        operation.Description = operationDto.Description;
        operation.Cron = operationDto.Cron.ToCronExpression(operationDto.StartDate);
        operation.StartDate = operationDto.StartDate;

        if (now <= operationDto.StartDate) 
            return;
        
        ProcessPastOperation(operation.NextOperationInstance, operationDto);
        operation.NextOperationInstance = null;
        
        if(operation.Cron == null)
            return;

        ProcessCronOperationUpdate(operation);
    }
    
    /// <summary>
    /// Закрывает выполнение операции с истекшей датой начала.
    /// </summary>
    /// <param name="operationInstance">Экземпляр операции.</param>
    /// <param name="operationDto">Данные операции.</param>
    private void ProcessPastOperation(OperationInstance operationInstance, CreateOperationDto operationDto)
    {
        operationInstance.ScheduledAt = operationDto.StartDate;
        operationInstance.Result = "Done";
        operationInstance.ExecutedAt = operationDto.StartDate;
    }
    
    /// <summary>
    /// Закрывает выполнение операции с истекшей датой начала при обновлении.
    /// </summary>
    /// <param name="operationInstance">Экземпляр операции.</param>
    /// <param name="operationDto">Данные операции.</param>
    private void ProcessPastOperation(OperationInstance? operationInstance, UpdateOperationDto operationDto)
    {
        if (operationInstance == null)
            return;
        
        operationInstance.ScheduledAt = operationDto.StartDate;
        operationInstance.Result = "Done";
        operationInstance.ExecutedAt = operationDto.StartDate;
    }

    /// <summary>
    /// Создает следующее выполнение для периодической операции.
    /// </summary>
    /// <param name="operation">Операция для обработки.</param>
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
    
    /// <summary>
    /// Обновляет следующее выполнение для периодической операции.
    /// </summary>
    /// <param name="operation">Операция для обработки.</param>
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

    /// <summary>
    /// Получает следующую дату выполнения для периодической операции.
    /// </summary>
    /// <param name="operation">Операция для расчета.</param>
    /// <returns>Дата следующего выполнения.</returns>
    private DateTime GetNextOccurrence(Operation operation)
    {
        var cronExpression = CrontabSchedule.Parse(
            operation.Cron, 
            new CrontabSchedule.ParseOptions {IncludingSeconds = true});
        var nextOccurrence = cronExpression.GetNextOccurrence(DateTime.UtcNow);
        
        return nextOccurrence;
    }
}