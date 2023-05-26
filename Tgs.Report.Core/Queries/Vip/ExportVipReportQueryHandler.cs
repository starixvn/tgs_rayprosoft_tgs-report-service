using System.Resources;
using MediatR;
using Tgs.Report.Core.Resources;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.Core.Queries.Vip;

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
            DepositAmount = x.DepositAmount.Truncate(2),
            WithdrawalAmount = x.WithdrawalAmount.Truncate(2),
            BonusAmount = x.BonusAmount.Truncate(2),
            CompanyAdjustAmount = x.CompanyAdjustAmount.Truncate(2),
            BonusAdjustAmount = x.BonusAdjustAmount.Truncate(2),
            RebateAmount = x.RebateAmount.Truncate(2),
            ValidBetAmount = x.Turnover.Truncate(2),
            WinLossAmount = x.WinLossAmount.Truncate(2),
            NetProfit = x.NetProfit.Truncate(2)
        });

        var resource = new ResourceManager(typeof(VipReport));

        var xlsx = exportData.ExportWithLocalization("vipReport", resource);

        await using var stream = new MemoryStream();

        xlsx.SaveAs(stream);

        return new(stream.ToArray());
    }
}