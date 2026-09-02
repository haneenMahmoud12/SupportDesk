namespace SupportDesk.Application.Models;

public class ResponseModel
{
    public bool Succeeded { get; init; }
    public IReadOnlyCollection<string> Errors { get; init; } = [];
}

public sealed class ResponseModel<T> : ResponseModel
{
    public T? Data { get; init; }
}
