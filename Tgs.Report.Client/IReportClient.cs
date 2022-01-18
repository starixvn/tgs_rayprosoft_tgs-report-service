using System.IO;
using System.Threading.Tasks;
using Tgs.Report.Client.Models.Agent;
using Tgs.Report.Client.Models.GameAction;
using Tgs.Report.Client.Models.GameProvider;
using Tgs.Report.Client.Models.LicenseeMarket;
using Tgs.Report.Client.Models.Market;
using Tgs.Report.Client.Models.Player;
using Tgs.Report.Client.Models.Round;
using Tgs.Report.Client.Models.Vip;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client
{
    public interface IReportClient
    {
        Task<PagedResponse<VipModel>> GetVipReportAsync(GetVipReportRequest request);

        Task<PagedResponse<VipRebateModel>> GetVipRebateReportAsync(GetVipRebateReportRequest request);

        Task<PagedResponse<RoundModel>> GetRoundsAsync(GetRoundsRequest request);
        Task<PagedResponse<OpenRoundModel>> GetOpenRoundsAsync(GetOpenRoundsRequest request);
        Task<PagedResponse<ClosedRoundModel>> GetClosedRoundsAsync(GetClosedRoundsRequest request);

        Task<GetRoundHistoryResponse> GetRoundHistoriesAsync(GetRoundHistoryRequest request);

        Task<PagedResponse<MarketModel>> GetMarketsAsync(GetMarketsRequest request);

        Task<GetAdditionalExpensesResponse> GetAdditionalExpensesAsync(GetAdditionalExpensesRequest request);
        Task<PagedResponse<AgentContributionModel>> GetAgentContributionsAsync(GetAgentContributionsRequest request);

        Task<PagedResponse<PlayerContributionModel>> GetPlayerContributionsAsync(GetPlayerContributionsRequest request);

        Task<PagedResponse<TeamInfoModel>> GetAgentTeamInfoAsync(GetAgentTeamInfoRequest request);

        Task<PagedResponse<AgentDividendModel>> GetAgentDividendsAsync(GetAgentDividendsRequest request);

        Task<PagedResponse<AgentPlayerWinLossModel>> GetAgentPlayerWinLossAsync(GetAgentPlayerWinLossRequest request);

        Task<PagedResponse<AgentDividendDetailModel>> GetAgentDividendDetailsAsync(
            GetAgentDividendDetailsRequest request);

        Task<PagedResponse<AgentDividendTeamDetailModel>> GetAgentDividendTeamDetailsAsync(
            GetAgentDividendTeamDetailsRequest request);

        Task<PagedResponse<PlayerAccountChangeModel>> GetPlayerAccountChangesAsync(
            GetPlayerAccountChangesRequest request);

        Task<PagedResponse<PlayersAccountChangeModel>> GetPlayersAccountChangesAsync(
            GetPlayersAccountChangesRequest request);

        Task<PagedResponse<AgentAccountChangeModel>> GetAgentAccountChangesAsync(GetAgentAccountChangesRequest request);
        Task<GetGameProviderRoundsResponse> GetGameProviderRoundsAsync(GetGameProviderRoundsRequest request);
        Task<PagedResponse<GameActionModel>> GetGameActionsAsync(GetGameActionsRequest request);

        Task<LicenseeMarketsResponse> GetLicenseeMarketsAsync(GetLicenseeMarketsRequest request);

        Task<Stream> ExportMarketsAsync(ExportMarketsRequest request);

        Task<Stream> ExportPlayerContributionsAsync(ExportPlayerContributionsRequest request);

        Task<Stream> ExportAgentContributionsAsync(ExportAgentContributionsRequest request);

        Task<Stream> ExportAgentDividendsAsync(ExportAgentDividendsRequest request);

        Task<Stream> ExportVipsAsync(ExportVipReportRequest request);

        Task<Stream> ExportAgentDividendsTrialCalculationAsync(ExportAgentDividendsTrialCalculationRequest request);

        Task<Stream> ExportLicenseeMarketsAsync(ExportLicenseeMarketsRequest request);
    }
}