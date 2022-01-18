using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public class CreateAgentDividendsRequest : IRequest<MessageResponse>
    {
        public string BrandCode { get; set; }
    }
}