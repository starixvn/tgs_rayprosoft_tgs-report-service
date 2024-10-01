using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round;

public class GetClosedRoundsQueryHandler(TgsContext context)
    : IRequestHandler<GetClosedRoundsRequest, PagedResponse<ClosedRoundModel>>
{
    private readonly TgsContext _context = context ?? throw new ArgumentNullException(nameof(context));

    public Task<PagedResponse<ClosedRoundModel>> Handle(
        GetClosedRoundsRequest request,
        CancellationToken cancellationToken)
        => _context.Rounds
            .Where(x => x.Status == (short)RoundStatus.Closed)
            .Where(request.GameProviderId, x => request.GameProviderId == x.GameProviderId)
            .Where(request.ExternalRoundId, x => x.ExternalRoundId == request.ExternalRoundId)
            .GroupJoin(
                _context.Players,
                round => round.PlayerCode,
                player => player.PlayerCode,
                (round, player) => new
                {
                    round,
                    player
                })
            .SelectMany(
                x => x.player.DefaultIfEmpty(),
                (x, player) => new
                {
                    x.round,
                    player
                })
            .GroupJoin(
                _context.Games,
                combine => combine.round.GameId,
                game => game.Id,
                (combine, game) => new
                {
                    combine.round,
                    combine.player,
                    game
                })
            .SelectMany(
                x => x.game.DefaultIfEmpty(),
                (x, game) => new
                {
                    x.round,
                    x.player,
                    game
                })
            .GroupJoin(
                _context.GameProviders,
                combine => combine.round.GameProviderId,
                gameProvider => gameProvider.Id,
                (combine, gameProvider) => new
                {
                    combine.round,
                    combine.player,
                    combine.game,
                    gameProvider
                })
            .SelectMany(
                x => x.gameProvider.DefaultIfEmpty(),
                (x, gameProvider) => new
                {
                    x.round,
                    x.player,
                    x.game,
                    gameProvider
                })
            .GroupJoin(
                _context.BrandGames,
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
                    brandGame
                })
            .SelectMany(
                x => x.brandGame.DefaultIfEmpty(),
                (x, brandGame) => new
                {
                    x.round,
                    x.player,
                    x.game,
                    x.gameProvider,
                    brandGame
                })
            .Select(combine => new ClosedRoundModel
            {
                Id = combine.round.Id,
                BrandCode = combine.player!.Brand.Code,
                BrandName = combine.player.Brand.Name,
                PlayerType = (PlayerType)combine.player.Type,
                Status = (ClientRoundStatus)combine.round.ClientStatus,
                PlayerCode = combine.player.PlayerCode,
                ExternalPlayerCode = combine.player.PlayerCode,
                PlayerName = combine.player.Username,
                GameProviderId = combine.round.GameProviderId,
                GameProviderCode = combine.game!.GameProvider.Code,
                GameProviderName = combine.gameProvider!.Name,
                SubGameProviderName = combine.game.SubGameProviderCodeNavigation!.Name,
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
                AdditionalData = ""
            })
            .OrderByDescending(x => x.CreatedOn)
            .CreatePagedResponseAsync(request);
}