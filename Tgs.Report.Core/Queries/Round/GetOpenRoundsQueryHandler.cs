using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round
{
    public class GetOpenRoundsQueryHandler : IRequestHandler<GetOpenRoundsRequest, PagedResponse<OpenRoundModel>>
    {
        private readonly ITgsRepository _repository;

        public GetOpenRoundsQueryHandler(ITgsRepository repository)
        {
            _repository = repository;
        }

        public Task<PagedResponse<OpenRoundModel>> Handle(
            GetOpenRoundsRequest request,
            CancellationToken cancellationToken) =>
            _repository.Rounds.Where(x => x.Status == RoundStatus.Open)
                .Where(x => x.CreatedOn >= request.From && x.CreatedOn <= request.To)
                .Where(request.GameProviderId, x => request.GameProviderId == x.GameProviderId)
                .Where(request.ExternalRoundId, x => x.ExternalRoundId == request.ExternalRoundId)
                .GroupJoin(
                    _repository.Players,
                    round => round.PlayerCode,
                    player => player.PlayerCode,
                    (round, player) => new
                    {
                        round,
                        player
                    })
                .SelectMany(x => x.player.DefaultIfEmpty(),
                    (x, player) => new
                    {
                        x.round,
                        player
                    })
                .GroupJoin(_repository.Games,
                    combine => combine.round.GameId,
                    game => game.Id,
                    (combine, game) => new
                    {
                        combine.round,
                        combine.player,
                        game
                    })
                .SelectMany(x => x.game.DefaultIfEmpty(),
                    (x, game) => new
                    {
                        x.round,
                        x.player,
                        game
                    })
                .GroupJoin(_repository.RoundDetails,
                    c => c.round.Id,
                    detail => detail.RoundId,
                    (combine, detail) => new
                    {
                        combine.round,
                        combine.player,
                        combine.game,
                        detail
                    })
                .SelectMany(x => x.detail.DefaultIfEmpty(),
                    (x, detail) => new
                    {
                        x.round,
                        x.player,
                        x.game,
                        detail
                    })
                .GroupJoin(_repository.GameProviders,
                    combine => combine.round.GameProviderId,
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
                    })
                .Select(combine => new OpenRoundModel
                {
                    Id = combine.round.Id,
                    BrandCode = combine.player.Brand.Code,
                    BrandName = combine.player.Brand.Name,
                    PlayerType = (PlayerType)combine.player.Type,
                    Status = combine.round.ClientStatus,
                    PlayerCode = combine.player.PlayerCode,
                    ExternalPlayerCode = combine.player.PlayerCode,
                    PlayerName = combine.player.Username,
                    GameProviderId = combine.round.GameProviderId,
                    GameProviderCode = combine.game.GameProvider.Code,
                    GameProviderName = combine.gameProvider.Name,
                    SubGameProviderName = combine.game.SubGameProviderCodeNavigation.Name,
                    GameId = combine.game.Id,
                    GameName = combine.game.Name,
                    ExternalRoundId = combine.round.ExternalRoundId,
                    ExternalCreatedOn = combine.round.ExternalCreatedOn,
                    ExternalClosedOn = combine.round.ExternalClosedOn,
                    CreatedOn = combine.round.CreatedOn,
                    ClosedOn = combine.round.ClosedOn,
                    CurrencyCode = combine.round.CurrencyCode,
                    BetAmount = combine.round.BetAmount,
                    ValidBetAmount =
                        combine.round.Turnover * (combine.round.EffectiveBettingPercentage ??
                            (combine.brandGame == null ? 100M : combine.brandGame.EffectiveBettingPercentage)) / 100M,
                    SettleAmount = combine.round.SettleAmount,
                    WinLoss = -combine.round.WinLoss,
                    AdditionalData = combine.detail.AdditionalData
                })
                .OrderByDescending(x => x.CreatedOn)
                .CreatePagedResponseAsync(request);
    }
}