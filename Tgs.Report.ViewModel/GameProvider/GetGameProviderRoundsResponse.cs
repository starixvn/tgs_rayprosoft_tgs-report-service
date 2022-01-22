namespace Tgs.Report.ViewModel.GameProvider
{
    public record GetGameProviderRoundsResponse
    {
        public PagedResponse<GameProviderRoundsModel> Datas { get; init; } = default!;

        public GameProviderRoundTotalModel[] Totals { get; init; } = default!;
    }
}