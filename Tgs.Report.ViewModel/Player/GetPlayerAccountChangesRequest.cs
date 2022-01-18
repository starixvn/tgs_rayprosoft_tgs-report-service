using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player
{
    public class GetPlayerAccountChangesRequest : PageableRequest, IRequest<PagedResponse<PlayerAccountChangeModel>>
    {
        public Guid BrandId { get; set; }
        public string PlayerCode { get; set; }
        public AccountChangeType? AccountChangeType { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}