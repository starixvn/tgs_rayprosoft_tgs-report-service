using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.IntegrationTest;

[TestClass]
public class RoundTests : BaseTest
{
    [TestMethod]
    public async Task Can_Get_Bet_Records()
    {
        var brandId = Guid.NewGuid();
        var playerCode = "playerCode";
        var gameProviderId = Guid.NewGuid();
        var createdOn = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new(8, 0, 0));

        await using var web = new TestReportApplication(builder => builder
            .ConfigureServices(services =>
            {
                var fakeDbContext = Substitute.For<TgsContext>();
                var fakeRoundService = Substitute.For<ITgsService<Round>>();
                var fakeRoundDetailService = Substitute.For<ITgsService<RoundDetail>>();
                var fakeGameService = Substitute.For<ITgsService<Game>>();
                var fakePlayerService = Substitute.For<ITgsService<Player>>();
                var fakeBrandGameService = Substitute.For<ITgsService<BrandGame>>();
                var fakeGameProviderService = Substitute.For<ITgsService<GameProvider>>();

                var rounds = CreateRoundFixture(brandId, createdOn, gameProviderId, null)
                    .CreateMany<Round>(2).Union(CreateRoundFixture(brandId, createdOn, gameProviderId, playerCode)
                        .CreateMany<Round>(3))
                    .AsQueryable()
                    .BuildMock();

                var roundDetails = rounds.Select(x => CreateRoundDetailFixture(x.Id).Create<RoundDetail>())
                    .ToArray()
                    .AsQueryable()
                    .BuildMock();

                var games = rounds.Select(x => x.GameId).Distinct()
                    .Select(id => CreateGameFixture(id).Create<Game>()).ToArray()
                    .AsQueryable()
                    .BuildMock();

                var players = rounds.Select(x => x.PlayerCode).Distinct()
                    .Select(x => CreatePlayerFixture(brandId, x).Create<Player>()).ToArray()
                    .AsQueryable()
                    .BuildMock();

                var gameProviders = CreateGameProviderFixture(gameProviderId)
                    .CreateMany<GameProvider>(1).ToArray().AsQueryable()
                    .BuildMock();

                var brandGames = games.Select(x => x.Id).Distinct()
                    .Select(id => CreateBrandGameFixture(brandId, id).Create<BrandGame>())
                    .ToArray()
                    .AsQueryable()
                    .BuildMock();

                fakeRoundService.GetQueries().Returns(rounds);
                fakeGameService.GetQueries().Returns(games);
                fakeRoundDetailService.GetQueries().Returns(roundDetails);
                fakePlayerService.GetQueries().Returns(players);
                fakeBrandGameService.GetQueries().Returns(brandGames);
                fakeGameProviderService.GetQueries().Returns(gameProviders);

                _ = services.AddSingleton(fakeDbContext);
                _ = services.AddTransient(x => fakeRoundService);
                _ = services.AddTransient(x => fakeRoundDetailService);
                _ = services.AddTransient(x => fakeGameService);
                _ = services.AddTransient(x => fakePlayerService);
                _ = services.AddTransient(x => fakeBrandGameService);
                _ = services.AddTransient(x => fakeGameProviderService);
            }));

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var actual = await sut.GetBetRecordsAsync(new()
        {
            BrandId = brandId,
            PlayerCode = playerCode,
            From = createdOn,
            To = createdOn.AddDays(1)
        });

        Assert.IsNotNull(actual.Datas);
        Assert.IsTrue(actual.Datas.Length == 3);
    }

    private static Fixture CreateRoundFixture(Guid brandId, DateTimeOffset createdOn, Guid gameProviderId, string? playerCode)
    {
        var fixture = new Fixture();
        fixture.Customize<Round>(u =>
        {
            var custom = u
                .With(s => s.BrandId, brandId)
                .With(s => s.GameProviderId, gameProviderId)
                .With(s => s.CreatedOn, createdOn);

            if (playerCode != null)
            {
                custom = custom.With(s => s.PlayerCode, playerCode);
            }

            return custom;
        });
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateRoundDetailFixture(Guid roundId)
    {
        var fixture = new Fixture();
        fixture.Customize<RoundDetail>(u => u
            .With(s => s.RoundId, roundId));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateGameFixture(int gameId)
    {
        var fixture = new Fixture();
        fixture.Customize<Game>(u => u
            .With(s => s.Id, gameId)
            .With(s => s.BrandGames, new BrandGame[]
            {
            }));

        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateGameProviderFixture(Guid gameProviderId)
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.GameProvider>(u => u
            .With(s => s.Id, gameProviderId)
            .With(s => s.Games, new Game[]
            {
            })
            .With(s => s.SubGameProviders, new SubGameProvider[]
            {
            }));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateBrandGameFixture(Guid brandId, int gameId)
    {
        var fixture = new Fixture();
        fixture.Customize<BrandGame>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.GameId, gameId)
            .With(s => s.Brand, new Brand()));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerFixture(Guid brandId, string playerCode)
    {
        var fixture = new Fixture();
        fixture.Customize<Player>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Brand, new Brand())
            .With(s => s.PlayerCode, playerCode)
            .With(s => s.Birthdate, DateOnly.Parse("2022/05/04")));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreatePlayerDailyRegisterStatisticsFixture(Guid brandId, DateTimeOffset reportDate)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerDailyRegisterStatistic>(u => u
            .With(s => s.ReportDate, DateOnly.FromDateTime(reportDate.DateTime)));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}