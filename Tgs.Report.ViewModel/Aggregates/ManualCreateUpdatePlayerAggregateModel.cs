using MediatR;

namespace Tgs.Report.ViewModel.Aggregates
{
    public record ManualCreateUpdatePlayerAggregateModel(string Date, int Hour) : IRequest<MessageResponse>;
}