using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Core;
using Tgs.Report.Core.Models;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Player;
using GetPlayerAllAgentRebatesRequest = Tgs.Report.Client.Models.Player.GetPlayerAllAgentRebatesRequest;
using GetPlayerAllAgentRebateTotalAmountRequest =
    Tgs.Report.Client.Models.Player.GetPlayerAllAgentRebateTotalAmountRequest;

namespace Tgs.Report.IntegrationTest;

[TestFixture]
[Parallelizable]
public class PlayerTests
{
    [Test]
    public async Task Can_Get_Player_All_Agent_Rebates()
    {
        var fakePlayerFactory = Substitute.For<IPlayerFactory>();

        const string username = "";
        var brandId = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

        var fixture = new Fixture();

        var playerAllAgentRebates = fixture.CreateMany<PlayerAllAgentRebateModel>(20);

        _ = fakePlayerFactory.GetPlayerAllAgentRebatesQueryable(Arg.Any<GetPlayerAllAgentRebatesQuery>())
            .Returns(playerAllAgentRebates.AsQueryable());

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                _ = services.AddSingleton(fakePlayerFactory);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportPlayerAllAgentClient(httpClient);

        var actual = await sut.GetPlayerAllAgentRebatesAsync(new GetPlayerAllAgentRebatesRequest
            {
                BrandId = brandId,
                Username = username,
                From = createdOn.UtcDateTime,
                To = createdOn.AddDays(1).UtcDateTime,
                Page = 1,
                Rows = 10
            })
            .ToArrayAsync();

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Has.Length.EqualTo(10));
    }

    [Test]
    public async Task Can_Get_Player_All_Agent_Rebates_By_Filter()
    {
        var fakeTgsContext = Substitute.For<TgsContext>();

        var brandId = Guid.NewGuid();
        var username = "";
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

        var playerAllAgentRebates = CreatePlayerAllAgentRebateContributorFixture(brandId, createdOn)
            .CreateMany<PlayerAllAgentRebateContributor>(10).ToArray();

        var players = playerAllAgentRebates.Select(x => x.PlayerCode)
            .Distinct()
            .Select(playerCode => CreatePlayerFixture(playerCode, playerCode).Create<Player>());

        var fakePlayerAllAgentRebateContributorDbSet = Substitute
            .For<DbSet<PlayerAllAgentRebateContributor>, IQueryable<PlayerAllAgentRebateContributor>>();
        var fakePlayerDbSet = Substitute.For<DbSet<Player>, IQueryable<Player>>();

        var fakePlayerAllAgentRebateContributors =
            fakePlayerAllAgentRebateContributorDbSet.Initialize(playerAllAgentRebates.AsQueryable().BuildMock());
        var fakePlayers = fakePlayerDbSet.Initialize(players.AsQueryable().BuildMock());

        _ = fakeTgsContext.PlayerAllAgentRebateContributors.Returns(fakePlayerAllAgentRebateContributors);
        _ = fakeTgsContext.Players.Returns(fakePlayers);

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                _ = services.AddSingleton(fakeTgsContext);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportPlayerAllAgentClient(httpClient);

        var actual = await sut.GetPlayerAllAgentRebatesAsync(new GetPlayerAllAgentRebatesRequest
            {
                BrandId = brandId,
                Username = username,
                From = createdOn.UtcDateTime,
                To = createdOn.AddDays(1).UtcDateTime,
                Page = 1,
                Rows = 10
            })
            .ToArrayAsync();

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Has.Length.EqualTo(10));
    }

    [Test]
    public async Task Can_Get_Player_All_Agent_Rebates_Total_Count()
    {
        var fakePlayerFactory = Substitute.For<IPlayerFactory>();

        const string username = "";
        var brandId = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

        var fixture = new Fixture();

        var playerAllAgentRebates = fixture.CreateMany<PlayerAllAgentRebateModel>(20).ToArray();

        _ = fakePlayerFactory.GetPlayerAllAgentRebatesQueryable(Arg.Any<GetPlayerAllAgentRebatesQuery>())
            .Returns(playerAllAgentRebates.AsQueryable().BuildMock());

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                _ = services.AddSingleton(fakePlayerFactory);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportPlayerAllAgentClient(httpClient);

        var actual = await sut.GetPlayerAllAgentRebatesTotalAmountAsync(new GetPlayerAllAgentRebateTotalAmountRequest
        {
            BrandId = brandId,
            Username = username,
            From = createdOn.UtcDateTime,
            To = createdOn.AddDays(1).UtcDateTime
        });
        Assert.Multiple(() =>
        {
            Assert.That(actual.TotalCount, Is.EqualTo(playerAllAgentRebates.Length));
            Assert.That(actual.ValidBet, Is.EqualTo(playerAllAgentRebates.Sum(s => s.ValidBet)));
            Assert.That(actual.Rebate, Is.EqualTo(playerAllAgentRebates.Sum(s => s.Rebate)));
        });
    }

    private static Fixture CreatePlayerAllAgentRebateContributorFixture(Guid brandId, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();

        fixture.Customize<PlayerAllAgentRebateContributor>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.ReportDate, reportDate));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());

        return fixture;
    }

    private static Fixture CreatePlayerFixture(string playerCode, string username)
    {
        var fixture = new Fixture();
        fixture.Customize<Player>(u => u
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.Username, username)
            .With(s => s.Birthdate, DateOnly.MinValue));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}