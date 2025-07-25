using Grpc.Core;
using Reglamentator.Application.Abstractions;
using Reglamentator.Application.Dtos;

namespace Reglamentator.WebAPI.Mapping.Adapters;

public class GrpcNotificationStreamAdapter(
    IServerStreamWriter<NotificationResponse> grpcStream
    ): IStreamWriter<NotificationResponseDto>
{
    public async Task WriteAsync(NotificationResponseDto message)
    {
        var grpcMessage = new NotificationResponse
        {
            TelegramId = message.TelegramId,
            Message = message.Message
        };
        
        await grpcStream.WriteAsync(grpcMessage);
    }
}