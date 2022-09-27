using System.Text;
using System.Text.Json;
using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player
{
    public class
        GetPlayersAccountChangeQueryHandler : IRequestHandler<GetPlayersAccountChangesRequest,
            PagedResponse<PlayersAccountChangeModel>>
    {
        private readonly TgsContext _context;

        public GetPlayersAccountChangeQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedResponse<PlayersAccountChangeModel>> Handle(
            GetPlayersAccountChangesRequest request,
            CancellationToken cancellationToken)
        {
            var playersAccountChanges = await _context.Transactions
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.RecordedOn >= request.From.ToUniversalTime() &&
                    x.RecordedOn < request.To.ToUniversalTime())
                .Where(request.TransactionType is not null, x => x.Type == (short?)request.TransactionType)
                .Where(request.TransactionType is null,
                    x => x.Type == (short)TransactionType.CompanyAdjust ||
                        x.Type > 5000 &&
                        x.Type < 6000 ||
                        x.Type == (short)TransactionType.Rebate ||
                        x.Type == (short)TransactionType.Promotion ||
                        x.Type == (short)TransactionType.UnsettledBetManual)
                .Join(_context.Players,
                    transaction => transaction.PlayerCode,
                    player => player.PlayerCode,
                    (transaction, player) => new
                    {
                        Transaction = transaction,
                        Player = player
                    })
                .Where(!string.IsNullOrEmpty(request.Username), x => x.Player.Username == request.Username)
                .GroupJoin(_context.Users,
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
                    Operator = x.User == null ? "System" : x.User.Username,
                    Description = x.Transaction.Description
                }).CreatePagedResponseAsync(request);

            if (playersAccountChanges.Items == null)
            {
                return playersAccountChanges;
            }

            foreach (var item in playersAccountChanges.Items)
            {
                if (item.TransactionType is not (TransactionType.CompanyAdjust or TransactionType.AgentAdjust
                    or TransactionType.SystemAdjust or TransactionType.TransferAdjust or TransactionType.BonusAdjust))
                {
                    continue;
                }

                if (item.Description is null)
                {
                    item.Description = "";
                }
                else
                {
                    try
                    {
                        item.Description = JsonSerializer.Deserialize<AdjustRemarkModel>(item.Description)?.Remark;
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
                
            }

            return playersAccountChanges;
        }
    }
}