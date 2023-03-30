namespace Tgs.Report.Client.Models.Marketing;

public record MarketingGamingDetailModel
{
    public string Username { get; init; } = default!;
    public string GameProviderName { get; init; } = default!;
    public string SubGameProviderName { get; init; } = default!;
    public decimal WinLossAmount { get; init; }
}