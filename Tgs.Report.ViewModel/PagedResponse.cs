namespace Tgs.Report.ViewModel
{
    public record PagedResponse<T> : MessageResponse
    {
        public int TotalCount { get; init; }
        public T[] Items { get; init; }

        public PagedResponse()
        {
        }

        public PagedResponse(int count, T[] items)
        {
            TotalCount = count;
            Items = items;
        }
    }
}