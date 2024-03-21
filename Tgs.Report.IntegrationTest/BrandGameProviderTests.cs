using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.IntegrationTest;

[TestFixture]
[Parallelizable]
public class BrandGameProviderTests
{
    [Test]
    public async Task Can_Get_Brand_Game_Providers()
    {
        var brandId = Guid.NewGuid();
        var gameProviderId = Guid.NewGuid();
        var gameProviderId1 = Guid.NewGuid();
        var gameProviderId2 = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new(8, 0, 0));

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                var fakeDbContext = Substitute.For<TgsContext>();
                var fakePlayerGameRebateAggregateService = Substitute.For<ITgsService<PlayerGameRebateAggregate>>();
                var fakeBrandCurrencyService = Substitute.For<ITgsService<BrandCurrency>>();
                var fakeGameProviderService = Substitute.For<ITgsService<GameProvider>>();

                var brandCurrencies = new BrandCurrency[]
                    {
                        CreateBrandCurrencyFixture(brandId).Create<BrandCurrency>()
                    }
                    .AsQueryable()
                    .BuildMock();

                var playerGameRebateAggregates = new PlayerGameRebateAggregate[]
                    {
                        CreatePlayerGameRebateAggregatesFixture(brandId, gameProviderId, createdOn)
                            .Create<PlayerGameRebateAggregate>(),
                        CreatePlayerGameRebateAggregatesFixture(brandId, gameProviderId1, createdOn)
                            .Create<PlayerGameRebateAggregate>(),
                        CreatePlayerGameRebateAggregatesFixture(brandId, gameProviderId2, createdOn)
                            .Create<PlayerGameRebateAggregate>()
                    }
                    .AsQueryable()
                    .BuildMock();

                var gameProviders = new GameProvider[]
                    {
                        CreateGameProviderFixture(gameProviderId).Create<GameProvider>(),
                        CreateGameProviderFixture(gameProviderId1).Create<GameProvider>(),
                        CreateGameProviderFixture(gameProviderId2).Create<GameProvider>()
                    }
                    .AsQueryable()
                    .BuildMock();

                _ = fakePlayerGameRebateAggregateService.GetQueries().Returns(playerGameRebateAggregates);
                _ = fakeBrandCurrencyService.GetQueries().Returns(brandCurrencies);
                _ = fakeGameProviderService.GetQueries().Returns(gameProviders);

                _ = services.AddSingleton(fakeDbContext);
                _ = services.AddTransient(x => fakePlayerGameRebateAggregateService);
                _ = services.AddTransient(x => fakeBrandCurrencyService);
                _ = services.AddTransient(x => fakeGameProviderService);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetBrandGameProvidersAsync(new()
        {
            BrandId = brandId,
            From = createdOn,
            To = createdOn.AddDays(1)
        });

        Assert.That(actual.Datas?.Items, Has.Length.EqualTo(3));
    }

    private static Fixture CreateGameProviderFixture(Guid gameProviderId)
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.GameProvider>(u => u
            .With(s => s.Id, gameProviderId)
            .With(s => s.Games, new Game[]
            {
            })
            .With(s => s.SubGameProviders, new SubGameProvider[]
            {
            }));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateBrandCurrencyFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<BrandCurrency>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Brand, new Brand())
            .With(s => s.CurrencyCode, "CNY")
            .With(s => s.CurrencyCodeNavigation, new Currency()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerGameRebateAggregatesFixture(Guid brandId, Guid gameProviderId, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerGameRebateAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.GameProviderId, gameProviderId)
            .With(s => s.ReportDate, reportDate));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}