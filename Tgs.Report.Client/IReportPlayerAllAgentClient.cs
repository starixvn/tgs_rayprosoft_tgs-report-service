using Tgs.Report.Client.Models.Player;

namespace Tgs.Report.Client;

public interface IReportPlayerAllAgentClient
{
    IAsyncEnumerable<PlayerAllAgentRebateModel> GetPlayerAllAgentRebatesAsync(GetPlayerAllAgentRebatesRequest request);
    Task<PlayerAllAgentRebateTotalAmountModel> GetPlayerAllAgentRebatesTotalAmountAsync(GetPlayerAllAgentRebateTotalAmountRequest request);
    IAsyncEnumerable<PlayerAllAgentGameTypeModel> GetPlayerAllAgentGameTypesAsync(GetPlayerAllAgentGameTypesRequest request);
    IAsyncEnumerable<PlayerAllAgentRebateDetailModel> GetPlayerAllAgentRebateDetailAsync(GetPlayerAllAgentRebatesDetailsRequest request);
}