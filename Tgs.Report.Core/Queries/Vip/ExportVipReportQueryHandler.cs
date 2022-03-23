using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.Core.Queries.Vip
{
    internal class ExportVipReportQueryHandler : IRequestHandler<ExportVipReportRequest, MemoryStream>
    {
        private readonly IMediator _mediator;

        public ExportVipReportQueryHandler(IMediator mediator) => _mediator = mediator;

        public async Task<MemoryStream> Handle(
            ExportVipReportRequest request,
            CancellationToken cancellationToken)
        {
            var getVipReportRequest = new GetVipReportRequest
            {
                BrandId = request.BrandId,
                Username = request.Username,
                Level = request.Level,
                From = request.From,
                To = request.To,
                ShowAll = true
            };

            var getVipReport =
                await _mediator.Send(getVipReportRequest, cancellationToken).ConfigureAwait(false);

            var exportData = getVipReport.Items?.Select(x => new ExportVipModel
            {
                Username = x.Username,
                Level = x.Level,
                DepositAmount = x.DepositAmount,
                WithdrawalAmount = x.WithdrawalAmount,
                BonusAmount = x.BonusAmount,
                CompanyAdjustAmount = x.CompanyAdjustAmount,
                RebateAmount = x.RebateAmount,
                ValidBetAmount = x.Turnover,
                WinLossAmount = x.WinLossAmount,
                NetProfit = x.NetProfit
            });

            var resource = new ResourceManager(typeof(VipReport));

            var xlsx = exportData.ExportWithLocalization("vipReport", resource);

            await using var stream = new MemoryStream();

            xlsx.SaveAs(stream);

            return new MemoryStream(stream.ToArray());
        }
    }
}