using Tgs.Report.Client.Models.Player;

namespace Tgs.Report.Client;

public interface IReportPlayerAllAgentClient
{
    IAsyncEnumerable<PlayerAllAgentRebateModel> GetPlayerAllAgentRebatesAsync(GetPlayerAllAgentRebatesRequest request);
    Task<PlayerAllAgentRebateTotalAmountModel> GetPlayerAllAgentRebatesTotalAmountAsync(GetPlayerAllAgentRebateTotalAmountRequest request);
    IAsyncEnumerable<PlayerAllAgentRebateDetailGameTypeModel> GetPlayerAllAgentRebateDetailGameTypesAsync(GetPlayerAllAgentRebateDetailGameTypesRequest request);
    IAsyncEnumerable<PlayerAllAgentRebateDetailModel> GetPlayerAllAgentRebateDetailAsync(GetPlayerAllAgentRebatesDetailsRequest request);
}