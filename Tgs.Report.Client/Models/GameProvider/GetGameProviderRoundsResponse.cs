using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.GameProvider
{
    public class GetGameProviderRoundsResponse
    {
        public PagedResponse<GameProviderRoundsModel> Datas { get; set; }

        public GameProviderRoundTotalModel[] Totals { get; set; }
    }
}