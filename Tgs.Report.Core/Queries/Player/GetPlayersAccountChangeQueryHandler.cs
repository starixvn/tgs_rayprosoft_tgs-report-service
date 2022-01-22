using System.Text.Json;
using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player
{
    public class
        GetPlayersAccountChangeQueryHandler : IRequestHandler<GetPlayersAccountChangesRequest,
            PagedResponse<PlayersAccountChangeModel>>
    {
        private readonly ITgsRepository _repository;

        public GetPlayersAccountChangeQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<PlayersAccountChangeModel>> Handle(
            GetPlayersAccountChangesRequest request,
            CancellationToken cancellationToken)
        {
            var playersAccountChanges = await _repository.Transactions
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.RecordedOn >= request.From && x.RecordedOn < request.To)
                .Where(request.TransactionType is not null, x => x.Type == (short)request.TransactionType!)
                .Where(request.TransactionType is null,
                    x => x.Type == (short)TransactionType.Adjust || x.Type == (short)TransactionType.Rebate ||
                        x.Type == (short)TransactionType.Promotion ||
                        x.Type == (short)TransactionType.UnsettledBetManual)
                .Join(_repository.Players,
                    transaction => transaction.PlayerCode,
                    player => player.PlayerCode,
                    (transaction, player) => new
                    {
                        Transaction = transaction,
                        Player = player
                    })
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Player.Username == request.Username)
                .GroupJoin(_repository.Users,
                    group => group.Transaction.CreatedByUserId,
                    user => user.Id,
                    (group, user) => new
                    {
                        Group = group,
                        User = user
                    })
                .SelectMany(x => x.User.DefaultIfEmpty(),
                    (x, user) => new
                    {
                        x.Group.Transaction,
                        x.Group.Player,
                        User = user
                    })
                .Select(x => new PlayersAccountChangeModel
                {
                    Username = x.Player.Username,
                    Amount = x.Transaction.Amount,
                    BeforeBalance = x.Transaction.Balance - x.Transaction.Amount,
                    AfterBalance = x.Transaction.Balance,
                    TransactionType = (TransactionType)x.Transaction.Type,
                    RecordedOn = x.Transaction.RecordedOn,
                    Operator = x.User.Username ?? "System",
                    Description = x.Transaction.Description
                }).CreatePagedResponseAsync(request);

            foreach (var item in playersAccountChanges.Items)
            {
                if (item.TransactionType != TransactionType.Adjust)
                {
                    continue;
                }

                var adjustRemark =
                    JsonSerializer.Deserialize<AdjustRemarkModel>(item.Description);
                item.Description = adjustRemark.Remark;
            }

            return playersAccountChanges;
        }
    }
}