using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.AgentWinLoss;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.UnitTest.AgentWinLoss;

[TestFixture]
public sealed class GetPlayerWinLossQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Player_Win_Loss()
    {
        var fakeDbContext = Substitute.For<TgsContext>();
        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new(8, 0, 0));

        var playerAggregates = CreatePlayerAggregateFixture(brandId, reportDate, "player")
            .CreateMany<PlayerAggregate>(1)
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
            To = reportDate.AddDays(1)
        };

        var response = await sut.Handle(request, default);

        Assert.IsTrue(response.Valid);
        Assert.AreEqual(1, response.Items?.Length);
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