using System.Text.Json;
using Tgs.Report.Client.Models;
using Tgs.Report.Client.Models.Agent;
using Tgs.Report.Client.Models.BrandGameProvider;
using Tgs.Report.Client.Models.GameAction;
using Tgs.Report.Client.Models.GameProvider;
using Tgs.Report.Client.Models.LicenseeMarket;
using Tgs.Report.Client.Models.Market;
using Tgs.Report.Client.Models.Marketing;
using Tgs.Report.Client.Models.Player;
using Tgs.Report.Client.Models.PlayerDeposit;
using Tgs.Report.Client.Models.ResellerMarket;
using Tgs.Report.Client.Models.Round;
using Tgs.Report.Client.Models.Vip;

namespace Tgs.Report.Client;

public interface IReportClient
{
    ValueTask<PagedResponse<VipModel>> GetVipReportAsync(GetVipReportRequest request);

    ValueTask<PagedResponse<VipRebateModel>> GetVipRebateReportAsync(GetVipRebateReportRequest request);

    ValueTask<PagedResponse<RoundModel>> GetRoundsAsync(GetRoundsRequest request);

    ValueTask<PagedResponse<SystemRoundModel>> GetSystemRoundsAsync(GetSystemRoundsRequest request);

    ValueTask<Stream> ExportSystemRoundsAsync(ExportSystemRoundsRequest request);

    ValueTask<RoundTotalModel> GetRoundsTotalAsync(GetRoundsTotalRequest request);

    ValueTask<SystemRoundTotalModel> GetSystemRoundsTotalAsync(GetSystemRoundsTotalRequest request);

    ValueTask<GetBetRecordsResponse> GetBetRecordsAsync(GetBetRecordsRequest request);

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

    ValueTask<PagedResponse<AgentDividendModel>> GetAgentDividendsAsync(GetAgentDividendsRequest request);

    ValueTask<PagedResponse<AgentDividendModel>> GetSingleAgentDividendsAsync(GetSingleAgentDividendsRequest request);

    ValueTask<PagedResponse<AgentDividendModel>> GetSubAgentDividendsAsync(GetSubAgentDividendsRequest request);

    ValueTask<PagedResponse<AgentPlayerWinLossModel>> GetAgentPlayerWinLossAsync(
        GetAgentPlayerWinLossRequest request);

    ValueTask<PagedResponse<AgentDividendDetailModel>> GetAgentDividendDetailsAsync(
        GetAgentDividendDetailsRequest request);

    ValueTask<PagedResponse<AgentDividendTeamDetailModel>> GetAgentDividendTeamDetailsAsync(
        GetAgentDividendTeamDetailsRequest request);

    ValueTask<PagedResponse<PlayerAccountChangeModel>> GetPlayerAccountChangesAsync(
        GetPlayerAccountChangesRequest request);

    ValueTask<decimal> GetPlayerAccountChangesTotalAmountAsync(
        GetPlayerAccountChangesTotalRequest request);

    ValueTask<PagedResponse<PlayersAccountChangeModel>> GetPlayersAccountChangesAsync(
        GetPlayersAccountChangesRequest request);

    ValueTask<PagedResponse<AgentAccountChangeModel>> GetAgentAccountChangesAsync(
        GetAgentAccountChangesRequest request);

    ValueTask<GetBrandGameProviderRoundsResponse> GetBrandGameProviderRoundsAsync(
        GetBrandGameProviderRoundsRequest request);

    ValueTask<PagedResponse<GameProviderRoundsModel>> GetGameProviderRoundsAsync(
        GetGameProviderRoundsRequest request);

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

    ValueTask<ResellerMarketsResponse> GetResellerMarketsAsync(GetResellerMarketsRequest request);

    ValueTask<Stream> ExportResellerMarketsAsync(ExportResellerMarketsRequest request);

    ValueTask<PagedResponse<PlayerDepositModel>> GetPlayerDepositsAsync(GetPlayerDepositsRequest request);

    ValueTask<PagedResponse<PlayerFirstDepositDetailModel>> GetPlayerFirstDepositDetailsAsync(GetPlayerFirstDepositDetailsRequest request);

    ValueTask<PagedResponse<PlayerNoSecondDepositDetailModel>> GetPlayerNoSecondDepositDetailsAsync(GetPlayerNoSecondDepositDetailsRequest request);

    ValueTask<PagedResponse<MarketingModel>> GetMarketingListAsync(GetMarketingListRequest request);

    ValueTask<PagedResponse<MarketingRegisterDetailModel>> GetMarketingRegisterDetailsAsync(GetMarketingRegisterDetailRequest request);

    ValueTask<PagedResponse<MarketingFirstDepositDetailModel>> GetMarketingFirstDepositDetailsAsync(GetMarketingFirstDepositDetailRequest request);

    ValueTask<PagedResponse<MarketingGamingDetailModel>> GetMarketingGamingDetailsAsync(GetMarketingGamingDetailRequest request);

    ValueTask<PlayerContributionTotalAmountModel> GetPlayerContributionsTotalAmountAsync(
        GetPlayerContributionsTotalAmountRequest request);

    ValueTask<VipTotalAmountModel> GetVipReportTotalAmountAsync(GetVipReportTotalAmountRequest request);
}