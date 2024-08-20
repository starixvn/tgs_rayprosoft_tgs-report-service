using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.Player;
using Tgs.Report.Core;
using Tgs.Report.Core.Models;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using PlayerAllAgentRebateDetailModel = Tgs.Report.ViewModel.Player.PlayerAllAgentRebateDetailModel;
using PlayerAllAgentRebateModel = Tgs.Report.ViewModel.Player.PlayerAllAgentRebateModel;

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

        foreach (var playerAllAgentRebateModel in playerAllAgentRebates)
        {
            playerAllAgentRebateModel.SentOn = null;
        }

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

        var playerAllAgentRebates = CreatePlayerAllAgentRebateContributorFixture(brandId, createdOn, 10.0032M)
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

        foreach (var playerAllAgentRebateModel in actual)
        {
            var playerRebate = playerAllAgentRebates.Where(x => x.PlayerCode == playerAllAgentRebateModel.PlayerCode)
                .Sum(s => s.SportsTurnoverRebate +
                          s.VirtualSportsTurnoverRebate +
                          s.LiveDealerTurnoverRebate +
                          s.CardGameTurnoverRebate +
                          s.SlotTurnoverRebate +
                          s.LotteryTurnoverRebate).Truncate(2);

            Assert.That(playerAllAgentRebateModel.Rebate, Is.EqualTo(playerRebate));
        }
    }

    [Test]
    public async Task Can_Get_Player_All_Agent_Rebates_By_Player_Code_Filter()
    {
        var fakeTgsContext = Substitute.For<TgsContext>();

        var brandId = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

        var playerAllAgentRebates = CreatePlayerAllAgentRebateContributorFixture(brandId, createdOn, 10.0032M)
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
            PlayerCode = players.First().PlayerCode,
            From = createdOn.UtcDateTime,
            To = createdOn.AddDays(1).UtcDateTime,
            Page = 1,
            Rows = 10
        })
            .ToArrayAsync();

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Has.Length.EqualTo(1));

        foreach (var playerAllAgentRebateModel in actual)
        {
            var playerRebate = playerAllAgentRebates.Where(x => x.PlayerCode == playerAllAgentRebateModel.PlayerCode)
                .Sum(s => s.SportsTurnoverRebate +
                          s.VirtualSportsTurnoverRebate +
                          s.LiveDealerTurnoverRebate +
                          s.CardGameTurnoverRebate +
                          s.SlotTurnoverRebate +
                          s.LotteryTurnoverRebate).Truncate(2);

            Assert.That(playerAllAgentRebateModel.Rebate, Is.EqualTo(playerRebate));
        }
    }
    
    [Test]
    public async Task Can_Get_Player_All_Agent_Rebate_Detail_Game_Types()
    {
        var fakePlayerFactory = Substitute.For<IPlayerFactory>();

        var brandId = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

        var fixture = new Fixture();

        var playerAllAgentRebateDetailGameTypes = fixture.CreateMany<ViewModel.Player.PlayerAllAgentRebateDetailGameTypeModel>(6).ToArray();

        _ = fakePlayerFactory.GetPlayerAllAgentRebateDetailGameTypesQueryable(Arg.Any<GetPlayerAllAgentRebateDetailGameTypesQuery>())
            .Returns(playerAllAgentRebateDetailGameTypes.AsQueryable());

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                _ = services.AddSingleton(fakePlayerFactory);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportPlayerAllAgentClient(httpClient);

        var actual = await sut.GetPlayerAllAgentRebateDetailGameTypesAsync(new GetPlayerAllAgentRebateDetailGameTypesRequest
            {
                BrandId = brandId,
                PlayerCode = "playerCode",
                SettleDateTime = createdOn.UtcDateTime
            })
            .ToArrayAsync();

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Has.Length.EqualTo(6));
    }


    [Test]
    public async Task Can_Get_Player_All_Agent_Rebate_Detail()
    {
        var fakePlayerFactory = Substitute.For<IPlayerFactory>();

        var brandId = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

        var fixture = new Fixture();

        var playerAllAgentRebateDetails = fixture.CreateMany<PlayerAllAgentRebateDetailModel>(20).ToArray();

        _ = fakePlayerFactory.GetPlayerAllAgentRebateDetailsQueryable(Arg.Any<GetPlayerAllAgentRebatesDetailQuery>())
            .Returns(playerAllAgentRebateDetails.AsQueryable());

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                _ = services.AddSingleton(fakePlayerFactory);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportPlayerAllAgentClient(httpClient);

        var actual = await sut.GetPlayerAllAgentRebateDetailAsync(new GetPlayerAllAgentRebatesDetailsRequest
            {
                BrandId = brandId,
                PlayerCode = "playerCode",
                SettleDateTime = createdOn.UtcDateTime
            })
            .ToArrayAsync();

        Assert.That(actual, Is.Not.Null);
        Assert.That(actual, Has.Length.EqualTo(20));
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

    [Test]
    public async Task Can_Get_Players_Account_Changes_Total_Count()
    {
        var fakePlayerFactory = Substitute.For<IPlayerFactory>();

        const string username = "";
        var brandId = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

        var fixture = new Fixture();

        var playerAccountChanges = fixture.CreateMany<Tgs.Report.ViewModel.Player.PlayersAccountChangeModel>(20).ToArray();

        _ = fakePlayerFactory.GetPlayersAccountChangesQueryable(Arg.Any<GetPlayersAccountChangesQuery>())
            .Returns(playerAccountChanges.AsQueryable().BuildMock());

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                _ = services.AddSingleton(fakePlayerFactory);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetMultiplePlayersAccountChangesTotalAmountAsync(new GetPlayersAccountChangesTotalAmountRequest
        {
            BrandId = brandId,
            Username = username,
            From = createdOn.UtcDateTime,
            To = createdOn.AddDays(1).UtcDateTime
        });
        Assert.Multiple(() =>
        {
            Assert.That(actual.TotalCount, Is.EqualTo(playerAccountChanges.Length));
            Assert.That(actual.Amount, Is.EqualTo(playerAccountChanges.Sum(s => s.Amount)));
            Assert.That(actual.BeforeBalance, Is.EqualTo(playerAccountChanges.Sum(s => s.BeforeBalance)));
            Assert.That(actual.AfterBalance, Is.EqualTo(playerAccountChanges.Sum(s => s.AfterBalance)));
        });
    }

    [Test]
    public async Task Can_Get_Multiple_Players_Account_Changes()
    {
        var fakePlayerFactory = Substitute.For<IPlayerFactory>();

        const string username = "";
        var brandId = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

        var fixture = new Fixture();

        var playerAccountChanges = fixture.CreateMany<Tgs.Report.ViewModel.Player.PlayersAccountChangeModel>(20).ToArray();

        _ = fakePlayerFactory.GetPlayersAccountChangesQueryable(Arg.Any<GetPlayersAccountChangesQuery>())
            .Returns(playerAccountChanges.AsQueryable().BuildMock());

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                _ = services.AddSingleton(fakePlayerFactory);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetMultiplePlayersAccountChangesAsync(new GetPlayersAccountChangesRequest
        {
            BrandId = brandId,
            Username = username,
            From = createdOn.UtcDateTime,
            To = createdOn.AddDays(1).UtcDateTime
        }).ToArrayAsync();

        foreach (var playersAccountChangeModel in actual)
        {
            Assert.Multiple(() =>
            {
                Assert.That(playerAccountChanges.Select(x => x.Username), Does.Contain(playersAccountChangeModel.Username));
                Assert.That(playerAccountChanges.Select(x => x.Amount), Does.Contain(playersAccountChangeModel.Amount));
                Assert.That(playerAccountChanges.Select(x => x.BeforeBalance), Does.Contain(playersAccountChangeModel.BeforeBalance));
                Assert.That(playerAccountChanges.Select(x => x.AfterBalance), Does.Contain(playersAccountChangeModel.AfterBalance));
                Assert.That(playerAccountChanges.Select(x => x.TransactionType), Does.Contain((TransactionType)playersAccountChangeModel.TransactionType));
                Assert.That(playerAccountChanges.Select(x => x.Operator), Does.Contain(playersAccountChangeModel.Operator));
            });
        }
    }

    private static Fixture CreatePlayerAllAgentRebateContributorFixture(Guid brandId, DateTimeOffset settleDate, decimal sportsTurnoverRebate)
    {
        var fixture = new Fixture();

        fixture.Customize<PlayerAllAgentRebateContributor>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.SettleDate, settleDate)
            .With(s => s.SportsTurnoverRebate, sportsTurnoverRebate));

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