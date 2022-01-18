using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    internal class GetAgentPlayerWinLossQueryHandler : IRequestHandler<GetAgentPlayerWinLossRequest,
        PagedResponse<AgentPlayerWinLossModel>>
    {
        private readonly ITgsRepository _repository;

        public GetAgentPlayerWinLossQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public async Task<PagedResponse<AgentPlayerWinLossModel>> Handle(
            GetAgentPlayerWinLossRequest request,
            CancellationToken cancellationToken)
        {
            var agent = await _repository.Agents
                .FirstOrDefaultAsync(x => x.Id == request.AgentId, cancellationToken);

            if (agent == null)
            {
                return new PagedResponse<AgentPlayerWinLossModel> { Valid = false, Message = Resources.Agent.NotFound };
            }

            var agentDividends = await _repository.AgentDividends
                .Where(x => x.AgentId == request.AgentId)
                .Where(x => x.CreatedOn >= request.From)
                .Where(x => x.CreatedOn < request.To).ToArrayAsync(cancellationToken);

            if (!agentDividends.Any())
            {
                return new PagedResponse<AgentPlayerWinLossModel>
                    { Valid = true, Items = Array.Empty<AgentPlayerWinLossModel>() };
            }

            var from = agentDividends.Min(x => x.From);
            var to = agentDividends.Max(x => x.To);

            var playerAggregate = await _repository.PlayerGameRebateAggregates
                .Where(x => x.AgentId == request.AgentId)
                .Where(x => x.ReportDate >= from && x.ReportDate <= to)
                .GroupBy(g => g.PlayerCode,
                    (playerCode, aggregate) => new
                    {
                        playerCode,
                        LiveDealerWinLossAmount =
                            aggregate.Where(x => x.GameType == GameType.LiveDealer).Sum(x => x.WinLoss),
                        SlotWinLossAmount =
                            aggregate.Where(x =>
                                x.GameType == GameType.Fishing || x.GameType == GameType.Arcade ||
                                x.GameType == GameType.Slot || x.GameType == GameType.TableGame).Sum(x => x.WinLoss),
                        LotteryWinLossAmount = aggregate.Where(x => x.GameType == GameType.Lottery).Sum(x => x.WinLoss),
                        CardGameWinLossAmount =
                            aggregate.Where(x => x.GameType == GameType.CardGame).Sum(x => x.WinLoss),
                        SportsWinLossAmount =
                            aggregate.Where(x => x.GameType == GameType.Sportsbook || x.GameType == GameType.ESports)
                                .Sum(x => x.WinLoss),
                        VirtualSportsWinLossAmount =
                            aggregate.Where(x => x.GameType == GameType.VirtualSports).Sum(x => x.WinLoss)
                    }).ToArrayAsync(cancellationToken);

            var players = await _repository.Players
                .Where(x => playerAggregate.Select(p => p.playerCode).Contains(x.PlayerCode))
                .ToArrayAsync(cancellationToken);
            return await players.Join(
                playerAggregate,
                p => p.PlayerCode,
                winLossPlayer => winLossPlayer.playerCode,
                (p, winLossPlayer) => new AgentPlayerWinLossModel
                {
                    Username = p.Username,
                    LiveDealerWinLossAmount = winLossPlayer.LiveDealerWinLossAmount,
                    SlotWinLossAmount = winLossPlayer.SlotWinLossAmount,
                    LotteryWinLossAmount = winLossPlayer.LotteryWinLossAmount,
                    CardGameWinLossAmount = winLossPlayer.CardGameWinLossAmount,
                    SportsWinLossAmount = winLossPlayer.SportsWinLossAmount,
                    VirtualSportsWinLossAmount = winLossPlayer.VirtualSportsWinLossAmount
                }).CreatePagedResponseAsync(players.Length);
        }
    }
}