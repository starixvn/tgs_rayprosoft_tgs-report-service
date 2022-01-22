using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.GameAction;

namespace Tgs.Report.Core.Queries.GameAction
{
    public class GetGameActionsQueryHandler : IRequestHandler<GetGameActionsRequest, PagedResponse<GameActionModel>>
    {
        private readonly ITgsRepository _repository;

        public GetGameActionsQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public Task<PagedResponse<GameActionModel>> Handle(
            GetGameActionsRequest request,
            CancellationToken cancellationToken)
            => _repository.GameActions.Where(x => x.RoundId == request.RoundId)
                .Select(x => new GameActionModel
                {
                    RecordedOn = x.RecordedOn,
                    ExternalTransactionId = x.ExternalTransactionId,
                    GameActionType = (GameActionType)x.Type,
                    Amount = x.Amount,
                    BeforeBalance = x.Balance - x.Amount,
                    AfterBalance = x.Balance
                })
                .OrderByDescending(x => x.RecordedOn)
                .CreatePagedResponseAsync(request);
    }
}