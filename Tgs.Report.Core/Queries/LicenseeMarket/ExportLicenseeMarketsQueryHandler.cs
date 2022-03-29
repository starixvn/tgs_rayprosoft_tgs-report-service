using System.Resources;
using MediatR;
using Tgs.Report.ViewModel.LicenseeMarket;

namespace Tgs.Report.Core.Queries.LicenseeMarket
{
    internal class ExportLicenseeMarketsQueryHandler : IRequestHandler<ExportLicenseeMarketsRequest, MemoryStream>
    {
        private readonly IMediator _mediator;

        public ExportLicenseeMarketsQueryHandler(IMediator mediator) => _mediator = mediator;

        public async Task<MemoryStream> Handle(
            ExportLicenseeMarketsRequest request,
            CancellationToken cancellationToken)
        {
            var getMarketsRequest = new GetLicenseeMarketsRequest
            {
                LicenseeId = request.LicenseeId,
                CurrencyCode = request.CurrencyCode,
                From = request.From,
                To = request.To,
                ShowAll = true,
                OrderBy = request.OrderBy,
                SortBy = request.SortBy
            };

            var markets = await _mediator.Send(getMarketsRequest, cancellationToken).ConfigureAwait(false);

            var exportData = markets.Datas?.Items?.Select(x => new ExportLicenseeMarketsModel
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

            var resource = new ResourceManager(typeof(Resources.LicenseeMarket));

            var xlsx = exportData.ExportWithLocalization("licenseeMarkets", resource);

            await using var stream = new MemoryStream();

            xlsx.SaveAs(stream);

            return new MemoryStream(stream.ToArray());
        }
    }
}