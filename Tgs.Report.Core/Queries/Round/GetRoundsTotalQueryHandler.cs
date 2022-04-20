using MediatR;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round
{
    public class GetRoundsTotalQueryHandler : IRequestHandler<GetRoundsTotalRequest, RoundTotalModel>
    {
        private readonly ITgsRepository _repository;

        public GetRoundsTotalQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public Task<RoundTotalModel> Handle(
            GetRoundsTotalRequest request,
            CancellationToken cancellationToken)
        {
            var result = _repository.Rounds.AsQueryable()
                .Where(x => x.BrandId == request.BrandId)
                .Where(request.From.HasValue && request.To.HasValue,
                    x => x.CreatedOn >= request.From!.Value.ToUniversalTime() &&
                        x.CreatedOn <= request.To!.Value.ToUniversalTime())
                .Where(request.ClosedFrom.HasValue && request.ClosedTo.HasValue,
                    x => x.ClosedOn >= request.ClosedFrom!.Value.ToUniversalTime() &&
                        x.ClosedOn <= request.ClosedTo!.Value.ToUniversalTime())
                .Where(request.GameProviderId, x => request.GameProviderId == x.GameProviderId)
                .Where(request.Status, x => (short?)request.Status == x.ClientStatus)
                .Where(request.RoundId, x => x.Id == request.RoundId)
                .Where(request.PlayerCode, x => x.PlayerCode == request.PlayerCode)
                .Where(request.ExternalRoundId, x => x.ExternalRoundId == request.ExternalRoundId)
                .GroupJoin(
                    _repository.Players,
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
                .GroupJoin(_repository.Games,
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
                .GroupJoin(_repository.RoundDetails, c => c.round.Id, detail => detail.RoundId,
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
                .GroupJoin(_repository.GameProviders, combine => combine.round.GameProviderId,
                    gameProvider => gameProvider.Id,
                    (combine, gameProvider) => new
                    {
                        combine.round,
                        combine.player,
                        combine.game,
                        combine.detail,
                        gameProvider
                    })
                .SelectMany(x => x.gameProvider.DefaultIfEmpty(),
                    (x, gameProvider) => new
                    {
                        x.round,
                        x.player,
                        x.game,
                        x.detail,
                        gameProvider
                    })
                .GroupJoin(_repository.BrandGames,
                    roundGroup => new
                    {
                        roundGroup.round.BrandId,
                        roundGroup.round.GameId
                    },
                    brandGame => new
                    {
                        brandGame.BrandId,
                        brandGame.GameId
                    },
                    (roundGroup, brandGame) => new
                    {
                        roundGroup.round,
                        roundGroup.player,
                        roundGroup.game,
                        roundGroup.gameProvider,
                        roundGroup.detail,
                        brandGame
                    })
                .SelectMany(x => x.brandGame.DefaultIfEmpty(),
                    (x, brandGame) => new
                    {
                        x.round,
                        x.player,
                        x.game,
                        x.gameProvider,
                        x.detail,
                        brandGame
                    });

            return Task.FromResult(new RoundTotalModel
            {
                BetAmount = result.Sum(r => r.round.BetAmount),
                ValidBetAmount = result.Sum(r => r.round.Turnover),
                WinLoss = result.Sum(r => -r.round.WinLoss)
            });
        }
    }
}