using MediatR;

namespace Tgs.Report.ViewModel.PlayerRebate
{
    public record CreatePlayerRebateRequest : IRequest<MessageResponse>
    {
        public string Date { get; init; } = default!;
    }
}