using MediatR;

namespace Tgs.Report.ViewModel.Agent
{
    public record CreateAgentDividendsRequest : IRequest<MessageResponse>
    {
        public string BrandCode { get; init; } = default!;
    }
}