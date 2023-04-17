using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.UnitTest.Agent;

[TestFixture]
public sealed class GetAgentContributionsQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Agent_Contributions()
    {
        var fakeAgentRepository = Substitute.For<IAgentRepository>();
        var fakeWithdrawForm = Substitute.For<ITgsService<WithdrawForm>>();
        var fakePlayer = Substitute.For<ITgsService<Repository.Entities.Player>>();
        var fakePlayerAggregate = Substitute.For<ITgsService<PlayerAggregate>>();
        var fakePlayerBonusAggregate = Substitute.For<ITgsService<PlayerBonusAggregate>>();
        var fakeContext = Substitute.For<TgsContext>();

        var sut = new GetAgentContributionsQueryHandler(fakeAgentRepository, fakeWithdrawForm, fakePlayer,
            fakePlayerAggregate, fakePlayerBonusAggregate, fakeContext);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var agentUsername = "agentUsername";
        var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new(8, 0, 0));

        var agents = CreateAgentFixture(brandId, agentUsername)
            .CreateMany<Repository.Entities.Agent>(1)
            .AsQueryable()
            .BuildMock();

        var withdrawForms = CreateWithdrawFormFixture(brandId)
            .CreateMany<WithdrawForm>(1).AsQueryable()
            .BuildMock();

        var players = agents.SelectMany(x => x.Players);

        var playerAggregates =
            CreatePlayerAggregateFixture(brandId, agents.First().Id, players.First().PlayerCode, reportDate)
                .CreateMany<PlayerAggregate>(1).AsQueryable()
                .BuildMock();

        var playerBonusAggregates =
            CreatePlayerBonusAggregateFixture(brandId, agents.First().Id, players.First().PlayerCode, reportDate)
                .CreateMany<PlayerBonusAggregate>(1).AsQueryable()
                .BuildMock();

        var request = new GetAgentContributionsRequest
        {
            BrandId = brandId,
            AgentId = null,
            Username = agentUsername,
            SortBy = null,
            From = reportDate,
            To = reportDate.AddDays(1)
        };

        var playerDailyPaymentStatistics =
            CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                .BuildMock();

        var agentDailyPaymentStatistics =
            CreateAgentDailyPaymentStatisticsFixture().CreateMany<AgentDailyPaymentStatistic>(1).AsQueryable()
                .BuildMock();

        var mockAgentDailyPaymentStatistics = GetDbSet(agentDailyPaymentStatistics);
        var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);
        var mockPlayers = GetDbSet(players);

        fakeContext.AgentDailyPaymentStatistics.Returns(mockAgentDailyPaymentStatistics.Object);
        fakeContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);
        fakeContext.Players.Returns(mockPlayers.Object);

        fakeAgentRepository.GetAgentWithNLevelAsync(
                Arg.Is(brandId),
                Arg.Is((Guid?)null),
                Arg.Is(agentUsername),
                Arg.Any<CancellationToken>())
            .Returns(x =>
            {
                var agent = agents.First();

                return new(agent.Id, "", 1);
            });

        fakeWithdrawForm.GetQueries().Returns(withdrawForms);
        fakePlayer.GetQueries().Returns(players);
        fakePlayerAggregate.GetQueries().Returns(playerAggregates);
        fakePlayerBonusAggregate.GetQueries().Returns(playerBonusAggregates);

        var response = await sut.Handle(request, default);

        Assert.IsTrue(response.Valid);
        Assert.AreEqual(players.Count(), response.Items?.Length);
    }

    private static Fixture CreateAgentFixture(Guid brandId, string username)
    {
        var agentId = Guid.NewGuid();
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.Agent>(u => u
            .With(s => s.Id, agentId)
            .With(s => s.BrandId, brandId)
            .With(s => s.Username, username)
            .With(s => s.Brand, new Brand())
            .With(s => s.Players, new[]
            {
                new Repository.Entities.Player
                {
                    PlayerCode = "playerCode",
                    AgentId = agentId,
                    BrandId = brandId
                }
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

    private static Fixture CreatePlayerAggregateFixture(Guid brandId, Guid agentId, string playerCode,
        DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.AgentId, agentId)
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.ReportDate, reportDate)
            .With(s => s.CreatedOn, reportDate));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerBonusAggregateFixture(Guid brandId, Guid agentId, string playerCode,
        DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerBonusAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.AgentId, agentId)
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.ReportDate, reportDate));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerDailyPaymentStatisticsFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerDailyPaymentStatistic>(u => u
            .With(s => s.ReportDate,
                DateOnly.FromDateTime(DateTimeOffset.UtcNow.DateTime)));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateAgentDailyPaymentStatisticsFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<AgentDailyPaymentStatistic>(u => u
            .With(s => s.ReportDate,
                DateOnly.FromDateTime(DateTimeOffset.UtcNow.AddDays(-1).DateTime)));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}