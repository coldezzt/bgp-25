namespace Reglamentator.Application.Abstractions;

public interface IStreamWriter<in T>
{
    Task WriteAsync(T message);
}