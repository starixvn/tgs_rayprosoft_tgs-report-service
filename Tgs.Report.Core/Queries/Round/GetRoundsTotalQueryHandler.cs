using MediatR;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round;

public class GetRoundsTotalQueryHandler : IRequestHandler<GetRoundsTotalRequest, RoundTotalModel>
{
    private readonly TgsContext _context;

    public GetRoundsTotalQueryHandler(TgsContext context)
        => _context = context ?? throw new ArgumentNullException(nameof(context));

    public Task<RoundTotalModel> Handle(
        GetRoundsTotalRequest request,
        CancellationToken cancellationToken)
    {
        var result = _context.Rounds
            .Where(x => x.BrandId == request.BrandId)
            .Where(request.From.HasValue && request.To.HasValue,
                x => x.CreatedOn >= request.From!.Value.ToUniversalTime() &&
                    x.CreatedOn < request.To!.Value.ToUniversalTime())
            .Where(request.ClosedFrom.HasValue && request.ClosedTo.HasValue,
                x => x.ClosedOn >= request.ClosedFrom!.Value.ToUniversalTime() &&
                    x.ClosedOn < request.ClosedTo!.Value.ToUniversalTime())
            .Where(request.GameProviderId, x => request.GameProviderId == x.GameProviderId)
            .Where(request.Status, x => (short?)request.Status == x.ClientStatus)
            .Where(request.RoundId, x => x.Id == request.RoundId)
            .Where(request.PlayerCode, x => x.PlayerCode == request.PlayerCode)
            .Where(request.ExternalRoundId, x => x.ExternalRoundId == request.ExternalRoundId)
            .GroupJoin(
                _context.Players,
                round => round.PlayerCode, player => player.PlayerCode,
                (round, player) => new
                {
                    round,
                    player
                })
            .SelectMany(x => x.player.DefaultIfEmpty(), (x, player) => new
            {
                x.round,
                player
            })
            .GroupJoin(_context.Games,
                combine => combine.round.GameId, game => game.Id,
                (combine, game) => new
                {
                    combine.round,
                    combine.player,
                    game
                })
            .SelectMany(x => x.game.DefaultIfEmpty(), (x, game) => new
            {
                x.round,
                x.player,
                game
            })
            .GroupJoin(_context.RoundDetails, c => c.round.Id, detail => detail.RoundId,
                (combine, detail) => new
                {
                    combine.round,
                    combine.player,
                    combine.game,
                    detail
                })
            .SelectMany(x => x.detail.DefaultIfEmpty(), (x, detail) => new
            {
                x.round,
                x.player,
                x.game,
                detail
            })
            .Where(request.Username, x => request.Username == x.player!.Username)
            .Where(request.GameType, x => x.game!.Type == (short?)request.GameType)
            .GroupBy(x => true)
            .Select(g => new RoundTotalModel
            {
                BetAmount = g.Sum(x => x.round.BetAmount),
                ValidBetAmount = g.Sum(r => r.round.Turnover),
                WinLoss = g.Sum(r => -r.round.WinLoss),
                SettleAmount = g.Sum(r => r.round.SettleAmount),
            });

        return Task.FromResult(result.FirstOrDefault() ?? new RoundTotalModel());
    }
}