using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.GameProvider;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.GameProvider;

namespace Tgs.Report.UnitTest.GameProvider
{
    [TestFixture]
    public sealed class GetGameProviderRoundsQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Game_Provider_Rounds()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetGameProviderRoundsQueryHandler(fakeRepository);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var gameId = 1;
            var gameProviderId = new Guid("aac11120-20f6-4f0d-6982-08d9dbdc0473");

            var gameProviders = CreateGameProviderFixture(gameProviderId)
                .CreateMany<Repository.Configurations.GameProvider>(1)
                .AsQueryable()
                .BuildMock().Object;

            var brands = CreateBrandFixture(brandId).CreateMany<Brand>(1)
                .AsQueryable()
                .BuildMock().Object;


            var games = CreateGameFixture(gameId).CreateMany<Game>(1)
                .AsQueryable()
                .BuildMock().Object;

            var rounds = CreateRoundFixture(brandId, gameId, gameProviderId)
                .CreateMany<Repository.Configurations.Round>(1)
                .AsQueryable()
                .BuildMock().Object;

            var request = new GetGameProviderRoundsRequest
            {
                BrandId = brandId,
                GameProviderCode = "GameProviderCode",
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1),
                SortBy = null
            };

            var mockGameProviders = GetDbSet(gameProviders);
            var mockBrands = GetDbSet(brands);
            var mockGames = GetDbSet(games);
            var mockRounds = GetDbSet(rounds);

            fakeRepository.GameProviders.Returns(mockGameProviders.Object);
            fakeRepository.Brands.Returns(mockBrands.Object);
            fakeRepository.Games.Returns(mockGames.Object);
            fakeRepository.Rounds.Returns(mockRounds.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.Datas.TotalCount);
        }

        private static Fixture CreateGameProviderFixture(Guid id)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Configurations.GameProvider>(u => u
                .With(s => s.Id, id)
                .With(s => s.Code, "GameProviderCode"));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateBrandFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<Brand>(u => u
                .With(s => s.Id, brandId));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateRoundFixture(Guid brandId, int gameId, Guid gameProviderId)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Configurations.Round>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.GameId, gameId)
                .With(s => s.CreatedOn, DateTime.UtcNow)
                .With(s => s.GameProviderId, gameProviderId));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateGameFixture(int id)
        {
            var fixture = new Fixture();
            fixture.Customize<Game>(u => u
                .With(s => s.Id, id));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}