using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.UnitTest.Agent;

[TestFixture]
public sealed class GetAgentWinLossQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Agent_Win_Loss()
    {
        var fakeAgentFactory = Substitute.For<IAgentFactory>();
        var agentWinLosses = CreateAgentWinLossFixture().CreateMany<AgentWinLossDataModel>(10);

        fakeAgentFactory.GetAgentWinLossAsync(Arg.Any<GetAgentWinLossDataRequest>()).Returns(agentWinLosses);

        var sut = new GetAgentWinLossQueryHandler(fakeAgentFactory);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var agentId = new Guid("aac22220-20f6-4f0d-6982-08d9dbdc0473");

        var request = new GetAgentWinLossRequest
        {
            BrandId = brandId,
            AgentId = agentId,
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now,
        };

        var response = await sut.Handle(request, default);

        Assert.IsTrue(response.Valid);
        Assert.AreEqual(10, response.Items?.Length);
    }
    [Test]
    public async Task Can_Get_Master_Agent_Win_Loss()
    {
        var fakeAgentFactory = Substitute.For<IAgentFactory>();
        var agentWinLosses = CreateAgentWinLossFixture().CreateMany<AgentWinLossDataModel>(10);

        fakeAgentFactory.GetMasterAgentWinLossAsync(Arg.Any<GetMasterAgentWinLossDataRequest>()).Returns(agentWinLosses);

        var sut = new GetMasterAgentWinLossQueryHandler(fakeAgentFactory);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");

        var request = new GetMasterAgentWinLossRequest
        {
            BrandId = brandId,
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now,
        };

        var response = await sut.Handle(request, default);

        Assert.IsTrue(response.Valid);
        Assert.AreEqual(10, response.Items?.Length);
    }

    [Test]
    public async Task Can_Get_DownLine_Agent_Win_Loss()
    {
        var fakeAgentFactory = Substitute.For<IAgentFactory>();
        var agentWinLosses = CreateAgentWinLossFixture().CreateMany<AgentWinLossDataModel>(10);

        fakeAgentFactory.GetDownLineAgentWinLossAsync(Arg.Any<GetDownLineAgentWinLossDataRequest>()).Returns(agentWinLosses);

        var sut = new GetDownLineAgentWinLossQueryHandler(fakeAgentFactory);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var agentId = new Guid("aac22220-20f6-4f0d-6982-08d9dbdc0473");

        var request = new GetDownLineAgentWinLossRequest
        {
            BrandId = brandId,
            AgentId = agentId,
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now,
        };

        var response = await sut.Handle(request, default);

        Assert.IsTrue(response.Valid);
        Assert.AreEqual(10, response.Items?.Length);
    }

    [Test]
    public async Task Can_Get_Player_Win_Loss()
    {
        var fakeDbContext = Substitute.For<TgsContext>();
        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new(8, 0, 0));

        var playerAggregates = CreatePlayerAggregateFixture(brandId, reportDate, "player").CreateMany<PlayerAggregate>(1)
            .AsQueryable()
            .BuildMock();

        var players = CreatePlayerFixture(brandId, "player").CreateMany<Repository.Entities.Player>(1)
            .AsQueryable()
            .BuildMock();

        var mockPlayerAggregates = GetDbSet(playerAggregates);
        var mockPlayers = GetDbSet(players);

        fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);
        fakeDbContext.Players.Returns(mockPlayers.Object);

        var sut = new GetPlayerWinLossQueryHandler(fakeDbContext);

        var request = new GetPlayerWinLossRequest
        {
            PlayerCode = "player",
            From = reportDate,
            To = reportDate.AddDays(1),
        };

        var response = await sut.Handle(request, default);

        Assert.IsTrue(response.Valid);
        Assert.AreEqual(1, response.Items?.Length);
    }


    private static Fixture CreateAgentWinLossFixture()
    {
        var fixture = new Fixture();
        fixture.Build<AgentWinLossDataModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
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

}