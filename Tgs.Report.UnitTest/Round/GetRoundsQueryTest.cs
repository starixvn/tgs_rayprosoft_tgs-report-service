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
using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.UnitTest.Round
{
    [TestFixture]
    public sealed class GetRoundsQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Rounds()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetRoundsQueryHandler(fakeRepository);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var roundId = new Guid("aac02111-20f6-4f0d-6982-08d9dbdc0473");
            var gameProviderId = new Guid("aac02112-20f6-4f0d-6982-08d9dbdc0473");
            var gameId = 1;

            var players = CreatePlayerFixture(brandId).CreateMany<Repository.Configurations.Player>(1).AsQueryable()
                .BuildMock().Object;

            var rounds = CreateRoundFixture(brandId, roundId, gameProviderId, gameId)
                .CreateMany<Repository.Configurations.Round>(1).AsQueryable()
                .BuildMock().Object;

            var games = CreateGameFixture(gameId).CreateMany<Game>(1).AsQueryable()
                .BuildMock().Object;

            var roundDetails = CreateRoundDetailFixture(roundId).CreateMany<RoundDetail>(1).AsQueryable()
                .BuildMock().Object;

            var gameProviders = CreateGameProviderFixture(gameProviderId)
                .CreateMany<Repository.Configurations.GameProvider>(1).AsQueryable()
                .BuildMock().Object;

            var brandGames = CreateBrandGameFixture(brandId, gameId).CreateMany<BrandGame>(1).AsQueryable()
                .BuildMock().Object;

            var request = new GetRoundsRequest
            {
                BrandId = brandId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1)
            };

            var mockRounds = GetDbSet(rounds);
            var mockPlayers = GetDbSet(players);
            var mockGames = GetDbSet(games);
            var mockRoundDetails = GetDbSet(roundDetails);
            var mockGameProviders = GetDbSet(gameProviders);
            var mockBrandGames = GetDbSet(brandGames);

            fakeRepository.Rounds.Returns(mockRounds.Object);
            fakeRepository.Players.Returns(mockPlayers.Object);
            fakeRepository.Games.Returns(mockGames.Object);
            fakeRepository.RoundDetails.Returns(mockRoundDetails.Object);
            fakeRepository.GameProviders.Returns(mockGameProviders.Object);
            fakeRepository.BrandGames.Returns(mockBrandGames.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.Items.Length);
        }

        private static Fixture CreatePlayerFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Configurations.Player>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.PlayerCode, "PlayerCode")
                .With(s => s.Brand, new Brand()));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateRoundFixture(Guid brandId, Guid roundId, Guid gameProviderId, int gameId)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Configurations.Round>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Id, roundId)
                .With(s => s.GameProviderId, gameProviderId)
                .With(s => s.PlayerCode, "PlayerCode")
                .With(s => s.Status, RoundStatus.Open)
                .With(s => s.CreatedOn, DateTimeOffset.UtcNow)
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
                .With(s => s.Id, gameId));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateGameProviderFixture(Guid gameProviderId)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Configurations.GameProvider>(u => u
                .With(s => s.Id, gameProviderId));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateBrandGameFixture(Guid brandId, int gameId)
        {
            var fixture = new Fixture();
            fixture.Customize<BrandGame>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.GameId, gameId));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}