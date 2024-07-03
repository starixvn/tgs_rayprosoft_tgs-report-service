using System;
using MediatR;

namespace Tgs.Report.ViewModel.Player
{
    public record GetPlayerAllAgentRebatesRequest : PageableRequest, IStreamRequest<PlayerAllAgentRebateModel>
    {
        public Guid BrandId { get; set; }
        public string? Username { get; set; }
        public string? PlayerCode { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
