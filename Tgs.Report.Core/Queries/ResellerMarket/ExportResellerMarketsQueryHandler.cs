using System.Resources;
using MediatR;
using Tgs.Report.ViewModel.ResellerMarket;

namespace Tgs.Report.Core.Queries.ResellerMarket;

internal class ExportResellerMarketsQueryHandler(IMediator mediator)
    : IRequestHandler<ExportResellerMarketsRequest, MemoryStream>
{
    public async Task<MemoryStream> Handle(
        ExportResellerMarketsRequest request,
        CancellationToken cancellationToken)
    {
        var getMarketsRequest = new GetResellerMarketsRequest
        {
            ResellerId = request.ResellerId,
            CurrencyCode = request.CurrencyCode,
            From = request.From,
            To = request.To,
            ShowAll = true,
            OrderBy = request.OrderBy,
            SortBy = request.SortBy
        };

        var markets = await mediator.Send(getMarketsRequest, cancellationToken).ConfigureAwait(false);

        var exportData = markets.Datas?.Items?.Select(x => new ExportResellerMarketsModel
        {
            BrandCode = x.BrandCode,
            CurrencyCode = x.CurrencyCode,
            DepositAmount = (x.DepositAmount - x.DepositHandlingFee).Truncate(2),
            WithdrawalAmount = x.WithdrawalAmount.Truncate(2),
            CompanyAdjustAmount = x.CompanyAdjustAmount.Truncate(2),
            BonusAmount = x.BonusAmount.Truncate(2),
            BetAmount = x.BetAmount.Truncate(2),
            Turnover = x.Turnover.Truncate(2),
            WinLossAmount = x.WinLossAmount.Truncate(2),
            NetProfit = x.NetProfit.Truncate(2),
            AgentProfit = x.AgentProfit.Truncate(2)
        });

        var resource = new ResourceManager(typeof(Resources.ResellerMarket));

        var xlsx = exportData.ExportWithLocalization("resellerMarkets", resource);

        await using var stream = new MemoryStream();

        xlsx.SaveAs(stream);

        return new(stream.ToArray());
    }
}