namespace Tgs.Report.Client.Models;

public record PagedResponse<T>(int TotalCount, T[] Items) : MessageResponse
{
    public PagedResponse() : this(0, Array.Empty<T>())
    {
    }
}