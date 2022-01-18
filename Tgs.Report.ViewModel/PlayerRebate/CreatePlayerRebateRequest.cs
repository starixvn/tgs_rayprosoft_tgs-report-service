using MediatR;

namespace Tgs.Report.ViewModel.PlayerRebate
{
    public class CreatePlayerRebateRequest : IRequest<MessageResponse>
    {
        public string Date { get; set; }
    }
}