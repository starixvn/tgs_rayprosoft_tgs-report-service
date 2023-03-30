using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent;

internal class ExportAgentDividendsQueryHandler : IRequestHandler<ExportAgentDividendsRequest, MemoryStream>
{
    private readonly IMediator _mediator;

    public ExportAgentDividendsQueryHandler(IMediator mediator) => _mediator = mediator;

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
            await _mediator.Send(singleAgentDividendsRequest, cancellationToken).ConfigureAwait(false);

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
            await _mediator.Send(subAgentDividendsRequest, cancellationToken).ConfigureAwait(false);

        var dividends = agentDividends.Items != null
            ? mainAgentDividends.Items?.Concat(agentDividends.Items)
            : mainAgentDividends.Items;

        var exportData = dividends?.Select(x => new ExportAgentDividendModel
        {
            Username = x.Username,
            BonusAmount = x.BonusAmount,
            AgentAdjustAmount = x.AgentAdjustAmount,
            BetAmount = x.BetAmount,
            ValidBetAmount = x.ValidBetAmount,
            WinLossAmount = x.WinLossAmount,
            NetProfit = x.NetProfit,
            AgentDividendAmount = x.AgentDividendAmount
        });

        var resource = new ResourceManager(typeof(AgentDividend));

        var xlsx = exportData?.ExportWithLocalization("agentDividends", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}