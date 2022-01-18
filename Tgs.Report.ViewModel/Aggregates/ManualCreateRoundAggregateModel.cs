using MediatR;

namespace Tgs.Report.ViewModel.Aggregates
{
    public record ManualCreateRoundAggregateModel(string Date, int Hour) : IRequest<MessageResponse>;
}