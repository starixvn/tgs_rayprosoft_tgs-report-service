using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player
{
    internal class
        ExportPlayerContributionsQueryHandler : IRequestHandler<ExportPlayerContributionsRequest, MemoryStream>
    {
        private readonly IMediator _mediator;

        public ExportPlayerContributionsQueryHandler(IMediator mediator) => _mediator = mediator;

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
                await _mediator.Send(getPlayerContributionsRequest, cancellationToken).ConfigureAwait(false);

            var exportData = playerContributions.Items?.Select(x => new ExportPlayerContributionModel
            {
                Username = x.Username,
                Type = x.Type,
                Level = x.Level,
                DepositAmount = x.DepositAmount,
                WithdrawalAmount = x.WithdrawalAmount,
                CompanyAdjustAmount = x.CompanyAdjustAmount,
                BonusAdjustAmount = x.BonusAdjustAmount,
                AgentAdjustAmount = x.AgentAdjustAmount,
                BonusAmount = x.BonusAmount,
                BetAmount = x.BetAmount,
                ValidBetAmount = x.ValidBetAmount,
                WinLossAmount = x.WinLossAmount,
                NetProfit = x.NetProfit
            });

            var xlsx = exportData?.ExportWithLocalization("playerContributions", resource);
            await using var stream = new MemoryStream();
            xlsx?.SaveAs(stream);

            return new MemoryStream(stream.ToArray());
        }
    }
}