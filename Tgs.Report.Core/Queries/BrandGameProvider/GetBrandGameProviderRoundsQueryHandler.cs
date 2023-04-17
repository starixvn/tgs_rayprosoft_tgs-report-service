using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.BrandGameProvider;

namespace Tgs.Report.Core.Queries.BrandGameProvider;

public class
    GetBrandGameProviderRoundsQueryHandler : IRequestHandler<GetBrandGameProviderRoundsRequest,
        GetBrandGameProviderRoundsResponse>
{
    private readonly ITgsService<BrandGameProviderAggregate> _brandGameProviderAggregateService;
    private readonly ITgsService<Brand> _brandService;
    private readonly ITgsService<Repository.Entities.GameProvider> _gameProviderService;

    public GetBrandGameProviderRoundsQueryHandler(
        ITgsService<BrandGameProviderAggregate> brandGameProviderAggregateService, ITgsService<Brand> brandService,
        ITgsService<Repository.Entities.GameProvider> gameProviderService)
    {
        _brandGameProviderAggregateService = brandGameProviderAggregateService;
        _brandService = brandService;
        _gameProviderService = gameProviderService;
    }

    public async Task<GetBrandGameProviderRoundsResponse> Handle(
        GetBrandGameProviderRoundsRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _brandGameProviderAggregateService.GetQueries()
            .Where(x => x.BrandId == request.BrandId)
            .Where(x => x.ReportDate >= request.From && x.ReportDate < request.To)
            .Join(_gameProviderService.GetQueries(),
                a => a.GameProviderId,
                g => g.Id,
                (round, gameProvider) => new
                {
                    round.BrandId,
                    round.GameProviderId,
                    round.GameType,
                    round.CurrencyCode,
                    round.TransactionCount,
                    round.BetAmount,
                    round.ValidBetAmount,
                    round.Turnover,
                    round.WinLoss,
                    GameProviderCode = gameProvider.Code,
                    GameProviderName = gameProvider.Name
                })
            .Where(request.GameProviderCode, x => x.GameProviderCode == request.GameProviderCode)
            .Where(request.GameType is not null, x => x.GameType == (short)request.GameType!)
            .Join(_brandService.GetQueries(),
                a => a.BrandId,
                brand => brand.Id,
                (group, brand) => new
                {
                    group.GameProviderId,
                    group.CurrencyCode,
                    group.TransactionCount,
                    group.BetAmount,
                    group.ValidBetAmount,
                    group.Turnover,
                    group.WinLoss,
                    group.GameProviderCode,
                    group.GameProviderName,
                    BrandName = brand.Name
                })
            .GroupBy(x => new
            {
                x.GameProviderId,
                x.GameProviderName,
                x.CurrencyCode
            }, (group, items) => new BrandGameProviderRoundsModel
            {
                GameProviderId = group.GameProviderId,
                GameProviderName = group.GameProviderName,
                CurrencyCode = group.CurrencyCode,
                TransactionCount = items.Sum(i => i.TransactionCount),
                BetAmount = items.Sum(i => i.BetAmount),
                OriginalTurnover = items.Sum(i => i.Turnover),
                ValidBetAmount = items.Sum(i => i.ValidBetAmount),
                CompanyWinLoss = items.Sum(i => i.WinLoss)
            }).ToArrayAsync(cancellationToken).ConfigureAwait(false);

        return new()
        {
            Datas = response.AsQueryable().CreatePagedResponse(request),
            Totals = response.GroupBy(x => new { x.CurrencyCode },
                (group, items) => new BrandGameProviderRoundTotalModel
                {
                    CurrencyCode = group.CurrencyCode,
                    TransactionCount = items.Sum(i => i.TransactionCount),
                    BetAmount = items.Sum(i => i.BetAmount),
                    ValidBetAmount = items.Sum(i => i.ValidBetAmount),
                    OriginalTurnover = items.Sum(i => i.OriginalTurnover),
                    CompanyWinLoss = items.Sum(i => i.CompanyWinLoss)
                }).ToArray()
        };
    }
}