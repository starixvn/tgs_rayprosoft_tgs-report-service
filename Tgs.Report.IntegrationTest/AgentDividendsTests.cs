using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Client.Enum;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.IntegrationTest;

[TestFixture]
[Parallelizable]
public class AgentDividendsTests
{
    [Test]
    public async Task Can_Get_Single_Agent_Dividends()
    {
        var brandId = Guid.NewGuid();
        var agentUsername = "username";

        var agentId = Guid.NewGuid();

        var agent1 = CreateAgentFixture(brandId, agentId, agentUsername)
            .Create<Agent>();

        var agents = new[]
        {
            agent1
        }.AsQueryable().BuildMock();

        var agentDividends = CreateAgentDividendFixture(brandId, agentId).CreateMany<AgentDividend>(1)
            .AsQueryable().BuildMock();

        var agentDividendMultipleLayers = agentDividends.SelectMany(x => x.AgentDividendMultipleLayers);

        var players = agents.SelectMany(x => x.Players);

        var playerDailyPaymentStatistics =
            CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable().BuildMock();

        var fees = agentDividends
            .Select(x => CreateAgentDividendProductFeeFixture(x.Id,10,100,GameType.Slot).Create<AgentDividendProductFee>())
            .AsQueryable()
            .BuildMock();

        var platformCharges = agentDividends
            .Select(x => CreateAgentDividendPlatformChargeFixture(x.Id).Create<AgentDividendPlatformCharge>())
            .AsQueryable()
            .BuildMock();

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                var fakeDbContext = Substitute.For<TgsContext>();
                var fakeAgentDbSet = Substitute.For<DbSet<Agent>, IQueryable<Agent>>();
                var fakeAgentDividendDbSet = Substitute.For<DbSet<AgentDividend>, IQueryable<AgentDividend>>();
                var fakePlayerDbSet = Substitute.For<DbSet<Player>, IQueryable<Player>>();
                var fakeAgentDividendMultipleLayerDbSet = Substitute.For<DbSet<AgentDividendMultipleLayer>, IQueryable<AgentDividendMultipleLayer>>();
                var fakePlayerDailyPaymentStatisticDbSet = Substitute.For<DbSet<PlayerDailyPaymentStatistic>, IQueryable<PlayerDailyPaymentStatistic>>();
                var fakeAgentDividendProductFeeDbSet = Substitute.For<DbSet<AgentDividendProductFee>, IQueryable<AgentDividendProductFee>>();
                var fakeAgentDividendPlatformChargeDbSet = Substitute.For<DbSet<AgentDividendPlatformCharge>, IQueryable<AgentDividendPlatformCharge>>();

                var fakeAgents = fakeAgentDbSet.Initialize(agents);
                var fakeAgentDividends = fakeAgentDividendDbSet.Initialize(agentDividends);
                var fakePlayers = fakePlayerDbSet.Initialize(players);
                var fakeAgentDividendMultipleLayers = fakeAgentDividendMultipleLayerDbSet.Initialize(agentDividendMultipleLayers);
                var fakePlayerDailyPaymentStatistics = fakePlayerDailyPaymentStatisticDbSet.Initialize(playerDailyPaymentStatistics);
                var fakeAgentDividendProductFees = fakeAgentDividendProductFeeDbSet.Initialize(fees);
                var fakeAgentDividendPlatformCharges = fakeAgentDividendPlatformChargeDbSet.Initialize(platformCharges);

                _ = fakeDbContext.Agents.Returns(fakeAgents);
                _ = fakeDbContext.AgentDividends.Returns(fakeAgentDividends);
                _ = fakeDbContext.Players.Returns(fakePlayers);
                _ = fakeDbContext.AgentDividendMultipleLayers.Returns(fakeAgentDividendMultipleLayers);
                _ = fakeDbContext.PlayerDailyPaymentStatistics.Returns(fakePlayerDailyPaymentStatistics);
                _ = fakeDbContext.AgentDividendProductFees.Returns(fakeAgentDividendProductFees);
                _ = fakeDbContext.AgentDividendPlatformCharges.Returns(fakeAgentDividendPlatformCharges);

                _ = services.AddSingleton(fakeDbContext);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetSingleAgentDividendsAsync(new()
        {
            BrandId = brandId,
            Username = agentUsername,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1),
            Page = 1,
            Rows = 20
        });

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Items, Has.Length.EqualTo(1));

        foreach (var agentDividendModel in actual.Items)
        {
            Assert.That(agentDividendModel.ProductFee, Is.EqualTo(fees.Sum(x => x.WinLossAmount * x.Percentage / 100)));
        }
    }

    [Test]
    public async Task Can_Get_Sub_Agent_Dividends()
    {
        var brandId = Guid.NewGuid();

        var agentId = Guid.NewGuid();
        var agentUsername = "username1";

        var childAgentId = Guid.NewGuid();
        var childAgentUsername = "username2";

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                var fakeDbContext = Substitute.For<TgsContext>();

                var agent1 = CreateAgentFixture(brandId, agentId, agentUsername)
                    .Create<Agent>();

                var agent2 = CreateAgentFixture(brandId, childAgentId, childAgentUsername)
                    .Create<Agent>();

                agent2.ParentAgentId = agentId;

                var agents = new[]
                    {
                        agent1,
                        agent2
                    }.AsQueryable()
                    .BuildMock();

                var agentDividends = CreateAgentDividendFixture(brandId, childAgentId).CreateMany<AgentDividend>(1)
                    .AsQueryable()
                    .BuildMock();

                var agentDividendMultipleLayers = agentDividends.SelectMany(x => x.AgentDividendMultipleLayers);

                var players = agents.SelectMany(x => x.Players);

                var playerDailyPaymentStatistics =
                    CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                        .BuildMock();

                var fees = agentDividends
                    .Select(x => CreateAgentDividendProductFeeFixture(x.Id, 10, 100, GameType.Slot).Create<AgentDividendProductFee>())
                    .AsQueryable()
                    .BuildMock();

                var platformCharges = agentDividends
                    .Select(x => CreateAgentDividendPlatformChargeFixture(x.Id).Create<AgentDividendPlatformCharge>())
                    .AsQueryable()
                    .BuildMock();

                var fakeAgentDbSet = Substitute.For<DbSet<Agent>, IQueryable<Agent>>();
                var fakeAgentDividendDbSet = Substitute.For<DbSet<AgentDividend>, IQueryable<AgentDividend>>();
                var fakePlayerDbSet = Substitute.For<DbSet<Player>, IQueryable<Player>>();
                var fakeAgentDividendMultipleLayerDbSet = Substitute.For<DbSet<AgentDividendMultipleLayer>, IQueryable<AgentDividendMultipleLayer>>();
                var fakePlayerDailyPaymentStatisticDbSet = Substitute.For<DbSet<PlayerDailyPaymentStatistic>, IQueryable<PlayerDailyPaymentStatistic>>();
                var fakeAgentDividendProductFeeDbSet = Substitute.For<DbSet<AgentDividendProductFee>, IQueryable<AgentDividendProductFee>>();
                var fakeAgentDividendPlatformChargeDbSet = Substitute.For<DbSet<AgentDividendPlatformCharge>, IQueryable<AgentDividendPlatformCharge>>();

                var fakeAgents = fakeAgentDbSet.Initialize(agents);
                var fakeAgentDividends = fakeAgentDividendDbSet.Initialize(agentDividends);
                var fakePlayers = fakePlayerDbSet.Initialize(players);
                var fakeAgentDividendMultipleLayers = fakeAgentDividendMultipleLayerDbSet.Initialize(agentDividendMultipleLayers);
                var fakePlayerDailyPaymentStatistics = fakePlayerDailyPaymentStatisticDbSet.Initialize(playerDailyPaymentStatistics);
                var fakeAgentDividendProductFees = fakeAgentDividendProductFeeDbSet.Initialize(fees);
                var fakeAgentDividendPlatformCharges = fakeAgentDividendPlatformChargeDbSet.Initialize(platformCharges);

                _ = fakeDbContext.Agents.Returns(fakeAgents);
                _ = fakeDbContext.AgentDividends.Returns(fakeAgentDividends);
                _ = fakeDbContext.Players.Returns(fakePlayers);
                _ = fakeDbContext.AgentDividendMultipleLayers.Returns(fakeAgentDividendMultipleLayers);
                _ = fakeDbContext.PlayerDailyPaymentStatistics.Returns(fakePlayerDailyPaymentStatistics);
                _ = fakeDbContext.AgentDividendProductFees.Returns(fakeAgentDividendProductFees);
                _ = fakeDbContext.AgentDividendPlatformCharges.Returns(fakeAgentDividendPlatformCharges);

                _ = services.AddSingleton(fakeDbContext);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetSubAgentDividendsAsync(new()
        {
            BrandId = brandId,
            Username = agentUsername,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1),
            Page = 1,
            Rows = 20
        });

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Items, Has.Length.EqualTo(1));
        Assert.That(actual.Items[0].Username, Is.EqualTo(childAgentUsername));
    }

    [Test]
    public async Task Can_Get_Agent_Dividends()
    {
        var brandId = Guid.NewGuid();

        var agentId = Guid.NewGuid();
        var agentUsername = "username1";

        var childAgentId = Guid.NewGuid();
        var childAgentUsername = "username2";

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                var fakeDbContext = Substitute.For<TgsContext>();

                var agent1 = CreateAgentFixture(brandId, agentId, agentUsername)
                    .Create<Agent>();

                var agent2 = CreateAgentFixture(brandId, childAgentId, childAgentUsername)
                    .Create<Agent>();

                agent2.ParentAgentId = agentId;

                var agents = new[]
                    {
                        agent1,
                        agent2
                    }.AsQueryable()
                    .BuildMock();

                var agentDividends = new[]
                    {
                        CreateAgentDividendFixture(brandId, agentId).Create<AgentDividend>(),
                        CreateAgentDividendFixture(brandId, childAgentId).Create<AgentDividend>()
                    }
                    .AsQueryable()
                    .BuildMock();

                var agentDividendMultipleLayers = agentDividends.SelectMany(x => x.AgentDividendMultipleLayers);

                var players = agents.SelectMany(x => x.Players);

                var playerDailyPaymentStatistics =
                    CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                        .BuildMock();

                var fees = agentDividends
                    .Select(x => CreateAgentDividendProductFeeFixture(x.Id, 10, 100, GameType.Slot).Create<AgentDividendProductFee>())
                    .AsQueryable()
                    .BuildMock();

                var platformCharges = agentDividends
                    .Select(x => CreateAgentDividendPlatformChargeFixture(x.Id).Create<AgentDividendPlatformCharge>())
                    .AsQueryable()
                    .BuildMock();

                var fakeAgentDbSet = Substitute.For<DbSet<Agent>, IQueryable<Agent>>();
                var fakeAgentDividendDbSet = Substitute.For<DbSet<AgentDividend>, IQueryable<AgentDividend>>();
                var fakePlayerDbSet = Substitute.For<DbSet<Player>, IQueryable<Player>>();
                var fakeAgentDividendMultipleLayerDbSet = Substitute.For<DbSet<AgentDividendMultipleLayer>, IQueryable<AgentDividendMultipleLayer>>();
                var fakePlayerDailyPaymentStatisticDbSet = Substitute.For<DbSet<PlayerDailyPaymentStatistic>, IQueryable<PlayerDailyPaymentStatistic>>();
                var fakeAgentDividendProductFeeDbSet = Substitute.For<DbSet<AgentDividendProductFee>, IQueryable<AgentDividendProductFee>>();
                var fakeAgentDividendPlatformChargeDbSet = Substitute.For<DbSet<AgentDividendPlatformCharge>, IQueryable<AgentDividendPlatformCharge>>();

                var fakeAgents = fakeAgentDbSet.Initialize(agents);
                var fakeAgentDividends = fakeAgentDividendDbSet.Initialize(agentDividends);
                var fakePlayers = fakePlayerDbSet.Initialize(players);
                var fakeAgentDividendMultipleLayers = fakeAgentDividendMultipleLayerDbSet.Initialize(agentDividendMultipleLayers);
                var fakePlayerDailyPaymentStatistics = fakePlayerDailyPaymentStatisticDbSet.Initialize(playerDailyPaymentStatistics);
                var fakeAgentDividendProductFees = fakeAgentDividendProductFeeDbSet.Initialize(fees);
                var fakeAgentDividendPlatformCharges = fakeAgentDividendPlatformChargeDbSet.Initialize(platformCharges);

                _ = fakeDbContext.Agents.Returns(fakeAgents);
                _ = fakeDbContext.AgentDividends.Returns(fakeAgentDividends);
                _ = fakeDbContext.Players.Returns(fakePlayers);
                _ = fakeDbContext.AgentDividendMultipleLayers.Returns(fakeAgentDividendMultipleLayers);
                _ = fakeDbContext.PlayerDailyPaymentStatistics.Returns(fakePlayerDailyPaymentStatistics);
                _ = fakeDbContext.AgentDividendProductFees.Returns(fakeAgentDividendProductFees);
                _ = fakeDbContext.AgentDividendPlatformCharges.Returns(fakeAgentDividendPlatformCharges);

                _ = services.AddSingleton(fakeDbContext);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetAgentDividendsAsync(new()
        {
            BrandId = brandId,
            Username = agentUsername,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1),
            Page = 1,
            Rows = 20
        });

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual.Items, Has.Length.EqualTo(2));
    }

    private static Fixture CreateAgentFixture(Guid brandId, Guid agentId, string username)
    {
        var fixture = new Fixture();
        fixture.Customize<Agent>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Id, agentId)
            .With(s => s.Username, username)
            .With(s => s.Brand, new Brand())
            .With(s => s.Players, new Player[]
            {
            }));
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

    private static Fixture CreateAgentDividendFixture(Guid brandId, Guid agentId)
    {
        var fixture = new Fixture();
        fixture.Customize<AgentDividend>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.AgentId, agentId)
            .With(s => s.From, DateTime.UtcNow)
            .With(s => s.To, DateTime.UtcNow)
            .With(s => s.AgentDividendMultipleLayers, new AgentDividendMultipleLayer[]
            {
            }));
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

    private static Fixture CreateAgentDividendProductFeeFixture(int agentDividendId, decimal percentage = 10,
        decimal winLoss = 100, GameType gameType = GameType.Slot)
    {
        var fixture = new Fixture();
        fixture.Customize<AgentDividendProductFee>(u => u
            .With(s => s.AgentDividendId, agentDividendId)
            .With(s => s.GameType, (short)gameType)
            .With(s => s.Percentage, percentage)
            .With(s => s.WinLossAmount, winLoss));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateAgentDividendPlatformChargeFixture(int agentDividendId)
    {
        var fixture = new Fixture();
        fixture.Customize<AgentDividendPlatformCharge>(u => u
            .With(s => s.AgentDividendId, agentDividendId));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}