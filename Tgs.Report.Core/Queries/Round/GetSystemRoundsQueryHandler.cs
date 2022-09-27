using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round
{
    public class GetSystemRoundsQueryHandler : IRequestHandler<GetSystemRoundsRequest, PagedResponse<SystemRoundModel>>
    {
        private readonly TgsContext _context;

        public GetSystemRoundsQueryHandler(TgsContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public Task<PagedResponse<SystemRoundModel>> Handle(
            GetSystemRoundsRequest request,
            CancellationToken cancellationToken)
            => _context.Rounds
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
                .GroupJoin(_context.GameProviders, combine => combine.round.GameProviderId,
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
                .GroupJoin(_context.BrandGames,
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
                .Select(combine => new SystemRoundModel
                {
                    PlayerName = combine.player!.Username,
                    PlayerCode = combine.player.PlayerCode,
                    ExternalRoundId = combine.round.ExternalRoundId,
                    ExternalCreatedOn = combine.round.ExternalCreatedOn,
                    ExternalClosedOn = combine.round.ExternalClosedOn,
                    Id = combine.round.Id,
                    CreatedOn = combine.round.CreatedOn,
                    ClosedOn = combine.round.ClosedOn,
                    GameProviderName = combine.gameProvider == null ? "" : combine.gameProvider.Name,
                    SubGameProviderName = combine.game == null || combine.game.SubGameProviderCodeNavigation == null
                        ? ""
                        : combine.game.SubGameProviderCodeNavigation.Name,
                    GameName = combine.game == null ? "" : combine.game.Name,
                    Status = (ClientRoundStatus)combine.round.ClientStatus,
                    CurrencyCode = combine.round.CurrencyCode,
                    BetAmount = combine.round.BetAmount,
                    ValidBetAmount = combine.round.Turnover,
                    Turnover = combine.round.Turnover * (combine.round.EffectiveBettingPercentage ??
                        (combine.brandGame == null ? 100M : combine.brandGame.EffectiveBettingPercentage)) / 100M,
                    SettleAmount = combine.round.SettleAmount,
                    WinLoss = -combine.round.WinLoss,
                    AdditionalData = combine.detail == null ? "" : combine.detail.AdditionalData,
                    GameProviderCode = combine.game == null ? "" : combine.game.GameProvider.Code,
                    GameId = combine.game == null ? 0 : combine.game.Id,
                })
                .OrderByDescending(x => x.CreatedOn)
                .CreatePagedResponseAsync(request);
    }
}