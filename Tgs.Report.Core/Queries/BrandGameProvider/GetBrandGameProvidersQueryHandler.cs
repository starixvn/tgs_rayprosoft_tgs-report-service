using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.BrandGameProvider;

namespace Tgs.Report.Core.Queries.BrandGameProvider;

public class
    GetBrandGameProvidersQueryHandler(
        ITgsService<PlayerGameRebateAggregate> playerGameRebateAggregate,
        ITgsService<BrandCurrency> brandCurrencyService,
        ITgsService<Repository.Entities.GameProvider> gameProviderService)
    : IRequestHandler<GetBrandGameProvidersRequest,
        GetBrandGameProvidersResponse>
{
    public async Task<GetBrandGameProvidersResponse> Handle(
        GetBrandGameProvidersRequest request,
        CancellationToken cancellationToken)
    {
        var brandCurrency = await brandCurrencyService.GetQueries()
            .FirstOrDefaultAsync(x => x.BrandId == request.BrandId, cancellationToken);

        var response = await playerGameRebateAggregate.GetQueries()
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= request.From && x.ReportDate < request.To)
            .Join(gameProviderService.GetQueries(),
                a => a.GameProviderId,
                g => g.Id,
                (round, gameProvider) => new
                {
                    round.BrandId,
                    round.GameProviderId,
                    round.GameType,
                    round.RoundCount,
                    round.BetAmount,
                    round.ValidBetAmount,
                    round.Turnover,
                    round.WinLoss,
                    round.ReportDate,
                    round.PlayerCode,
                    GameProviderCode = gameProvider.Code,
                    GameProviderName = gameProvider.Name
                })
            .Where(request.GameProviderCode, x => x.GameProviderCode == request.GameProviderCode)
            .Where(request.GameType is not null, x => x.GameType == (short)request.GameType!)
            .GroupBy(x => new
            {
                x.GameProviderId,
                x.GameProviderName,
                x.ReportDate
            }, (group, items) => new
            {
                GameProviderId = group.GameProviderId,
                GameProviderName = group.GameProviderName,
                ReportDate = group.ReportDate,
                PlayerCount = items.Select(i => i.PlayerCode).Distinct().Count(),
                TransactionCount = items.Sum(i => i.RoundCount),
                BetAmount = items.Sum(i => i.BetAmount),
                OriginalTurnover = items.Sum(i => i.Turnover),
                ValidBetAmount = items.Sum(i => i.ValidBetAmount),
                CompanyWinLoss = items.Sum(i => i.WinLoss)
            })
            .GroupBy(x => new
            {
                x.GameProviderId,
                x.GameProviderName
            }, (group, items) => new BrandGameProvidersModel
            {
                GameProviderId = group.GameProviderId,
                GameProviderName = group.GameProviderName,
                CurrencyCode = brandCurrency!.CurrencyCode,
                PlayerCount = items.Sum(i => i.PlayerCount),
                TransactionCount = items.Sum(i => i.TransactionCount),
                BetAmount = items.Sum(i => i.BetAmount),
                OriginalTurnover = items.Sum(i => i.OriginalTurnover),
                ValidBetAmount = items.Sum(i => i.ValidBetAmount),
                CompanyWinLoss = items.Sum(i => i.CompanyWinLoss)
            })
            .ToArrayAsync(cancellationToken).ConfigureAwait(false);

        return new GetBrandGameProvidersResponse
        {
            Datas = response.AsQueryable().CreatePagedResponse(request),
            Total = response.GroupBy(x => new { x.CurrencyCode },
                            (group, items) => new BrandGameProviderTotalModel
                            {
                                CurrencyCode = group.CurrencyCode,
                                TransactionCount = items.Sum(i => i.TransactionCount),
                                BetAmount = items.Sum(i => i.BetAmount),
                                ValidBetAmount = items.Sum(i => i.ValidBetAmount),
                                OriginalTurnover = items.Sum(i => i.OriginalTurnover),
                                CompanyWinLoss = items.Sum(i => i.CompanyWinLoss)
                            })
                        .FirstOrDefault() ??
                    new BrandGameProviderTotalModel { CurrencyCode = brandCurrency!.CurrencyCode }
        };
    }
}