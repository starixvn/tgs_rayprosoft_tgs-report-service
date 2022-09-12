using MediatR;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round
{
    public class GetSystemRoundsTotalQueryHandler : IRequestHandler<GetSystemRoundsTotalRequest, SystemRoundTotalModel>
    {
        private readonly TgsContext _context;

        public GetSystemRoundsTotalQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<SystemRoundTotalModel> Handle(
            GetSystemRoundsTotalRequest request,
            CancellationToken cancellationToken)
        {
            var result = _context.Rounds.AsQueryable()
                .Where(x => request.GameProviderId == x.GameProviderId)
                .Where(request.PlayerCode, x => x.PlayerCode == request.PlayerCode)
                .Where(request.ExternalRoundId, x => x.ExternalRoundId == request.ExternalRoundId)
                .Where(request.RoundId, x => x.Id == request.RoundId)
                .Where(request.ExternalFrom.HasValue && request.ExternalTo.HasValue,
                    x => x.ExternalCreatedOn >= request.ExternalFrom!.Value.ToUniversalTime() &&
                        x.ExternalCreatedOn < request.ExternalTo!.Value.ToUniversalTime())
                .Where(request.ExternalClosedFrom.HasValue && request.ExternalClosedTo.HasValue,
                    x => x.ExternalClosedOn >= request.ExternalClosedFrom!.Value.ToUniversalTime() &&
                        x.ExternalClosedOn < request.ExternalClosedTo!.Value.ToUniversalTime())
                .Where(request.From.HasValue && request.To.HasValue,
                    x => x.CreatedOn >= request.From!.Value.ToUniversalTime() &&
                        x.CreatedOn < request.To!.Value.ToUniversalTime())
                .Where(request.ClosedFrom.HasValue && request.ClosedTo.HasValue,
                    x => x.ClosedOn >= request.ClosedFrom!.Value.ToUniversalTime() &&
                        x.ClosedOn < request.ClosedTo!.Value.ToUniversalTime())
                .Where(request.Status, x => (short?)request.Status == x.ClientStatus)
                .Where(request.BrandId, x => x.BrandId == request.BrandId)
                .Where(request.CurrencyCode, x => x.CurrencyCode == request.CurrencyCode)
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
                .Where(request.SubGameProviderCode, x => x.game!.SubGameProviderCode == request.SubGameProviderCode)
                .Where(request.GameType, x => x.game!.Type == (short?)request.GameType)
                .GroupBy(x => true)
                .Select(g => new SystemRoundTotalModel
                {
                    BetAmount = g.Sum(r => r.round.BetAmount),
                    ValidBetAmount = g.Sum(r => r.round.ValidBetAmount),
                    Turnover = g.Sum(r => r.round.Turnover),
                    SettleAmount = g.Sum(r => r.round.SettleAmount),
                    WinLoss = g.Sum(r => -r.round.WinLoss)
                });

            return Task.FromResult(result.FirstOrDefault() ?? new SystemRoundTotalModel());
        }
    }
}