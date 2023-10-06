using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.IntegrationTest;

[TestFixture]
[Parallelizable]
public class MarketTests
{
    [Test]
    public async Task Can_Get_Markets()
    {
        var brandId = Guid.NewGuid();
        var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new(8, 0, 0));

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                var fakeDbContext = Substitute.For<TgsContext>();
                var fakePlayerAggregateDbSet = Substitute.For<DbSet<PlayerAggregate>, IQueryable<PlayerAggregate>>();
                var fakeWithdrawFormDbSet = Substitute.For<DbSet<WithdrawForm>, IQueryable<WithdrawForm>>();
                var fakePlayerDbSet = Substitute.For<DbSet<Player>, IQueryable<Player>>();
                var fakePlayerLoginRecordDbSet = Substitute.For<DbSet<PlayerLoginRecord>, IQueryable<PlayerLoginRecord>>();
                var fakeBrandDailyPaymentStatisticDbSet = Substitute.For<DbSet<BrandDailyPaymentStatistic>, IQueryable<BrandDailyPaymentStatistic>>();
                var fakePlayerBonusAggregateDbSet = Substitute.For<DbSet<PlayerBonusAggregate>, IQueryable<PlayerBonusAggregate>>();
                var fakePlayerDailyPaymentStatisticDbSet = Substitute.For<DbSet<PlayerDailyPaymentStatistic>, IQueryable<PlayerDailyPaymentStatistic>>();
                var fakePlayerDailyRegisterStatisticDbSet = Substitute.For<DbSet<PlayerDailyRegisterStatistic>, IQueryable<PlayerDailyRegisterStatistic>>();

                var playerAggregates = CreatePlayerAggregateFixture(brandId, reportDate).CreateMany<PlayerAggregate>(1)
                    .AsQueryable()
                    .BuildMock();

                var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                    .BuildMock();

                var playerLoginRecords = CreatePlayerLoginRecordsFixture(brandId).CreateMany<PlayerLoginRecord>(1)
                    .AsQueryable()
                    .BuildMock();

                var dailyStatistic = CreateBrandDailyPaymentStatisticFixture(brandId, reportDate)
                    .CreateMany<BrandDailyPaymentStatistic>(1).AsQueryable()
                    .BuildMock();

                var playerBonusAggregates = CreatePlayerBonusAggregateFixture(brandId, reportDate)
                    .CreateMany<PlayerBonusAggregate>(1).AsQueryable()
                    .BuildMock();

                var dailyRegisterStatistic = CreatePlayerDailyRegisterStatisticsFixture(brandId, reportDate)
                    .CreateMany<PlayerDailyRegisterStatistic>(1).AsQueryable()
                    .BuildMock();

                var playerDailyPaymentStatistics =
                    CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                        .BuildMock();

                var players = CreatePlayerFixture(brandId, reportDate).CreateMany<Player>(1).AsQueryable()
                    .BuildMock();

                foreach (var item in dailyRegisterStatistic) item.PlayerCode = players.First().PlayerCode;

                var fakePlayerAggregates = fakePlayerAggregateDbSet.Initialize(playerAggregates);
                var fakeWithdrawForms = fakeWithdrawFormDbSet.Initialize(withdrawForms);
                var fakePlayerLoginRecords = fakePlayerLoginRecordDbSet.Initialize(playerLoginRecords);
                var fakeDailyStatistic = fakeBrandDailyPaymentStatisticDbSet.Initialize(dailyStatistic);
                var fakePlayerBonusAggregates = fakePlayerBonusAggregateDbSet.Initialize(playerBonusAggregates);
                var fakePlayerDailyPaymentStatistics = fakePlayerDailyPaymentStatisticDbSet.Initialize(playerDailyPaymentStatistics);
                var fakePlayers = fakePlayerDbSet.Initialize(players);
                var fakeDailyRegisterStatistics = fakePlayerDailyRegisterStatisticDbSet.Initialize(dailyRegisterStatistic);

                _ = fakeDbContext.WithdrawForms.Returns(fakeWithdrawForms);
                _ = fakeDbContext.PlayerAggregates.Returns(fakePlayerAggregates);
                _ = fakeDbContext.PlayerLoginRecords.Returns(fakePlayerLoginRecords);
                _ = fakeDbContext.BrandDailyPaymentStatistics.Returns(fakeDailyStatistic);
                _ = fakeDbContext.PlayerBonusAggregates.Returns(fakePlayerBonusAggregates);
                _ = fakeDbContext.PlayerDailyPaymentStatistics.Returns(fakePlayerDailyPaymentStatistics);
                _ = fakeDbContext.PlayerDailyRegisterStatistics.Returns(fakeDailyRegisterStatistics);
                _ = fakeDbContext.Players.Returns(fakePlayers);

                _ = services.AddSingleton(fakeDbContext);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetMarketsAsync(new()
        {
            BrandId = brandId,
            From = reportDate,
            To = reportDate.AddDays(1)
        });

        Assert.IsNotNull(actual);
        Assert.IsTrue(actual.Items.Length == 1);
    }

    private static Fixture CreatePlayerAggregateFixture(Guid brandId, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.ReportDate, reportDate)
            .With(s => s.CreatedOn, reportDate));
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

    private static Fixture CreatePlayerLoginRecordsFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerLoginRecord>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Brand, new Brand())
            .With(s => s.PlayerCodeNavigation, new Player()));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateBrandDailyPaymentStatisticFixture(Guid brandId, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<BrandDailyPaymentStatistic>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.ReportDate, new DateOnly(reportDate.Year, reportDate.Month, reportDate.Day - 1)));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerBonusAggregateFixture(Guid brandId, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerBonusAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.ReportDate, reportDate));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerDailyPaymentStatisticsFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerDailyPaymentStatistic>(u => u
            .With(s => s.ReportDate,
                new DateOnly(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day)));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerFixture(Guid brandId, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<Player>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Brand, new Brand())
            .With(s => s.RegisteredOn, reportDate)
            .With(s => s.Birthdate, DateOnly.Parse("2022/05/04")));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerDailyRegisterStatisticsFixture(Guid brandId, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerDailyRegisterStatistic>(u => u
            .With(s => s.ReportDate, DateOnly.FromDateTime(reportDate.DateTime)));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}