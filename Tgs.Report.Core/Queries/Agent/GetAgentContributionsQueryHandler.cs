using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    public class
        GetAgentContributionsQueryHandler : IRequestHandler<GetAgentContributionsRequest,
            PagedResponse<AgentContributionModel>>
    {
        private readonly ITgsRepository _repository;

        public GetAgentContributionsQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<AgentContributionModel>> Handle(
            GetAgentContributionsRequest request,
            CancellationToken cancellationToken)
        {
            var agent = await _repository.Agents.Where(x => x.BrandId == request.BrandId)
                .Where(request.AgentId, a => a.Id == request.AgentId)
                .Where(request.Username, a => a.Username == request.Username).FirstOrDefaultAsync(cancellationToken);

            if (agent == null)
            {
                return new PagedResponse<AgentContributionModel> { Valid = false, Message = Resources.Agent.NotFound };
            }

            var playerWithdrawals = await _repository.WithdrawForms
                .Where(x => x.BrandId == request.BrandId)
                .Where(x => x.UserType == 1 && x.Status == 9000)
                .Where(x => x.CreatedOn >= request.From.ToUniversalTime() && x.CreatedOn < request.To.ToUniversalTime())
                .Join(_repository.Players,
                    withdrawForm => withdrawForm.UserId,
                    player => player.PlayerCode,
                    (withdrawForm, player) => new
                    {
                        player.AgentId,
                        player.PlayerCode,
                        WithdrawalAmount = withdrawForm.ActualAmount ?? withdrawForm.Amount
                    })
                .GroupBy(x => new
                    {
                        x.AgentId,
                        x.PlayerCode
                    },
                    (x, withdrawals) => new
                    {
                        x.AgentId,
                        x.PlayerCode,
                        WithdrawalAmount = withdrawals.Sum(w => w.WithdrawalAmount)
                    })
                .ToArrayAsync(cancellationToken)
                .ConfigureAwait(false);

            var childAgents = await _repository.Agents.Where(x => x.ParentAgentId == agent.Id)
                .Include(x => x.Players)
                .ToArrayAsync(cancellationToken);
            var playerCodes = childAgents.SelectMany(c => c.Players.Select(p => p.PlayerCode));

            var players = agent.Players.GroupJoin(_repository.PlayerAggregates
                        .Where(x => x.BrandId == request.BrandId)
                        .Where(x => x.AgentId == agent.Id)
                        .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                            x.ReportDate < request.To.ToUniversalTime()),
                    p => p.PlayerCode,
                    playerAggregate => playerAggregate.PlayerCode,
                    (player, players) => new
                    {
                        player,
                        players
                    })
                .SelectMany(x => x.players.DefaultIfEmpty(),
                    (x, playerAggregate) => new
                    {
                        x.player,
                        playerAggregate
                    })
                .GroupBy(x => new
                    {
                        x.player.Username,
                        x.player.PlayerCode
                    },
                    (player, playerAggregate) =>
                        new AgentContributionModel
                        {
                            Username = player.Username,
                            Type = AccountType.Player,
                            DepositAmount = playerAggregate.Sum(p => p.playerAggregate?.DepositAmount ?? 0),
                            WithdrawalAmount =
                                playerWithdrawals.FirstOrDefault(w => w.PlayerCode == player.PlayerCode)
                                    ?.WithdrawalAmount ?? 0,
                            AdjustmentAmount = playerAggregate.Sum(p => p.playerAggregate?.AdjustmentAmount ?? 0),
                            BonusAmount =
                                playerAggregate.Sum(p
                                    => p.playerAggregate == null
                                        ? 0
                                        : p.playerAggregate.RebateAmount + p.playerAggregate.PromotionAmount),
                            BetAmount = playerAggregate.Sum(p => p.playerAggregate?.BetAmount ?? 0),
                            ValidBetAmount = playerAggregate.Sum(p => p.playerAggregate?.Turnover ?? 0),
                            Turnover = playerAggregate.Sum(p => p.playerAggregate?.Turnover ?? 0),
                            WinLossAmount = playerAggregate.Sum(p => p.playerAggregate?.WinLossAmount ?? 0),
                            NetProfit = playerAggregate.Sum(p => p.playerAggregate == null
                                ? 0
                                : p.playerAggregate.WinLossAmount - p.playerAggregate.AdjustmentAmount -
                                p.playerAggregate.RebateAmount -
                                p.playerAggregate.PromotionAmount)
                        }).ToArray();

            if (!request.IncludeAgents)
            {
                return await players
                    .CreatePagedResponseAsync(players.Length);
            }

            var childAgentPlayers = childAgents.GroupJoin(_repository.PlayerAggregates
                        .Where(x => x.BrandId == request.BrandId)
                        .Where(x => playerCodes
                            .Contains(x.PlayerCode))
                        .Where(x => x.AgentId != null)
                        .Where(x => x.ReportDate >= request.From.ToUniversalTime() &&
                            x.ReportDate < request.To.ToUniversalTime()),
                    a => a.Id,
                    playerAggregate => playerAggregate.AgentId,
                    (agent, players) => new
                    {
                        agent,
                        players
                    })
                .SelectMany(x => x.players.DefaultIfEmpty(),
                    (x, playerAggregate) => new
                    {
                        x.agent,
                        playerAggregate
                    })
                .GroupBy(x => new
                    {
                        x.agent.Username,
                        x.agent.Id
                    },
                    (agent, playerAggregate) =>
                        new AgentContributionModel
                        {
                            Username = agent.Username,
                            Type = AccountType.Agent,
                            DepositAmount = playerAggregate.Sum(p => p.playerAggregate?.DepositAmount ?? 0),
                            WithdrawalAmount =
                                playerWithdrawals.Where(w => w.AgentId == agent.Id).Sum(w => w.WithdrawalAmount),
                            AdjustmentAmount = playerAggregate.Sum(p => p.playerAggregate?.AdjustmentAmount ?? 0),
                            BonusAmount =
                                playerAggregate.Sum(p
                                    => p.playerAggregate == null
                                        ? 0
                                        : p.playerAggregate.RebateAmount + p.playerAggregate.PromotionAmount),
                            BetAmount = playerAggregate.Sum(p => p.playerAggregate?.BetAmount ?? 0),
                            ValidBetAmount = playerAggregate.Sum(p => p.playerAggregate?.OriginalTurnover ?? 0),
                            Turnover = playerAggregate.Sum(p => p.playerAggregate?.Turnover ?? 0),
                            WinLossAmount = playerAggregate.Sum(p => p.playerAggregate?.WinLossAmount ?? 0),
                            NetProfit = playerAggregate.Sum(p => p.playerAggregate == null
                                ? 0
                                : p.playerAggregate.WinLossAmount - p.playerAggregate.AdjustmentAmount -
                                p.playerAggregate.RebateAmount -
                                p.playerAggregate.PromotionAmount)
                        }).ToArray();


            return childAgentPlayers.Union(players).AsQueryable()
                .CreatePagedResponse(request);
        }
    }
}