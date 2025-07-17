using Grpc.Core;

namespace Reglamentator.WebAPI.Services;

public class UserGrpcService: User.UserBase
{
    public override Task<CreateUserResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
    {
        return base.CreateUser(request, context);
    }
}