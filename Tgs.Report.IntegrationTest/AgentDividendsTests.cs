using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Agent;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.IntegrationTest;

[TestClass]
public class AgentDividendsTests : BaseTest
{
    [TestMethod]
    public async Task Can_Get_Single_Agent_Dividends()
    {
        var brandId = Guid.NewGuid();
        var agentUsername = "username";

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                var fakeDbContext = Substitute.For<TgsContext>();

                var agentId = Guid.NewGuid();

                var agent1 = CreateAgentFixture(brandId, agentId, agentUsername)
                    .Create<Agent>();

                var agents = new Agent[]
                    {
                        agent1
                    }.AsQueryable()
                    .BuildMock();

                var agentDividends = CreateAgentDividendFixture(brandId, agentId).CreateMany<AgentDividend>(1)
                    .AsQueryable()
                    .BuildMock();

                var agentDividendMultipleLayers = agentDividends.SelectMany(x => x.AgentDividendMultipleLayers);

                var players = agents.SelectMany(x => x.Players);

                var playerDailyPaymentStatistics =
                    CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                        .BuildMock();

                var mockAgents = GetDbSet(agents);
                var mockAgentDividends = GetDbSet(agentDividends);
                var mockPlayers = GetDbSet(players);
                var mockAgentDividendMultipleLayers = GetDbSet(agentDividendMultipleLayers);
                var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);

                fakeDbContext.Agents.Returns(mockAgents.Object);
                fakeDbContext.AgentDividends.Returns(mockAgentDividends.Object);
                fakeDbContext.Players.Returns(mockPlayers.Object);
                fakeDbContext.AgentDividendMultipleLayers.Returns(mockAgentDividendMultipleLayers.Object);
                fakeDbContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);

                _ = services.AddSingleton(fakeDbContext);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetSingleAgentDividendsAsync(new GetSingleAgentDividendsRequest
        {
            BrandId = brandId,
            Username = agentUsername,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1),
            Page = 1,
            Rows = 20
        });

        Assert.IsNotNull(actual);
        Assert.IsTrue(actual.Items.Length == 1);
    }

    [TestMethod]
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

                var agents = new Agent[]
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

                var mockAgents = GetDbSet(agents);
                var mockAgentDividends = GetDbSet(agentDividends);
                var mockPlayers = GetDbSet(players);
                var mockAgentDividendMultipleLayers = GetDbSet(agentDividendMultipleLayers);
                var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);

                fakeDbContext.Agents.Returns(mockAgents.Object);
                fakeDbContext.AgentDividends.Returns(mockAgentDividends.Object);
                fakeDbContext.Players.Returns(mockPlayers.Object);
                fakeDbContext.AgentDividendMultipleLayers.Returns(mockAgentDividendMultipleLayers.Object);
                fakeDbContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);

                _ = services.AddSingleton(fakeDbContext);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetSubAgentDividendsAsync(new GetSubAgentDividendsRequest
        {
            BrandId = brandId,
            Username = agentUsername,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1),
            Page = 1,
            Rows = 20
        });

        Assert.IsNotNull(actual);
        Assert.IsTrue(actual.Items.Length == 1);
        Assert.IsTrue(actual.Items[0].Username == childAgentUsername);
    }

    [TestMethod]
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

                var agents = new Agent[]
                    {
                        agent1,
                        agent2
                    }.AsQueryable()
                    .BuildMock();

                var agentDividends = new AgentDividend[]
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

                var mockAgents = GetDbSet(agents);
                var mockAgentDividends = GetDbSet(agentDividends);
                var mockPlayers = GetDbSet(players);
                var mockAgentDividendMultipleLayers = GetDbSet(agentDividendMultipleLayers);
                var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);

                fakeDbContext.Agents.Returns(mockAgents.Object);
                fakeDbContext.AgentDividends.Returns(mockAgentDividends.Object);
                fakeDbContext.Players.Returns(mockPlayers.Object);
                fakeDbContext.AgentDividendMultipleLayers.Returns(mockAgentDividendMultipleLayers.Object);
                fakeDbContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);

                _ = services.AddSingleton(fakeDbContext);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetAgentDividendsAsync(new GetAgentDividendsRequest
        {
            BrandId = brandId,
            Username = agentUsername,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1),
            Page = 1,
            Rows = 20
        });

        Assert.IsNotNull(actual);
        Assert.IsTrue(actual.Items.Length == 2);
    }

    private static Fixture CreateAgentFixture(Guid brandId, Guid agentId, string username)
    {
        var fixture = new Fixture();
        fixture.Customize<Agent>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Id, agentId)
            .With(s => s.Username, username)
            .With(s => s.Brand, new Brand())
            .With(s => s.Players, new Repository.Entities.Player[]
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
}
