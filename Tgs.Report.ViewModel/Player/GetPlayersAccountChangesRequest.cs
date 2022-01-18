using System;
using MediatR;
using Tgs.Report.Enum;

namespace Tgs.Report.ViewModel.Player
{
    public class GetPlayersAccountChangesRequest : PageableRequest, IRequest<PagedResponse<PlayersAccountChangeModel>>
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public TransactionType? TransactionType { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}