using System;
using MediatR;

namespace Tgs.Report.ViewModel.Vip
{
    public record GetVipReportTotalAmountRequest : IRequest<VipTotalAmountModel>
    {
        public Guid BrandId { get; init; }
        public string? Username { get; init; }
        public int? Level { get; init; }
        public DateTimeOffset From { get; init; }
        public DateTimeOffset To { get; init; }
    }
}