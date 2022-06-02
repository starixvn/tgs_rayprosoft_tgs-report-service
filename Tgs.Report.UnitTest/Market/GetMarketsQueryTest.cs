using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Market;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Market;

namespace Tgs.Report.UnitTest.Market
{
    [TestFixture]
    public sealed class GetMarketsQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Markets()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetMarketsQueryHandler(fakeRepository);

            var brandId = Guid.NewGuid();
            var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

            var playerAggregates = CreatePlayerAggregateFixture(brandId, reportDate).CreateMany<PlayerAggregate>(1)
                .AsQueryable()
                .BuildMock();

            var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                .BuildMock();

            var request = new GetMarketsRequest
            {
                BrandId = brandId,
                From = reportDate,
                To = reportDate.AddDays(1)
            };

            var mockPlayerAggregates = GetDbSet(playerAggregates);
            var mockWithdrawForms = GetDbSet(withdrawForms);

            fakeRepository.WithdrawForms.Returns(mockWithdrawForms.Object);
            fakeRepository.PlayerAggregates.Returns(mockPlayerAggregates.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual(playerAggregates.First().RegisteredCount,
                response.Items?.First().RegisteredCount);
        }

        private static Fixture CreatePlayerAggregateFixture(Guid brandId, DateTimeOffset reportDate)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerAggregate>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.ReportDate, reportDate.DateTime));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateWithdrawFormFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<WithdrawForm>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Brand, new Brand())
                .With(s => s.CreatedOn));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}