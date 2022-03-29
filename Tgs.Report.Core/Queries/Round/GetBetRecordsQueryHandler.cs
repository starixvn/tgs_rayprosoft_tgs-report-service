using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round
{
    public class GetBetRecordsQueryHandler : IRequestHandler<GetBetRecordsRequest, GetBetRecordsResponse>
    {
        private readonly ITgsService<BrandGame> _brandGameService;
        private readonly ITgsService<Repository.Configurations.GameProvider> _gameProviderService;
        private readonly ITgsService<Game> _gameService;
        private readonly ITgsService<Repository.Configurations.Player> _playerService;
        private readonly ITgsService<RoundDetail> _roundDetailService;
        private readonly ITgsService<Repository.Configurations.Round> _roundService;

        public GetBetRecordsQueryHandler(
            ITgsService<Repository.Configurations.Round> roundService,
            ITgsService<Repository.Configurations.Player> playerService,
            ITgsService<Game> gameService,
            ITgsService<RoundDetail> roundDetailService,
            ITgsService<Repository.Configurations.GameProvider> gameProviderService,
            ITgsService<BrandGame> brandGameService)
        {
            _roundService = roundService;
            _playerService = playerService;
            _gameService = gameService;
            _roundDetailService = roundDetailService;
            _gameProviderService = gameProviderService;
            _brandGameService = brandGameService;
        }

        public async Task<GetBetRecordsResponse> Handle(
            GetBetRecordsRequest request,
            CancellationToken cancellationToken)
        {
            var query = _roundService.GetQueries().Where(x => x.BrandId == request.BrandId)
                .Where(x => x.CreatedOn >= request.From.ToUniversalTime() &&
                    x.CreatedOn <= request.To.ToUniversalTime())
                .Where(request.GameProviderId, x => request.GameProviderId == x.GameProviderId)
                .Where(request.Status, x => request.Status == x.ClientStatus)
                .Where(x => x.PlayerCode == request.PlayerCode)
                .GroupJoin(_gameService.GetQueries(),
                    round => round.GameId, game => game.Id,
                    (round, game) => new
                    {
                        round,
                        game
                    })
                .SelectMany(x => x.game.DefaultIfEmpty(), (x, game) => new
                {
                    x.round,
                    game
                })
                .Where(request.GameType, x => x.game!.Type == (short?)request.GameType);

            var total = await query.GroupBy(q => q.round.PlayerCode,
                (playerCode, rounds) => new
                {
                    BetAmount = rounds.Sum(r => r.round.BetAmount) ?? 0M,
                    ValidBetAmount = rounds.Sum(r => r.round.ValidBetAmount) ?? 0M,
                    SettleAmount = rounds.Sum(r => r.round.SettleAmount) ?? 0M,
                    WinLoss = rounds.Sum(r => r.round.WinLoss) ?? 0M
                }).FirstOrDefaultAsync(cancellationToken) ?? new
            {
                BetAmount = 0M,
                ValidBetAmount = 0M,
                SettleAmount = 0M,
                WinLoss = 0M
            };

            var betRecords = await query
                .GroupJoin(
                    _playerService.GetQueries(),
                    combine => combine.round.PlayerCode, player => player.PlayerCode,
                    (combine, player) => new
                    {
                        combine.round,
                        combine.game,
                        player
                    })
                .SelectMany(x => x.player.DefaultIfEmpty(), (x, player) => new
                {
                    x.round,
                    x.game,
                    player
                })
                .GroupJoin(_roundDetailService.GetQueries(), c => c.round.Id, detail => detail.RoundId,
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
                .GroupJoin(_gameProviderService.GetQueries(), combine => combine.round.GameProviderId,
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
                .GroupJoin(_brandGameService.GetQueries(),
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
                .Select(combine => new BetRecordModel
                {
                    Id = combine.round.Id,
                    Status = combine.round.ClientStatus,
                    PlayerCode = combine.player!.PlayerCode,
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
                    ValidBetAmount = combine.round.Turnover,
                    Turnover = combine.round.Turnover * (combine.round.EffectiveBettingPercentage ??
                        (combine.brandGame == null ? 100M : combine.brandGame.EffectiveBettingPercentage)) / 100M,
                    SettleAmount = combine.round.SettleAmount,
                    WinLoss = -combine.round.WinLoss,
                    AdditionalData = combine.detail.AdditionalData
                })
                .OrderByDescending(x => x.CreatedOn)
                .CreatePagedResponseAsync(request);

            return new GetBetRecordsResponse
            {
                TotalCount = betRecords.TotalCount,
                Datas = betRecords.Items ?? Array.Empty<BetRecordModel>(),
                Total = new BetRecordTotalModel(total.BetAmount, total.ValidBetAmount, total.SettleAmount,
                    total.WinLoss)
            };
        }
    }
}