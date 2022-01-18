namespace Tgs.Report.ViewModel.GameProvider
{
    public class GetGameProviderRoundsResponse
    {
        public PagedResponse<GameProviderRoundsModel> Datas { get; set; }

        public GameProviderRoundTotalModel[] Totals { get; set; }
    }
}