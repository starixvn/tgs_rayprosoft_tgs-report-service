namespace Tgs.Report.Client.Models.Market;

public record GetAdditionalExpensesRequest
{
    public Guid BrandId { get; init; }

    public DateTimeOffset ReportDate { get; init; }
}