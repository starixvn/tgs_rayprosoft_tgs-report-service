namespace Tgs.Report.Client.Models.Agent;

public record AgentDividendDetailModel
{
    public Guid Id { get; init; }
    public string Username { get; init; } = default!;
    public DateTimeOffset SettlementDate { get; init; }
    public DateTimeOffset SettlementDateFrom { get; init; }
    public DateTimeOffset SettlementDateTo { get; init; }
    public decimal AgentDividendAmount { get; init; }
    public decimal ExpenditureAmount { get; init; }
}