using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Player;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.UnitTest.Player
{
    [TestFixture]
    public sealed class GetPlayerContributionsQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Player_Contributions()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetPlayerContributionsQueryHandler(fakeRepository);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

            var playerAggregates = CreatePlayerAggregateFixture(brandId, reportDate).CreateMany<PlayerAggregate>(1)
                .AsQueryable()
                .BuildMock();

            var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                .BuildMock();

            var players = CreatePlayerFixture(brandId).CreateMany<Repository.Entities.Player>(1).AsQueryable()
                .BuildMock();

            var request = new GetPlayerContributionsRequest
            {
                BrandId = brandId,
                From = reportDate,
                To = reportDate.AddDays(1)
            };

            var mockPlayerAggregates = GetDbSet(playerAggregates);
            var mockWithdrawForms = GetDbSet(withdrawForms);
            var mockPlayers = GetDbSet(players);

            fakeRepository.WithdrawForms.Returns(mockWithdrawForms.Object);
            fakeRepository.PlayerAggregates.Returns(mockPlayerAggregates.Object);
            fakeRepository.Players.Returns(mockPlayers.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.Items?.Length);
        }

        private static Fixture CreatePlayerAggregateFixture(Guid brandId, DateTimeOffset reportDate)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerAggregate>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.ReportDate, reportDate));
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

        private static Fixture CreatePlayerFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Entities.Player>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Brand, new Brand())
                .With(s => s.Birthdate, DateOnly.Parse("2022/05/04")));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}