using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.IntegrationTest;

[TestClass]
public class MarketAgentProfitsTests : BaseTest
{
    [TestMethod]
    public async Task Can_Get_Market_Agent_Profits()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "playerCode";
        var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new(8, 0, 0));

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                var fakeDbContext = Substitute.For<TgsContext>();

                var agent = CreateAgentFixture(brandId).Create<Repository.Entities.Agent>();

                var playerAggregates = CreatePlayerAggregateFixture(brandId, agent.Id, playerCode, 3, reportDate).CreateMany<PlayerAggregate>(1)
                    .AsQueryable()
                    .BuildMock();

                var playerBonusAggregates = CreatePlayerBonusAggregateFixture(brandId, agent.Id, playerCode, 3, reportDate)
                    .CreateMany<PlayerBonusAggregate>(1).AsQueryable()
                    .BuildMock();

                var mockPlayerAggregates = GetDbSet(playerAggregates);
                var mockPlayerBonusAggregates = GetDbSet(playerBonusAggregates);
                var mockAgents = GetDbSet(new[] { agent }.AsQueryable().BuildMock());

                fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);
                fakeDbContext.PlayerBonusAggregates.Returns(mockPlayerBonusAggregates.Object);
                fakeDbContext.Agents.Returns(mockAgents.Object);

                _ = services.AddSingleton(fakeDbContext);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetMarketAgentProfitsAsync(new()
        {
            BrandId = brandId,
            Date = reportDate
        });

        Assert.IsNotNull(actual);
        Assert.IsTrue(actual.Items.Length == 1);
    }

    private static Fixture CreatePlayerAggregateFixture(Guid brandId, Guid agentId, string playerCode, int hour, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.AgentId, agentId)
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.Hour, hour)
            .With(s => s.AgentDividendPercentageTreeData, new AgentDividendPercentage[]{ new AgentDividendPercentage
            {
                AgentId = agentId,
                DividendPercentage = 10M
            }})
            .With(s => s.ReportDate, reportDate)
            .With(s => s.CreatedOn, reportDate));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateAgentFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.Agent>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Brand, new Brand())
            .With(s => s.Players, new Repository.Entities.Player[] { }));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerBonusAggregateFixture(Guid brandId, Guid agentId, string playerCode, int hour, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerBonusAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.AgentId, agentId)
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.Hour, hour)
            .With(s => s.ReportDate, reportDate));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}