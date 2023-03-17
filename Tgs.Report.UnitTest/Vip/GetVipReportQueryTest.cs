using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Vip;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.UnitTest.Vip;

[TestFixture]
public sealed class GetVipReportQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_vip_report()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var sut = new GetVipReportQueryHandler(fakeDbContext);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");

        var players = CreatePlayerFixture(brandId)
            .CreateMany<Repository.Entities.Player>(1).AsQueryable()
            .BuildMock();

        var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
            .BuildMock();

        var playerAggregates = CreatePlayerAggregateFixture(brandId).CreateMany<PlayerAggregate>(1).AsQueryable()
            .BuildMock();

        var playerGameAggregates = CreatePlayerGameRebateAggregateFixture(brandId)
            .CreateMany<PlayerGameRebateAggregate>(1)
            .AsQueryable()
            .BuildMock();

        var playerBonusAggregates = CreatePlayerBonusAggregateFixture(brandId).CreateMany<PlayerBonusAggregate>(1)
            .AsQueryable()
            .BuildMock();

        var request = new GetVipReportRequest
        {
            BrandId = brandId,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1)
        };

        var playerDailyPaymentStatistics =
            CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                .BuildMock();

        var deposits = CreateDepositFormFixture(brandId).CreateMany<DepositForm>(5).AsQueryable()
            .BuildMock();

        var mockWithdrawForms = GetDbSet(withdrawForms);
        var mockPlayers = GetDbSet(players);
        var mockPlayerAggregates = GetDbSet(playerAggregates);
        var mockPlayerGameAggregates = GetDbSet(playerGameAggregates);
        var mockPlayerBonusAggregates = GetDbSet(playerBonusAggregates);
        var mockDepositForms = GetDbSet(deposits);
        var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);

        fakeDbContext.WithdrawForms.Returns(mockWithdrawForms.Object);
        fakeDbContext.Players.Returns(mockPlayers.Object);
        fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);
        fakeDbContext.PlayerGameRebateAggregates.Returns(mockPlayerGameAggregates.Object);
        fakeDbContext.PlayerBonusAggregates.Returns(mockPlayerBonusAggregates.Object);
        fakeDbContext.DepositForms.Returns(mockDepositForms.Object);
        fakeDbContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);

        var response = await sut.Handle(request, default);

        Assert.AreEqual(1, response.Items?.Length);
    }

    private static Fixture CreatePlayerFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.Player>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.PlayerCode, "PlayerCode")
            .With(s => s.Brand, new Brand())
            .With(s => s.Birthdate, DateOnly.Parse("2022/05/04")));
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

    private static Fixture CreatePlayerAggregateFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.PlayerCode, "PlayerCode")
            .With(s => s.ReportDate, DateTime.UtcNow)
            .With(s => s.CreatedOn, DateTime.UtcNow));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerGameRebateAggregateFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerGameRebateAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.PlayerCode, "PlayerCode")
            .With(s => s.ReportDate, DateTime.UtcNow));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerBonusAggregateFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerBonusAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.PlayerCode, "PlayerCode")
            .With(s => s.ReportDate, DateTime.UtcNow));
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

    private static Fixture CreatePlayerDailyPaymentStatisticsFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerDailyPaymentStatistic>(u => u
            .With(s => s.ReportDate,
                new DateOnly(DateTimeOffset.UtcNow.Year, DateTimeOffset.UtcNow.Month, DateTimeOffset.UtcNow.Day)));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}