namespace Tgs.Report.Client.Models.GameProvider
{
    public record GetGameProviderRoundsResponse
    {
        public PagedResponse<GameProviderRoundsModel> Datas { get; init; }

        public GameProviderRoundTotalModel[] Totals { get; init; }
    }
}