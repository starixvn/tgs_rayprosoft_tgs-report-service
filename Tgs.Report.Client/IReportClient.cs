using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Tgs.Report.Client.Models;
using Tgs.Report.Client.Models.Agent;
using Tgs.Report.Client.Models.GameAction;
using Tgs.Report.Client.Models.GameProvider;
using Tgs.Report.Client.Models.LicenseeMarket;
using Tgs.Report.Client.Models.Market;
using Tgs.Report.Client.Models.Player;
using Tgs.Report.Client.Models.Round;
using Tgs.Report.Client.Models.Vip;

namespace Tgs.Report.Client
{
    public interface IReportClient
    {
        ValueTask<PagedResponse<VipModel>> GetVipReportAsync(GetVipReportRequest request);

        ValueTask<PagedResponse<VipRebateModel>> GetVipRebateReportAsync(GetVipRebateReportRequest request);

        ValueTask<PagedResponse<RoundModel>> GetRoundsAsync(GetRoundsRequest request);

        ValueTask<PagedResponse<RoundModel>> GetAgentPlayerRoundsAsync(GetAgentPlayerRoundsRequest request);

        ValueTask<PagedResponse<OpenRoundModel>> GetOpenRoundsAsync(GetOpenRoundsRequest request);

        ValueTask<PagedResponse<ClosedRoundModel>> GetClosedRoundsAsync(GetClosedRoundsRequest request);

        ValueTask<JsonDocument?> GetRoundHistoriesAsync(GetRoundHistoryRequest request);

        ValueTask<PagedResponse<MarketModel>> GetMarketsAsync(GetMarketsRequest request);

        ValueTask<GetAdditionalExpensesResponse> GetAdditionalExpensesAsync(GetAdditionalExpensesRequest request);

        ValueTask<PagedResponse<AgentContributionModel>> GetAgentContributionsAsync(
            GetAgentContributionsRequest request);

        ValueTask<PagedResponse<PlayerContributionModel>> GetPlayerContributionsAsync(
            GetPlayerContributionsRequest request);

        ValueTask<PagedResponse<TeamInfoModel>> GetAgentTeamInfoAsync(GetAgentTeamInfoRequest request);

        ValueTask<PagedResponse<AgentDividendModel>> GetAgentDividendsAsync(GetAgentDividendsRequest request);

        ValueTask<PagedResponse<AgentPlayerWinLossModel>> GetAgentPlayerWinLossAsync(
            GetAgentPlayerWinLossRequest request);

        ValueTask<PagedResponse<AgentDividendDetailModel>> GetAgentDividendDetailsAsync(
            GetAgentDividendDetailsRequest request);

        ValueTask<PagedResponse<AgentDividendTeamDetailModel>> GetAgentDividendTeamDetailsAsync(
            GetAgentDividendTeamDetailsRequest request);

        ValueTask<PagedResponse<PlayerAccountChangeModel>> GetPlayerAccountChangesAsync(
            GetPlayerAccountChangesRequest request);

        ValueTask<PagedResponse<PlayersAccountChangeModel>> GetPlayersAccountChangesAsync(
            GetPlayersAccountChangesRequest request);

        ValueTask<PagedResponse<AgentAccountChangeModel>> GetAgentAccountChangesAsync(
            GetAgentAccountChangesRequest request);

        ValueTask<GetGameProviderRoundsResponse> GetGameProviderRoundsAsync(GetGameProviderRoundsRequest request);

        ValueTask<PagedResponse<GameActionModel>> GetGameActionsAsync(GetGameActionsRequest request);

        ValueTask<LicenseeMarketsResponse> GetLicenseeMarketsAsync(GetLicenseeMarketsRequest request);

        ValueTask<Stream> ExportMarketsAsync(ExportMarketsRequest request);

        ValueTask<Stream> ExportPlayerContributionsAsync(ExportPlayerContributionsRequest request);

        ValueTask<Stream> ExportAgentContributionsAsync(ExportAgentContributionsRequest request);

        ValueTask<Stream> ExportAgentDividendsAsync(ExportAgentDividendsRequest request);

        ValueTask<Stream> ExportVipsAsync(ExportVipReportRequest request);

        ValueTask<Stream>
            ExportAgentDividendsTrialCalculationAsync(ExportAgentDividendsTrialCalculationRequest request);

        ValueTask<Stream> ExportLicenseeMarketsAsync(ExportLicenseeMarketsRequest request);
    }
}