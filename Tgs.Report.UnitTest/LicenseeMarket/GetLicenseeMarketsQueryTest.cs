using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.LicenseeMarket;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.LicenseeMarket;

namespace Tgs.Report.UnitTest.LicenseeMarket;

[TestFixture]
[Parallelizable]
public sealed class GetLicenseeMarketsQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Licensee_Markets()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var sut = new GetLicenseeMarketsQueryHandler(fakeDbContext);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var licenseeId = new Guid("aac11120-20f6-4f0d-6982-08d9dbdc0473");

        var brands = CreateBrandFixture(brandId, licenseeId).CreateMany<Brand>(1)
            .AsQueryable()
            .BuildMock();

        var brandCurrencies = brands.Select(x => CreateBrandCurrenciesFixture(x).Create<BrandCurrency>()).BuildMock();

        var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable().BuildMock();

        var playerAggregates = CreatePlayerAggregateFixture(brandId).CreateMany<PlayerAggregate>(1)
            .AsQueryable()
            .BuildMock();

        var playerBonusAggregates = CreatePlayerBonusAggregateFixture(brandId).CreateMany<PlayerBonusAggregate>(1)
            .AsQueryable()
            .BuildMock();

        var request = new GetLicenseeMarketsRequest
        {
            LicenseeId = licenseeId,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1),
            SortBy = null
        };

        var brandDailyPaymentStatistics =
            CreateBrandDailyPaymentStatisticsFixture()
                .CreateMany<BrandDailyPaymentStatistic>(1)
                .AsQueryable()
                .BuildMock();

        var playerDailyPaymentStatistics =
            CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                .BuildMock();

        var deposits = CreateDepositFormFixture(brandId).CreateMany<DepositForm>(5).AsQueryable()
            .BuildMock();

        var players = CreatePlayerFixture(brandId).CreateMany<Repository.Entities.Player>(1).AsQueryable()
            .BuildMock();

        var mockBrandDailyPaymentStatistics = GetDbSet(brandDailyPaymentStatistics);
        var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);

        var mockBrands = GetDbSet(brands);
        var mockPlayerAggregates = GetDbSet(playerAggregates);
        var mockWithdrawForms = GetDbSet(withdrawForms);
        var mockPlayerBonusAggregates = GetDbSet(playerBonusAggregates);
        var mockDepositForms = GetDbSet(deposits);
        var mockPlayers = GetDbSet(players);
        var mockBrandCurrencies = GetDbSet(brandCurrencies);

        fakeDbContext.Brands.Returns(mockBrands.Object);
        fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);
        fakeDbContext.WithdrawForms.Returns(mockWithdrawForms.Object);
        fakeDbContext.PlayerBonusAggregates.Returns(mockPlayerBonusAggregates.Object);
        fakeDbContext.BrandDailyPaymentStatistics.Returns(mockBrandDailyPaymentStatistics.Object);
        fakeDbContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);
        fakeDbContext.DepositForms.Returns(mockDepositForms.Object);
        fakeDbContext.Players.Returns(mockPlayers.Object);
        fakeDbContext.BrandCurrencies.Returns(mockBrandCurrencies.Object);

        var response = await sut.Handle(request, default);

        Assert.That(response.Datas?.TotalCount, Is.EqualTo(1));
    }

    private static Fixture CreateBrandFixture(Guid brandId, Guid licenseeId)
    {
        var fixture = new Fixture();
        fixture.Customize<Brand>(u => u
            .With(s => s.Id, brandId)
            .With(s => s.LicenseeId, licenseeId)
            .With(s => s.Licensee, new Licensee{ResellerId = Guid.NewGuid()})
            .With(s => s.VipLevels, new VipLevel[]
            {
            })
            .With(s => s.Transactions, new Transaction[]
            {
            })
            .With(s => s.BrandCurrencies, new BrandCurrency[]
            {
            })
            .With(s => s.Agents, new Repository.Entities.Agent[]
            {
            })
            .With(s => s.Players, new Repository.Entities.Player[]
            {
            })
            .With(s => s.DepositForms, new DepositForm[]
            {
            })
            .With(s => s.PlayerLoginRecords, new PlayerLoginRecord[]
            {
            }));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateBrandCurrenciesFixture(Brand brand)
    {
        var fixture = new Fixture();
        fixture.Customize<BrandCurrency>(u => u
            .With(s => s.BrandId, brand.Id)
            .With(s => s.Brand, brand)
            .With(s => s.CurrencyCodeNavigation, new Currency()));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateWithdrawFormFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<WithdrawForm>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Brand, new Brand()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateDepositFormFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<DepositForm>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Brand, new Brand())
            .With(s => s.PlayerCodeNavigation, new Repository.Entities.Player()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerAggregateFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.ReportDate, DateTime.UtcNow));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerBonusAggregateFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerBonusAggregate>(u => u
            .With(s => s.BrandId, brandId));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerDailyPaymentStatisticsFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerDailyPaymentStatistic>(u => u
            .With(s => s.ReportDate,
                DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime)));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.Player>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Brand, new Brand())
            .With(s => s.Birthdate, DateOnly.Parse("2022/05/04")));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateBrandDailyPaymentStatisticsFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<BrandDailyPaymentStatistic>(u => u
            .With(s => s.ReportDate,
                DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(-1).DateTime)));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}