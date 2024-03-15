using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player;

internal class
    ExportPlayerContributionsQueryHandler(IMediator mediator)
    : IRequestHandler<ExportPlayerContributionsRequest, MemoryStream>
{
    public async Task<MemoryStream> Handle(
        ExportPlayerContributionsRequest request,
        CancellationToken cancellationToken)
    {
        var getPlayerContributionsRequest = new GetPlayerContributionsRequest
        {
            BrandId = request.BrandId,
            From = request.From,
            To = request.To,
            Page = 1,
            ShowAll = true,
            Username = request.Username,
            Level = request.Level
        };

        var resource = new ResourceManager(typeof(PlayerContribution));

        var playerContributions =
            await mediator.Send(getPlayerContributionsRequest, cancellationToken).ConfigureAwait(false);

        var exportData = playerContributions.Items?.Select(x => new ExportPlayerContributionModel
        {
            Username = x.Username,
            Type = x.Type,
            Level = x.Level,
            DepositAmount = (x.DepositAmount - x.DepositHandlingFee).Truncate(2),
            WithdrawalAmount = x.WithdrawalAmount.Truncate(2),
            CompanyAdjustAmount = x.CompanyAdjustAmount.Truncate(2),
            BonusAdjustAmount = x.BonusAdjustAmount.Truncate(2),
            AgentAdjustAmount = x.AgentAdjustAmount.Truncate(2),
            BonusAmount = x.BonusAmount.Truncate(2),
            BetAmount = x.BetAmount.Truncate(2),
            ValidBetAmount = x.ValidBetAmount.Truncate(2),
            WinLossAmount = x.WinLossAmount.Truncate(2),
            NetProfit = x.NetProfit.Truncate(2)
        });

        var xlsx = exportData?.ExportWithLocalization("playerContributions", resource);
        await using var stream = new MemoryStream();
        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}