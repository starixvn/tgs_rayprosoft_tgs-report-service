using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Market;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.UnitTest.Market;

[TestFixture]
public sealed class GetMarketAgentProfitsQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Market_Agent_Profits()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var sut = new GetMarketAgentProfitQueryHandler(fakeDbContext);

        var brandId = Guid.NewGuid();
        var playerCode = "playerCode";
        var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new(8, 0, 0));

        var agent = CreateAgentFixture(brandId).Create<Repository.Entities.Agent>();

        var playerAggregates = CreatePlayerAggregateFixture(brandId, agent.Id, playerCode, 3, reportDate).CreateMany<PlayerAggregate>(1)
            .AsQueryable()
            .BuildMock();

        var playerBonusAggregates = CreatePlayerBonusAggregateFixture(brandId, agent.Id, playerCode, 3, reportDate)
            .CreateMany<PlayerBonusAggregate>(1).AsQueryable()
            .BuildMock();

        var request = new GetMarketAgentProfitsRequest
        {
            BrandId = brandId,
            Date = reportDate
        };

        var mockPlayerAggregates = GetDbSet(playerAggregates);
        var mockPlayerBonusAggregates = GetDbSet(playerBonusAggregates);
        var mockAgents = GetDbSet(new[]{ agent }.AsQueryable().BuildMock());

        fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);
        fakeDbContext.PlayerBonusAggregates.Returns(mockPlayerBonusAggregates.Object);
        fakeDbContext.Agents.Returns(mockAgents.Object);

        var response = await sut.Handle(request, default);

        Assert.AreEqual(1,
            response.Items?.Count());
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
            .With(s => s.Players,new Repository.Entities.Player[]{}));

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