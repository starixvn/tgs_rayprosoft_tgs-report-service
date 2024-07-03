using System;
using MediatR;

namespace Tgs.Report.ViewModel.Player
{
    public record GetPlayerAllAgentRebateTotalAmountRequest : IRequest<PlayerAllAgentRebateTotalAmountModel>
    {
        public Guid BrandId { get; set; }
        public string? Username { get; set; }
        public string? PlayerCode { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
