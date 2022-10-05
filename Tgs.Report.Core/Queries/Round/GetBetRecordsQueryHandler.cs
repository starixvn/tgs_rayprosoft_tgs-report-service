using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round
{
    public class GetBetRecordsQueryHandler : IRequestHandler<GetBetRecordsRequest, GetBetRecordsResponse>
    {
        private readonly ITgsService<BrandGame> _brandGameService;
        private readonly ITgsService<Repository.Entities.GameProvider> _gameProviderService;
        private readonly ITgsService<Game> _gameService;
        private readonly ITgsService<Repository.Entities.Player> _playerService;
        private readonly ITgsService<RoundDetail> _roundDetailService;
        private readonly ITgsService<Repository.Entities.Round> _roundService;

        public GetBetRecordsQueryHandler(
            ITgsService<Repository.Entities.Round> roundService,
            ITgsService<Repository.Entities.Player> playerService,
            ITgsService<Game> gameService,
            ITgsService<RoundDetail> roundDetailService,
            ITgsService<Repository.Entities.GameProvider> gameProviderService,
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
            var total = await _roundService.GetQueries().Where(x => x.BrandId == request.BrandId)
                .Where(x => x.CreatedOn >= request.From.ToUniversalTime() &&
                    x.CreatedOn <= request.To.ToUniversalTime())
                .Where(request.GameProviderId, x => request.GameProviderId == x.GameProviderId)
                .Where(request.Status, x => (short?)request.Status == x.ClientStatus)
                .Where(x => x.PlayerCode == request.PlayerCode)
                .GroupJoin(_gameService.GetQueries(),
                    round => round.GameId, game => game.Id,
                    (round, game) => new
                    {
                        round.PlayerCode,
                        round.BetAmount,
                        round.Turnover,
                        round.SettleAmount,
                        round.WinLoss,
                        game
                    })
                .SelectMany(x => x.game.DefaultIfEmpty(), (x, game) => new
                {
                    x.PlayerCode,
                    x.BetAmount,
                    x.Turnover,
                    x.SettleAmount,
                    x.WinLoss,
                    GameType = game.Type
                })
                .Where(request.GameType, x => x.GameType == (short?)request.GameType)
                .GroupBy(q => q.PlayerCode,
                (playerCode, rounds) => new
                {
                    BetAmount = rounds.Sum(r => r.BetAmount) ?? 0M,
                    ValidBetAmount = rounds.Sum(r => r.Turnover) ?? 0M,
                    SettleAmount = rounds.Sum(r => r.SettleAmount) ?? 0M,
                    WinLoss = -rounds.Sum(r => r.WinLoss) ?? 0M
                }).FirstOrDefaultAsync(cancellationToken) ?? new
            {
                BetAmount = 0M,
                ValidBetAmount = 0M,
                SettleAmount = 0M,
                WinLoss = 0M
            };

            var betRecords = await _roundService.GetQueries().Where(x => x.BrandId == request.BrandId)
                .Where(x => x.CreatedOn >= request.From.ToUniversalTime() &&
                    x.CreatedOn <= request.To.ToUniversalTime())
                .Where(request.GameProviderId, x => request.GameProviderId == x.GameProviderId)
                .Where(request.Status, x => (short?)request.Status == x.ClientStatus)
                .Where(x => x.PlayerCode == request.PlayerCode)
                .GroupJoin(_gameService.GetQueries(),
                    round => round.GameId, game => game.Id,
                    (round, game) => new
                    {
                        round.Id,
                        round.ClientStatus,
                        round.BrandId,
                        round.GameProviderId,
                        round.GameId,
                        round.PlayerCode,
                        round.BetAmount,
                        round.ValidBetAmount,
                        round.SettleAmount,
                        round.WinLoss,
                        round.ExternalRoundId,
                        round.ExternalCreatedOn,
                        round.ExternalClosedOn,
                        round.CreatedOn,
                        round.ClosedOn,
                        round.Turnover,
                        round.EffectiveBettingPercentage,
                        round.CurrencyCode,
                        game
                    })
                .SelectMany(x => x.game.DefaultIfEmpty(), (x, game) => new
                {
                    RoundId = x.Id,
                    x.ClientStatus,
                    x.BrandId,
                    x.GameProviderId,
                    x.GameId,
                    x.PlayerCode,
                    x.BetAmount,
                    x.ValidBetAmount,
                    x.SettleAmount,
                    x.WinLoss,
                    x.ExternalRoundId,
                    x.ExternalCreatedOn,
                    x.ExternalClosedOn,
                    x.CreatedOn,
                    x.ClosedOn,
                    x.Turnover,
                    x.EffectiveBettingPercentage,
                    x.CurrencyCode,
                    GameName = game.Name,
                    GameType = game.Type,
                    SubGameProviderName = game.SubGameProviderCodeNavigation.Name
                })
                .Where(request.GameType, x => x.GameType == (short?)request.GameType)
                .GroupJoin(
                    _playerService.GetQueries(),
                    combine => combine.PlayerCode, player => player.PlayerCode,
                    (combine, player) => new
                    {
                        combine,
                        player
                    })
                .SelectMany(x => x.player.DefaultIfEmpty(), (x, player) => new
                {
                    x.combine,
                    player.PlayerCode,
                    player.Username
                })
                .GroupJoin(_roundDetailService.GetQueries(), c => c.combine.RoundId, detail => detail.RoundId,
                    (combine, detail) => new
                    {
                        combine.combine,
                        combine.PlayerCode,
                        combine.Username,
                        detail
                    })
                .SelectMany(x => x.detail.DefaultIfEmpty(), (x, detail) => new
                {
                    x.combine,
                    x.PlayerCode,
                    x.Username,
                    detail.AdditionalData
                })
                .GroupJoin(_gameProviderService.GetQueries(), c => c.combine.GameProviderId,
                    gameProvider => gameProvider.Id,
                    (combine, gameProvider) => new
                    {
                        combine.combine,
                        combine.PlayerCode,
                        combine.Username,
                        combine.AdditionalData,
                        gameProvider
                    })
                .SelectMany(x => x.gameProvider.DefaultIfEmpty(),
                    (x, gameProvider) => new
                    {
                        x.combine,
                        x.PlayerCode,
                        x.Username,
                        x.AdditionalData,
                        GameProviderCode = gameProvider.Code,
                        GameProviderName = gameProvider.Name,
                    })
                .GroupJoin(_brandGameService.GetQueries(),
                    roundGroup => new
                    {
                        roundGroup.combine.BrandId,
                        roundGroup.combine.GameId
                    },
                    brandGame => new
                    {
                        brandGame.BrandId,
                        brandGame.GameId
                    },
                    (roundGroup, brandGame) => new
                    {
                        roundGroup.combine,
                        roundGroup.PlayerCode,
                        roundGroup.Username,
                        roundGroup.AdditionalData,
                        roundGroup.GameProviderCode,
                        roundGroup.GameProviderName,
                        brandGame
                    })
                .SelectMany(x => x.brandGame.DefaultIfEmpty(),
                    (x, brandGame) => new
                    {
                        x.combine,
                        x.PlayerCode,
                        x.Username,
                        x.AdditionalData,
                        x.GameProviderCode,
                        x.GameProviderName,
                        brandGame
                    })
                .Select(combine => new BetRecordModel
                {
                    Id = combine.combine.RoundId,
                    Status = (ClientRoundStatus)combine.combine.ClientStatus,
                    PlayerCode = combine.PlayerCode,
                    ExternalPlayerCode = combine.PlayerCode,
                    PlayerName = combine.Username,
                    GameProviderId = combine.combine.GameProviderId,
                    GameProviderCode = combine.GameProviderCode,
                    GameProviderName = combine.GameProviderName,
                    SubGameProviderName = combine.combine.SubGameProviderName,
                    GameId = combine.combine.GameId,
                    GameName = combine.combine.GameName,
                    ExternalRoundId = combine.combine.ExternalRoundId,
                    ExternalCreatedOn = combine.combine.ExternalCreatedOn,
                    ExternalClosedOn = combine.combine.ExternalClosedOn,
                    CreatedOn = combine.combine.CreatedOn,
                    ClosedOn = combine.combine.ClosedOn,
                    CurrencyCode = combine.combine.CurrencyCode,
                    BetAmount = combine.combine.BetAmount,
                    ValidBetAmount = combine.combine.Turnover,
                    Turnover = combine.combine.Turnover * (combine.combine.EffectiveBettingPercentage ??
                        (combine.brandGame == null ? 100M : combine.brandGame.EffectiveBettingPercentage)) / 100M,
                    SettleAmount = combine.combine.SettleAmount,
                    WinLoss = -combine.combine.WinLoss,
                    AdditionalData = combine.AdditionalData
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