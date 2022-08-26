using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.Core.Queries.Agent
{
    internal class GetAgentPlayerWinLossQueryHandler : IRequestHandler<GetAgentPlayerWinLossRequest,
        PagedResponse<AgentPlayerWinLossModel>>
    {
        private readonly TgsContext _context;

        public GetAgentPlayerWinLossQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<PagedResponse<AgentPlayerWinLossModel>> Handle(
            GetAgentPlayerWinLossRequest request,
            CancellationToken cancellationToken)
        {
            var agent = await _context.Agents
                .Select(x => new
                {
                    x.Id
                })
                .FirstOrDefaultAsync(x => x.Id == request.AgentId, cancellationToken);

            if (agent == null)
            {
                return new PagedResponse<AgentPlayerWinLossModel>
                {
                    Valid = false,
                    Message = Resources.Agent.NotFound
                };
            }

            var agentDividends = await _context.AgentDividends
                .Where(x => x.AgentId == request.AgentId)
                .Where(x => x.From >= request.From.ToUniversalTime())
                .Where(x => x.To < request.To.ToUniversalTime())
                .Select(x => new
                {
                    x.From,
                    x.To
                })
                .ToArrayAsync(cancellationToken);

            if (!agentDividends.Any())
            {
                return new PagedResponse<AgentPlayerWinLossModel>
                {
                    Valid = true,
                    Items = Array.Empty<AgentPlayerWinLossModel>()
                };
            }

            var from = agentDividends.Min(x => x.From);
            var to = agentDividends.Max(x => x.To);

            var playerAggregate = await _context.PlayerGameRebateAggregates
                .Where(x => x.AgentId == request.AgentId)
                .Where(x => x.ReportDate >= from && x.ReportDate < to)
                .GroupBy(g => g.PlayerCode,
                    (playerCode, aggregate) => new
                    {
                        playerCode,
                        LiveDealerWinLossAmount =
                            aggregate.Where(x => x.GameType == (short)GameType.LiveDealer).Sum(x => x.WinLoss),
                        SlotWinLossAmount =
                            aggregate.Where(x =>
                                    x.GameType == (short)GameType.Fishing || x.GameType == (short)GameType.Arcade ||
                                    x.GameType == (short)GameType.Slot || x.GameType == (short)GameType.TableGame)
                                .Sum(x => x.WinLoss),
                        LotteryWinLossAmount = aggregate.Where(x => x.GameType == (short)GameType.Lottery)
                            .Sum(x => x.WinLoss),
                        CardGameWinLossAmount =
                            aggregate.Where(x => x.GameType == (short)GameType.CardGame).Sum(x => x.WinLoss),
                        SportsWinLossAmount =
                            aggregate.Where(x
                                    => x.GameType == (short)GameType.Sportsbook ||
                                    x.GameType == (short)GameType.ESports)
                                .Sum(x => x.WinLoss),
                        VirtualSportsWinLossAmount =
                            aggregate.Where(x => x.GameType == (short)GameType.VirtualSports).Sum(x => x.WinLoss)
                    }).ToArrayAsync(cancellationToken);

            var players = await _context.Players
                .Where(x => playerAggregate.Select(p => p.playerCode).Contains(x.PlayerCode))
                .Select(x => new
                {
                    x.PlayerCode,
                    x.Username
                })
                .ToArrayAsync(cancellationToken);

            return players.Join(
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
                }).AsQueryable().CreatePagedResponse(request);
        }
    }
}