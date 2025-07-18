using Grpc.Core;
using Reglamentator.Application.Dtos;

namespace Reglamentator.WebAPI.Adapters;

public class GrpcNotificationStreamAdapter(
    IServerStreamWriter<NotificationResponse> grpcStream
    ): IServerStreamWriter<NotificationResponseDto>
{
    public WriteOptions? WriteOptions { get; set; }

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