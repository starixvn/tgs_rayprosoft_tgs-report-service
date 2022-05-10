using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Vip;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.UnitTest.Vip
{
    [TestFixture]
    public sealed class GetVipReportQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_vip_report()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetVipReportQueryHandler(fakeRepository);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");

            var players = CreatePlayerFixture(brandId)
                .CreateMany<Repository.Entities.Player>(1).AsQueryable()
                .BuildMock();

            var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                .BuildMock();

            var playerAggregates = CreatePlayerAggregateFixture(brandId).CreateMany<PlayerAggregate>(1).AsQueryable()
                .BuildMock();

            var playerGameAggregates = CreatePlayerGameRebateAggregateFixture(brandId)
                .CreateMany<PlayerGameRebateAggregate>(1)
                .AsQueryable()
                .BuildMock();

            var request = new GetVipReportRequest
            {
                BrandId = brandId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1)
            };

            var mockWithdrawForms = GetDbSet(withdrawForms);
            var mockPlayers = GetDbSet(players);
            var mockPlayerAggregates = GetDbSet(playerAggregates);
            var mockPlayerGameAggregates = GetDbSet(playerGameAggregates);

            fakeRepository.WithdrawForms.Returns(mockWithdrawForms.Object);
            fakeRepository.Players.Returns(mockPlayers.Object);
            fakeRepository.PlayerAggregates.Returns(mockPlayerAggregates.Object);
            fakeRepository.PlayerGameRebateAggregates.Returns(mockPlayerGameAggregates.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.Items?.Length);
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

        private static Fixture CreateWithdrawFormFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<WithdrawForm>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Brand, new Brand()));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreatePlayerAggregateFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerAggregate>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.PlayerCode, "PlayerCode")
                .With(s => s.ReportDate, DateTimeOffset.UtcNow));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreatePlayerGameRebateAggregateFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerGameRebateAggregate>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.PlayerCode, "PlayerCode")
                .With(s => s.ReportDate, DateTimeOffset.UtcNow));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}