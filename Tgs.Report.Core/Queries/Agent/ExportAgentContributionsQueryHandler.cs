using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

internal class ExportAgentContributionsQueryHandler(IMediator mediator)
    : IRequestHandler<ExportAgentContributionsRequest, MemoryStream>
{
    public async Task<MemoryStream> Handle(
        ExportAgentContributionsRequest request,
        CancellationToken cancellationToken)
    {
        var getAgentContributionsRequest = new GetAgentContributionsRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            Username = request.Username,
            Type = request.Type,
            From = request.From,
            To = request.To,
            IncludeAgents = request.IncludeAgents,
            ShowAll = true
        };

        var agentContributions =
            await mediator.Send(getAgentContributionsRequest, cancellationToken).ConfigureAwait(false);

        var exportData = agentContributions.Items?.Select(x => new ExportAgentContributionModel
        {
            Username = x.Username,
            Type = x.Type,
            DepositAmount = (x.DepositAmount - x.DepositHandlingFee).Truncate(2),
            WithdrawalAmount = x.WithdrawalAmount.Truncate(2),
            BonusAmount = x.BonusAmount.Truncate(2),
            CompanyAdjustAmount = x.CompanyAdjustAmount.Truncate(2),
            AgentAdjustAmount = x.AgentAdjustAmount.Truncate(2),
            BonusAdjustAmount = x.BonusAdjustAmount.Truncate(2),
            BetAmount = x.BetAmount.Truncate(2),
            ValidBetAmount = x.ValidBetAmount.Truncate(2),
            WinLossAmount = x.WinLossAmount.Truncate(2),
            NetProfit = x.NetProfit.Truncate(2)
        });

        var resource = new ResourceManager(typeof(AgentContribution));

        var xlsx = exportData?.ExportWithLocalization("agentContributions", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}