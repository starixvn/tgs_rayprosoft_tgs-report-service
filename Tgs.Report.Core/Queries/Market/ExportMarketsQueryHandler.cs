using System.Resources;
using MediatR;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.Core.Queries.Market;

internal class ExportMarketsQueryHandler(IMediator mediator) : IRequestHandler<ExportMarketsRequest, MemoryStream>
{
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

        var markets = await mediator.Send(getMarketsRequest, cancellationToken).ConfigureAwait(false);

        var exportData = markets.Items?.Select(x => new ExportMarketModel
        {
            ReportDate = x.ReportDate.ToOffset(new(8, 0, 0)).ToString("d"),
            RegisteredCount = x.RegisteredCount,
            FirstFundInCount = x.FirstFundInCount,
            DepositAmount = (x.DepositAmount - x.DepositHandlingFee).Truncate(2),
            WithdrawalAmount = x.WithdrawalAmount.Truncate(2),
            CompanyAdjustAmount = x.CompanyAdjustAmount.Truncate(2),
            BonusAdjustAmount = x.BonusAdjustAmount.Truncate(2),
            BonusAmount = x.BonusAmount.Truncate(2),
            BetAmount = x.BetAmount.Truncate(2),
            ValidBetAmount = x.ValidBetAmount.Truncate(2),
            WinLossAmount = x.WinLossAmount.Truncate(2),
            NetProfit = x.NetProfit.Truncate(2),
            AgentProfit = x.AgentProfit.Truncate(2)
        });

        var resource = new ResourceManager(typeof(Resources.Market));

        var xlsx = exportData?.ExportWithLocalization("markets", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}