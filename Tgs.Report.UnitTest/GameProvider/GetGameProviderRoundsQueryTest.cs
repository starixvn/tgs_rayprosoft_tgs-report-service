using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.GameProvider;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.GameProvider;

namespace Tgs.Report.UnitTest.GameProvider
{
    public sealed class GetGameProviderRoundsQueryTest
    {
        [Test]
        public async Task Can_Get_Game_Provider_Rounds()
        {
            var fakeGameProviderAggregate = Substitute.For<ITgsService<GameProviderAggregate>>();
            var fakeGameProvider = Substitute.For<ITgsService<Repository.Entities.GameProvider>>();

            var sut = new GetGameProviderRoundsQueryHandler(fakeGameProviderAggregate, fakeGameProvider);

            var gameProviderId = new Guid("aac11120-20f6-4f0d-6982-08d9dbdc0473");

            var gameProviders = CreateGameProviderFixture(gameProviderId)
                .CreateMany<Repository.Entities.GameProvider>(1)
                .AsQueryable()
                .BuildMock().Object;

            var gameProviderAggregate = CreateGameProviderAggregateFixture(gameProviderId)
                .CreateMany<GameProviderAggregate>(1)
                .AsQueryable()
                .BuildMock().Object;

            fakeGameProvider.GetQueries().Returns(gameProviders);
            fakeGameProviderAggregate.GetQueries().Returns(gameProviderAggregate);

            var request = new GetGameProviderRoundsRequest
            {
                GameProviderId = gameProviderId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1),
                SortBy = null
            };

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.TotalCount);
        }

        private static Fixture CreateGameProviderFixture(Guid id)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Entities.GameProvider>(u => u
                .With(s => s.Id, id)
                .With(s => s.Code, "GameProviderCode"));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateGameProviderAggregateFixture(Guid id)
        {
            var fixture = new Fixture();
            fixture.Customize<GameProviderAggregate>(u => u
                .With(s => s.GameProviderId, id)
                .With(s => s.ReportDate, DateTimeOffset.UtcNow));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}