using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class ExportMasterAgentWinLossHandler : IRequestHandler<ExportMasterAgentWinLossRequest, MemoryStream>
{
    private readonly IMediator _mediator;

    public ExportMasterAgentWinLossHandler(IMediator mediator) => _mediator = mediator;

    public async Task<MemoryStream> Handle(
        ExportMasterAgentWinLossRequest request,
        CancellationToken cancellationToken)
    {
        var getMasterAgentWinLossRequest = new GetMasterAgentWinLossRequest
        {
            BrandId = request.BrandId,
            PlatformCharge = request.PlatformCharge,
            From = request.From,
            To = request.To,
            ShowAll = true
        };

        var agentContributions =
            await _mediator.Send(getMasterAgentWinLossRequest, cancellationToken).ConfigureAwait(false);

        var exportData = agentContributions.Items?.Select(x => new ExportAgentWinLossModel
        {
            Username = x.Username,
            MemberType = x.MemberType,
            RoundCount = x.RoundCount,
            Turnover = x.Turnover,
            WinLoss = x.WinLoss,
            Special = x.Special,
            AgentWinLoss = x.AgentWinLoss,
            AgentSpecial = x.AgentSpecial,
            AgentTotal = x.AgentTotal,
            CompanyWinLoss = x.CompanyWinLoss,
            CompanySpecial = x.CompanySpecial,
            CompanyTotal = x.CompanyTotal
        });

        var resource = new ResourceManager(typeof(AgentWinLossRes));

        var xlsx = exportData?.ExportWithLocalization("agentWinLoss", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}