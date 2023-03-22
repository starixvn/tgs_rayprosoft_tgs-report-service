using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Round;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.UnitTest.Round;

[TestFixture]
public sealed class GetBetRecordsQueryTest
{
    [Test]
    public async Task Can_Get_Bet_Records()
    {
        var fakeBrandGameService = Substitute.For<ITgsService<BrandGame>>();
        var fakeGameProviderService = Substitute.For<ITgsService<Repository.Entities.GameProvider>>();
        var fakeGameService = Substitute.For<ITgsService<Game>>();
        var fakePlayerService = Substitute.For<ITgsService<Repository.Entities.Player>>();
        var fakeRoundDetailService = Substitute.For<ITgsService<RoundDetail>>();
        var fakeRoundService = Substitute.For<ITgsService<Repository.Entities.Round>>();

        var sut = new GetBetRecordsQueryHandler(fakeRoundService, fakePlayerService, fakeGameService,
            fakeRoundDetailService, fakeGameProviderService, fakeBrandGameService);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var roundId = new Guid("aac02111-20f6-4f0d-6982-08d9dbdc0473");
        var gameProviderId = new Guid("aac02112-20f6-4f0d-6982-08d9dbdc0473");
        var gameId = 1;

        var players = CreatePlayerFixture(brandId).CreateMany<Repository.Entities.Player>(1).AsQueryable()
            .BuildMock();

        var rounds = CreateRoundFixture(brandId, roundId, gameProviderId, gameId)
            .CreateMany<Repository.Entities.Round>(1).AsQueryable()
            .BuildMock();

        var games = CreateGameFixture(gameId).CreateMany<Game>(1).AsQueryable()
            .BuildMock();

        var roundDetails = CreateRoundDetailFixture(roundId).CreateMany<RoundDetail>(1).AsQueryable()
            .BuildMock();

        var gameProviders = CreateGameProviderFixture(gameProviderId)
            .CreateMany<Repository.Entities.GameProvider>(1).AsQueryable()
            .BuildMock();

        var brandGames = CreateBrandGameFixture(brandId, gameId).CreateMany<BrandGame>(1).AsQueryable()
            .BuildMock();

        var request = new GetBetRecordsRequest
        {
            BrandId = brandId,
            PlayerCode = "PlayerCode",
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1)
        };

        fakeBrandGameService.GetQueries().Returns(brandGames);
        fakeGameProviderService.GetQueries().Returns(gameProviders);
        fakeGameService.GetQueries().Returns(games);
        fakePlayerService.GetQueries().Returns(players);
        fakeRoundDetailService.GetQueries().Returns(roundDetails);
        fakeRoundService.GetQueries().Returns(rounds);

        var response = await sut.Handle(request, default);

        Assert.AreEqual(1, response.TotalCount);
    }

    private static Fixture CreatePlayerFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.Player>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.PlayerCode, "PlayerCode")
            .With(s => s.Brand, new Brand())
            .With(s => s.Birthdate, DateOnly.Parse("2022/05/04")));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateRoundFixture(Guid brandId, Guid roundId, Guid gameProviderId, int gameId)
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.Round>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Id, roundId)
            .With(s => s.GameProviderId, gameProviderId)
            .With(s => s.PlayerCode, "PlayerCode")
            .With(s => s.Status, (short)RoundStatus.Open)
            .With(s => s.CreatedOn, DateTime.UtcNow)
            .With(s => s.GameId, gameId));
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
}