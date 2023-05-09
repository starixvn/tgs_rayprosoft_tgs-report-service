using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class ExportAgentWinLossHandler : IRequestHandler<ExportAgentWinLossRequest, MemoryStream>
{
    private readonly IMediator _mediator;

    public ExportAgentWinLossHandler(IMediator mediator) => _mediator = mediator;

    public async Task<MemoryStream> Handle(
        ExportAgentWinLossRequest request,
        CancellationToken cancellationToken)
    {
        var getAgentWinLossRequest = new GetAgentWinLossRequest
        {
            BrandId = request.BrandId,
            AgentId = request.AgentId,
            PlatformCharge = request.PlatformCharge,
            From = request.From,
            To = request.To,
            ShowAll = true
        };

        var agentContributions =
            await _mediator.Send(getAgentWinLossRequest, cancellationToken).ConfigureAwait(false);

        var exportData = agentContributions.Items?.Select(x => new ExportAgentWinLossModel
        {
            Username = x.Username,
            MemberType = x.MemberType,
            RoundCount = x.RoundCount,
            Turnover = x.Turnover,
            WinLoss = x.WinLoss,
            AgentWinLoss = x.AgentWinLoss,
            CompanyWinLoss = x.CompanyWinLoss,
            Special = x.Special,
            AgentSpecial = x.AgentSpecial,
            CompanySpecial = x.CompanySpecial,
            AgentTotal = x.AgentTotal,
            CompanyTotal = x.CompanyTotal
        });

        var resource = new ResourceManager(typeof(AgentWinLossRes));

        var xlsx = exportData?.ExportWithLocalization("agentWinLoss", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}