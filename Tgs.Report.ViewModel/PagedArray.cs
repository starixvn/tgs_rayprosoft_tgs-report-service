namespace Tgs.Report.ViewModel;

public class PagedArray<T>(int totalCount, T[] items)
{
    public int TotalCount { get; } = totalCount;

    public T[]? Items { get; } = items;
}