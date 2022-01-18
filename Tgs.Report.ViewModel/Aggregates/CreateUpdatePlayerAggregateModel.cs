using MediatR;

namespace Tgs.Report.ViewModel.Aggregates
{
    public record CreateUpdatePlayerAggregateModel : IRequest<MessageResponse>;
}