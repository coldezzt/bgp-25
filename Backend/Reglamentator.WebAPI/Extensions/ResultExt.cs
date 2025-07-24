using AutoMapper;
using FluentResults;

namespace Reglamentator.WebAPI.Extensions;

public static class ResultExt
{
    public static StatusResponse ToStatusResponse<T>(this Result<T> result)
        => new()
        {
            IsSuccess = result.IsSuccess,
            StatusMessage = result.IsSuccess ? "Success" : result.Errors[0].Message
        };

    public static TV ToResponseData<T, TV>(this Result<T> result, IMapper mapper) 
        where T : class
        where TV : class, new()
        => result.IsSuccess ? mapper.Map<TV>(result.Value) : new TV();
    
    public static List<TV> ToResponseData<T, TV>(this Result<List<T>> result, IMapper mapper) 
        where T : class
        where TV : class
        => result.IsSuccess ? mapper.Map<List<TV>>(result.Value) : [];
}