using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    internal class ExportAgentContributionsQueryHandler : IRequestHandler<ExportAgentContributionsRequest, MemoryStream>
    {
        private readonly IMediator _mediator;

        public ExportAgentContributionsQueryHandler(IMediator mediator) => _mediator = mediator;

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
                await _mediator.Send(getAgentContributionsRequest, cancellationToken).ConfigureAwait(false);

            var exportData = agentContributions.Items?.Select(x => new ExportAgentContributionModel
            {
                Username = x.Username,
                Type = x.Type,
                DepositAmount = x.DepositAmount,
                WithdrawalAmount = x.WithdrawalAmount,
                BonusAmount = x.BonusAmount,
                CompanyAdjustAmount = x.CompanyAdjustAmount,
                AgentAdjustAmount = x.AgentAdjustAmount,
                BetAmount = x.BetAmount,
                ValidBetAmount = x.ValidBetAmount,
                WinLossAmount = x.WinLossAmount,
                NetProfit = x.NetProfit
            });

            var resource = new ResourceManager(typeof(AgentContribution));

            var xlsx = exportData.ExportWithLocalization("agentContributions", resource);

            await using var stream = new MemoryStream();

            xlsx.SaveAs(stream);

            return new MemoryStream(stream.ToArray());
        }
    }
}