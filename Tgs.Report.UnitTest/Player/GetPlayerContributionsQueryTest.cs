using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Player;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.UnitTest.Player;

[TestFixture]
[Parallelizable]
public sealed class GetPlayerContributionsQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Player_Contributions()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var sut = new GetPlayerContributionsQueryHandler(fakeDbContext);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");

        var now = DateTimeOffset.Now;

        var yesterday = new DateTimeOffset(now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0,
            new(8, 0, 0)).AddDays(-1);

        var reportDate = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new(8, 0, 0));

        var playerCodes = new[]
        {
            "player1",
            "player2",
            "player3"
        };

        var brands = CreateBrandFixture(brandId, 5)
            .CreateMany<Brand>(1)
            .AsQueryable()
            .BuildMock();

        var playerAggregates = playerCodes.SelectMany(x
                => new[]
                {
                    CreatePlayerAggregateFixture(brandId, reportDate, x).Create<PlayerAggregate>(),
                    CreatePlayerAggregateFixture(brandId, yesterday, x).Create<PlayerAggregate>()
                })
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var playerBonusAggregates = playerCodes.SelectMany(x
                => new[]
                {
                    CreatePlayerBonusAggregateFixture(brandId, reportDate, x).Create<PlayerBonusAggregate>(),
                    CreatePlayerBonusAggregateFixture(brandId, yesterday, x).Create<PlayerBonusAggregate>()
                })
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var withdrawForms = playerCodes
            .Select(x => CreateWithdrawFormFixture(brandId, reportDate, x).Create<WithdrawForm>())
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var depositForms = playerCodes
            .SelectMany(x => new[]
            {
                CreateDepositFormFixture(brandId, reportDate, x).Create<DepositForm>(),
                CreateDepositFormFixture(brandId, yesterday, x).Create<DepositForm>()
            })
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var players = playerCodes
            .Select(x => CreatePlayerFixture(brandId, x).Create<Repository.Entities.Player>())
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var playerDailyPaymentStatistics = playerCodes
            .SelectMany(x => new[]
            {
                CreatePlayerDailyPaymentStatisticFixture(reportDate, x).Create<PlayerDailyPaymentStatistic>(),
                CreatePlayerDailyPaymentStatisticFixture(yesterday, x).Create<PlayerDailyPaymentStatistic>()
            })
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var request = new GetPlayerContributionsRequest
        {
            BrandId = brandId,
            From = yesterday,
            To = reportDate
        };

        var mockPlayerAggregates = GetDbSet(playerAggregates);
        var mockPlayerBonusAggregates = GetDbSet(playerBonusAggregates);
        var mockWithdrawForms = GetDbSet(withdrawForms);
        var mockDepositForms = GetDbSet(depositForms);
        var mockPlayers = GetDbSet(players);
        var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);
        var mockBrands = GetDbSet(brands);

        fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);
        fakeDbContext.PlayerBonusAggregates.Returns(mockPlayerBonusAggregates.Object);
        fakeDbContext.WithdrawForms.Returns(mockWithdrawForms.Object);
        fakeDbContext.DepositForms.Returns(mockDepositForms.Object);
        fakeDbContext.Players.Returns(mockPlayers.Object);
        fakeDbContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);
        fakeDbContext.Brands.Returns(mockBrands.Object);

        var response = await sut.Handle(request, default);

        Assert.That(response.Items?.Length, Is.EqualTo(3));

        await fakeDbContext.DepositForms.Where(x => x.UpdatedOn == yesterday).GroupBy(x => x.PlayerCode).Select(x => new
        {
            PlayerCode = x.Key,
            Amount = x.Sum(s => s.ActualAmount ?? s.Amount)
        }).ForEachAsync(x =>
        {
            Assert.That(response.Items!.First(f => f.PlayerCode == x.PlayerCode).DepositAmount, Is.EqualTo(x.Amount));
        });
    }

    [Test]
    public async Task Can_Get_Today_Player_Contributions()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var sut = new GetPlayerContributionsQueryHandler(fakeDbContext);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");

        var now = DateTimeOffset.Now;

        var yesterday = new DateTimeOffset(now.Year,
            now.Month,
            now.Day,
            0,
            0,
            0,
            new(8, 0, 0)).AddDays(-1);

        var reportDate = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, new(8, 0, 0));

        var playerCodes = new[] { "player1", "player2", "player3" };

        var brands = CreateBrandFixture(brandId, 5)
            .CreateMany<Brand>(1)
            .AsQueryable()
            .BuildMock();

        var playerAggregates = playerCodes.SelectMany(x
                => new[]
                {
                    CreatePlayerAggregateFixture(brandId, reportDate, x).Create<PlayerAggregate>(),
                    CreatePlayerAggregateFixture(brandId, yesterday, x).Create<PlayerAggregate>()
                })
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var playerBonusAggregates = playerCodes.SelectMany(x
                => new[]
                {
                    CreatePlayerBonusAggregateFixture(brandId, reportDate, x).Create<PlayerBonusAggregate>(),
                    CreatePlayerBonusAggregateFixture(brandId, yesterday, x).Create<PlayerBonusAggregate>()
                })
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var withdrawForms = playerCodes
            .Select(x => CreateWithdrawFormFixture(brandId, reportDate, x).Create<WithdrawForm>())
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var depositForms = playerCodes
            .SelectMany(x => new[]
            {
                CreateDepositFormFixture(brandId, reportDate, x).Create<DepositForm>(),
                CreateDepositFormFixture(brandId, yesterday, x).Create<DepositForm>()
            })
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var players = playerCodes
            .Select(x => CreatePlayerFixture(brandId, x).Create<Repository.Entities.Player>())
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var playerDailyPaymentStatistics = playerCodes
            .SelectMany(x => new[]
            {
                CreatePlayerDailyPaymentStatisticFixture(reportDate, x).Create<PlayerDailyPaymentStatistic>(),
                CreatePlayerDailyPaymentStatisticFixture(yesterday, x).Create<PlayerDailyPaymentStatistic>()
            })
            .ToArray()
            .AsQueryable()
            .BuildMock();

        var request = new GetPlayerContributionsRequest
        {
            BrandId = brandId, From = reportDate, To = reportDate.AddDays(1)
        };

        var mockPlayerAggregates = GetDbSet(playerAggregates);
        var mockPlayerBonusAggregates = GetDbSet(playerBonusAggregates);
        var mockWithdrawForms = GetDbSet(withdrawForms);
        var mockDepositForms = GetDbSet(depositForms);
        var mockPlayers = GetDbSet(players);
        var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);
        var mockBrands = GetDbSet(brands);

        fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);
        fakeDbContext.PlayerBonusAggregates.Returns(mockPlayerBonusAggregates.Object);
        fakeDbContext.WithdrawForms.Returns(mockWithdrawForms.Object);
        fakeDbContext.DepositForms.Returns(mockDepositForms.Object);
        fakeDbContext.Players.Returns(mockPlayers.Object);
        fakeDbContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);
        fakeDbContext.Brands.Returns(mockBrands.Object);

        var response = await sut.Handle(request, default);

        var totalNetProfit =
            playerAggregates.Where(x => x.PlayerCode == "player1" && x.ReportDate == reportDate)
                .Sum(x =>
                    x.WinLossAmount -
                    (x.AgentAdjustAmount - x.NoIncludeNetProfitAgentAdjustAmount) -
                    (x.BonusAdjustAmount - x.NoIncludeNetProfitBonusAdjustAmount) -
                    x.PromotionAmount -
                    x.RebateAmount -
                    x.WinLossAmount * 5 / 100) -
            playerBonusAggregates.Where(x => x.PlayerCode == "player1" && x.ReportDate == reportDate)
                .Sum(x => x.SeasonAmount + x.UpgradeAmount + x.BirthdayAmount);

        Assert.That(response.Items?.Length, Is.EqualTo(3));

        Assert.That(response.Items.Where(x => x.PlayerCode == "player1").Sum(x => x.NetProfit) == totalNetProfit);

        await fakeDbContext.DepositForms.Where(x => x.UpdatedOn == reportDate)
            .GroupBy(x => x.PlayerCode)
            .Select(x => new { PlayerCode = x.Key, Amount = x.Sum(s => s.ActualAmount ?? s.Amount) })
            .ForEachAsync(x =>
            {
                Assert.That(response.Items!.First(f => f.PlayerCode == x.PlayerCode).DepositAmount,
                    Is.EqualTo(x.Amount));
            });
    }

    private static Fixture CreatePlayerAggregateFixture(Guid brandId, DateTimeOffset reportDate, string playerCode)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.ReportDate, reportDate.UtcDateTime)
            .With(s => s.CreatedOn, reportDate.UtcDateTime));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerBonusAggregateFixture(Guid brandId, DateTimeOffset reportDate, string playerCode)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerBonusAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.ReportDate, reportDate.UtcDateTime));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateWithdrawFormFixture(Guid brandId, DateTimeOffset reportDate, string playerCode)
    {
        var fixture = new Fixture();
        fixture.Customize<WithdrawForm>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.UserId, playerCode)
            .With(s => s.PaidOn, reportDate.UtcDateTime)
            .With(s => s.Brand, new Brand()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateDepositFormFixture(Guid brandId, DateTimeOffset reportDate, string playerCode)
    {
        var fixture = new Fixture();
        fixture.Customize<DepositForm>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.UpdatedOn, reportDate.UtcDateTime)
            .With(s => s.Status, 8000)
            .With(s => s.Brand, new Brand())
            .With(s => s.PlayerCodeNavigation, new Repository.Entities.Player()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerFixture(Guid brandId, string playerCode)
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.Player>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.AgentId, (Guid?)null)
            .With(s => s.Brand, new Brand())
            .With(s => s.Birthdate, DateOnly.Parse("2022/05/04")));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerDailyPaymentStatisticFixture(DateTimeOffset reportDate, string playerCode)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerDailyPaymentStatistic>(u => u
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.ReportDate, new DateOnly(reportDate.Year, reportDate.Month, reportDate.Day)));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateBrandFixture(Guid brandId, decimal platformCharge)
    {
        var fixture = new Fixture();
        fixture.Customize<Brand>(u => u
            .With(s => s.Id, brandId)
            .With(s => s.PlatformCharge, platformCharge)
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
}