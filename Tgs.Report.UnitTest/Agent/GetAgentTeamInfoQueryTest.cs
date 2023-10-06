using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.UnitTest.Agent;

[TestFixture]
[Parallelizable]
public sealed class GetAgentTeamInfoQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Agent_Team_Info()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var sut = new GetAgentTeamInfoQueryHandler(fakeDbContext);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var agentId = new Guid("aac22220-20f6-4f0d-6982-08d9dbdc0473");

        var players = CreatePlayerFixture(brandId, agentId).CreateMany<Repository.Entities.Player>(1)
            .AsQueryable()
            .BuildMock();

        var playerAggregates = CreatePlayerAggregateFixture(brandId, agentId).CreateMany<PlayerAggregate>(1)
            .AsQueryable()
            .BuildMock();

        var request = new GetAgentTeamInfoRequest
        {
            AgentId = agentId,
            SortBy = null
        };

        var mockPlayers = GetDbSet(players);
        var mockPlayerAggregates = GetDbSet(playerAggregates);

        fakeDbContext.Players.Returns(mockPlayers.Object);
        fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);

        var response = await sut.Handle(request, default);

        Assert.IsTrue(response.Valid);
        Assert.AreEqual(1, response.Items?.Length);
    }

    private static Fixture CreatePlayerAggregateFixture(Guid brandId, Guid agentId)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.AgentId, agentId)
            .With(s => s.PlayerCode, "playerCode"));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerFixture(Guid brandId, Guid agentId)
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.Player>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.AgentId, agentId)
            .With(s => s.PlayerCode, "playerCode")
            .With(s => s.RegisteredOn, DateTime.UtcNow)
            .With(s => s.Birthdate, DateOnly.Parse("2022/05/04")));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}