using MediatR;

namespace Tgs.Report.ViewModel.Aggregates
{
    public record CreateRoundAggregateRequest : IRequest<MessageResponse>;
}