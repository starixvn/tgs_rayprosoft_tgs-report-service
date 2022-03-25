using System.Resources;
using MediatR;
using Tgs.Report.ViewModel.ResellerMarket;

namespace Tgs.Report.Core.Queries.ResellerMarket
{
    internal class ExportResellerMarketsQueryHandler : IRequestHandler<ExportResellerMarketsRequest, MemoryStream>
    {
        private readonly IMediator _mediator;

        public ExportResellerMarketsQueryHandler(IMediator mediator) => _mediator = mediator;

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

            var markets = await _mediator.Send(getMarketsRequest, cancellationToken).ConfigureAwait(false);

            var exportData = markets.Datas?.Items?.Select(x => new ExportResellerMarketsModel
            {
                BrandCode = x.BrandCode,
                CurrencyCode = x.CurrencyCode,
                DepositAmount = x.DepositAmount,
                WithdrawalAmount = x.WithdrawalAmount,
                CompanyAdjustAmount = x.CompanyAdjustAmount,
                BonusAmount = x.BonusAmount,
                BetAmount = x.BetAmount,
                Turnover = x.Turnover,
                WinLossAmount = x.WinLossAmount,
                NetProfit = x.NetProfit
            });

            var resource = new ResourceManager(typeof(Resources.ResellerMarket));

            var xlsx = exportData.ExportWithLocalization("resellerMarkets", resource);

            await using var stream = new MemoryStream();

            xlsx.SaveAs(stream);

            return new MemoryStream(stream.ToArray());
        }
    }
}