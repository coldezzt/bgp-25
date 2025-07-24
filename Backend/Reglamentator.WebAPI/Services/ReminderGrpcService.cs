using AutoMapper;
using Grpc.Core;
using Reglamentator.Application.Abstractions;
using Reglamentator.WebAPI.Extensions;

namespace Reglamentator.WebAPI.Services;

public class ReminderGrpcService(
    IReminderService reminderService,
    IMapper mapper
    ): Reminder.ReminderBase
{
    public override async Task<ReminderResponse> AddReminder(AddReminderRequest request, ServerCallContext context)
    {
        var result = await reminderService.AddReminderAsync(
            request.TelegramId,
            request.OperationId,
            mapper.Map<Application.Dtos.CreateReminderDto>(request.Reminder),
            context.CancellationToken);

        return new ReminderResponse()
        {
            Status = result.ToStatusResponse(),
            Reminder = result.IsSuccess ? mapper.Map<ReminderDto>(result.Value) : new ReminderDto()
        };
    }

    public override async Task<ReminderResponse> UpdateOperation(UpdateReminderRequest request, ServerCallContext context)
    {
        var result = await reminderService.UpdateReminderAsync(
            request.TelegramId,
            request.OperationId,
            mapper.Map<Application.Dtos.UpdateReminderDto>(request.Reminder),
            context.CancellationToken);

        return new ReminderResponse()
        {
            Status = result.ToStatusResponse(),
            Reminder = result.IsSuccess ? mapper.Map<ReminderDto>(result.Value) : new ReminderDto()
        };
    }

    public override async Task<ReminderResponse> DeleteOperation(DeleteReminderRequest request, ServerCallContext context)
    {
        var result = await reminderService.DeleteReminderAsync(
            request.TelegramId,
            request.OperationId,
            request.ReminderId,
            context.CancellationToken);

        return new ReminderResponse()
        {
            Status = result.ToStatusResponse(),
            Reminder = result.IsSuccess ? mapper.Map<ReminderDto>(result.Value) : new ReminderDto()
        };
    }
}