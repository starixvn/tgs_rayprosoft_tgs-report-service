using System.Resources;
using MediatR;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market;

internal class ExportMarketsQueryHandler : IRequestHandler<ExportMarketsRequest, MemoryStream>
{
    private readonly IMediator _mediator;

    public ExportMarketsQueryHandler(IMediator mediator) => _mediator = mediator;

    public async Task<MemoryStream> Handle(
        ExportMarketsRequest request,
        CancellationToken cancellationToken)
    {
        var getMarketsRequest = new GetMarketsRequest
        {
            BrandId = request.BrandId,
            From = request.From,
            To = request.To,
            ShowAll = true
        };

        var markets = await _mediator.Send(getMarketsRequest, cancellationToken).ConfigureAwait(false);

        var exportData = markets.Items?.Select(x => new ExportMarketModel
        {
            ReportDate = x.ReportDate.ToOffset(new(8, 0, 0)).ToString("d"),
            RegisteredCount = x.RegisteredCount,
            FirstFundInCount = x.FirstFundInCount,
            DepositAmount = x.DepositAmount,
            DepositHandlingFee = x.DepositHandlingFee,
            WithdrawalAmount = x.WithdrawalAmount,
            CompanyAdjustAmount = x.CompanyAdjustAmount,
            BonusAdjustAmount = x.BonusAdjustAmount,
            BonusAmount = x.BonusAmount,
            BetAmount = x.BetAmount,
            ValidBetAmount = x.ValidBetAmount,
            WinLossAmount = x.WinLossAmount,
            NetProfit = x.NetProfit,
            AgentProfit = x.AgentProfit
        });

        var resource = new ResourceManager(typeof(Resources.Market));

        var xlsx = exportData?.ExportWithLocalization("markets", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}