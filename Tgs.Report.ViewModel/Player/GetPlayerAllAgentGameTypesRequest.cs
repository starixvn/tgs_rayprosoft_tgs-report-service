using System;
using MediatR;

namespace Tgs.Report.ViewModel.Player
{
    public record GetPlayerAllAgentGameTypesRequest : PageableRequest, IStreamRequest<PlayerAllAgentGameTypeModel>
    {
        public Guid BrandId { get; set; }
        public string PlayerCode { get; set; } = string.Empty;
        public DateTime SettleDateTime { get; set; }
    }
}
