using System;
using MediatR;

namespace Tgs.Report.ViewModel.Market;

public record GetAdditionalExpensesRequest : IRequest<GetAdditionalExpensesResponse>
{
    public Guid BrandId { get; set; }

    public DateTimeOffset ReportDate { get; set; }
}