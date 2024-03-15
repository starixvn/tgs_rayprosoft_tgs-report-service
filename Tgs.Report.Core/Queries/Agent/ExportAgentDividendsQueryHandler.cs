using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

internal class ExportAgentDividendsQueryHandler(IMediator mediator)
    : IRequestHandler<ExportAgentDividendsRequest, MemoryStream>
{
    public async Task<MemoryStream> Handle(
        ExportAgentDividendsRequest request,
        CancellationToken cancellationToken)
    {
        var singleAgentDividendsRequest = new GetSingleAgentDividendsRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            Username = request.Username,
            Type = request.Type,
            From = request.From,
            To = request.To,
            ShowAll = true
        };

        var mainAgentDividends =
            await mediator.Send(singleAgentDividendsRequest, cancellationToken).ConfigureAwait(false);

        var subAgentDividendsRequest = new GetSubAgentDividendsRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            Username = request.Username,
            Type = request.Type,
            From = request.From,
            To = request.To,
            ShowAll = true
        };

        var agentDividends =
            await mediator.Send(subAgentDividendsRequest, cancellationToken).ConfigureAwait(false);

        var dividends = agentDividends.Items != null
            ? mainAgentDividends.Items?.Concat(agentDividends.Items)
            : mainAgentDividends.Items;

        var exportData = dividends?.Select(x => new ExportAgentDividendModel
        {
            Username = x.Username,
            BonusAmount = x.BonusAmount.Truncate(2),
            AgentAdjustAmount = x.AgentAdjustAmount.Truncate(2),
            BetAmount = x.BetAmount.Truncate(2),
            ValidBetAmount = x.ValidBetAmount.Truncate(2),
            WinLossAmount = x.WinLossAmount.Truncate(2),
            NetProfit = x.NetProfit.Truncate(2),
            AgentDividendAmount = x.AgentDividendAmount.Truncate(2)
        });

        var resource = new ResourceManager(typeof(AgentDividend));

        var xlsx = exportData?.ExportWithLocalization("agentDividends", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}