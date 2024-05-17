using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.Core.Queries.AgentWinLoss;

internal class ExportMasterAgentWinLossHandler(IMediator mediator)
    : IRequestHandler<ExportMasterAgentWinLossRequest, MemoryStream>
{
    public async Task<MemoryStream> Handle(
        ExportMasterAgentWinLossRequest request,
        CancellationToken cancellationToken)
    {
        var getMasterAgentWinLossRequest = new GetMasterAgentWinLossRequest
        {
            BrandId = request.BrandId,
            From = request.From,
            To = request.To,
            ShowAll = true
        };

        var agentContributions =
            await mediator.Send(getMasterAgentWinLossRequest, cancellationToken).ConfigureAwait(false);

        var exportData = agentContributions.Items?.Select(x => new ExportAgentWinLossModel
        {
            Username = x.Username,
            MemberType = x.MemberType,
            RoundCount = x.RoundCount,
            Turnover = x.Turnover.Truncate(2),
            WinLoss = x.WinLoss.Truncate(2),
            AgentWinLoss = x.AgentWinLoss.Truncate(2),
            CompanyWinLoss = x.CompanyWinLoss.Truncate(2),
            Special = x.Special.Truncate(2),
            AgentSpecial = x.AgentSpecial.Truncate(2),
            CompanySpecial = x.CompanySpecial.Truncate(2),
            AgentTotal = x.AgentTotal.Truncate(2),
            CompanyTotal = x.CompanyTotal.Truncate(2),
            PlatformChargeAmount = x.PlatformChargeAmount.Truncate(2),
        });

        var resource = new ResourceManager(typeof(AgentWinLossRes));

        var xlsx = exportData?.ExportWithLocalization("agentWinLoss", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}