using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    internal class ExportAgentDividendsQueryHandler : IRequestHandler<ExportAgentDividendsRequest, MemoryStream>
    {
        private readonly IMediator _mediator;

        public ExportAgentDividendsQueryHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<MemoryStream> Handle(
            ExportAgentDividendsRequest request,
            CancellationToken cancellationToken)
        {
            var getAgentDividendsRequest = new GetAgentDividendsRequest
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
                await _mediator.Send(getAgentDividendsRequest, cancellationToken).ConfigureAwait(false);

            var exportData = agentDividends.Items?.Select(x => new ExportAgentDividendModel
            {
                Username = x.Username,
                DepositAmount = x.DepositAmount,
                WithdrawalAmount = x.WithdrawalAmount,
                BonusAmount = x.BonusAmount,
                AdjustmentAmount = x.AdjustmentAmount,
                BetAmount = x.BetAmount,
                ValidBetAmount = x.ValidBetAmount,
                WinLossAmount = x.WinLossAmount,
                NetProfit = x.NetProfit,
                AgentDividendAmount = x.AgentDividendAmount
            });

            var resource = new ResourceManager(typeof(AgentDividend));

            var xlsx = exportData.ExportWithLocalization("agentContributions", resource);

            await using var stream = new MemoryStream();

            xlsx.SaveAs(stream);

            return new MemoryStream(stream.ToArray());
        }
    }
}