using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.ResellerMarket;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.ResellerMarket;

namespace Tgs.Report.UnitTest.ResellerMarket
{
    public sealed class GetResellerMarketsQueryTest
    {
        [Test]
        public async Task Can_Get_Reseller_Markets()
        {
            var fakeBrand = Substitute.For<ITgsService<Brand>>();
            var fakeWithdrawForm = Substitute.For<ITgsService<WithdrawForm>>();
            var fakePlayerAggregate = Substitute.For<ITgsService<PlayerAggregate>>();

            var sut = new GetResellerMarketsQueryHandler(fakeBrand, fakeWithdrawForm, fakePlayerAggregate);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var resellerId = new Guid("aac11120-20f6-4f0d-6982-08d9dbdc0473");

            var brands = CreateBrandFixture(brandId, resellerId).CreateMany<Brand>(1)
                .AsQueryable()
                .BuildMock();

            var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                .BuildMock();

            var playerAggregates = CreatePlayerAggregateFixture(brandId).CreateMany<PlayerAggregate>(1)
                .AsQueryable()
                .BuildMock();

            fakeBrand.GetQueries().Returns(brands);
            fakeWithdrawForm.GetQueries().Returns(withdrawForms);
            fakePlayerAggregate.GetQueries().Returns(playerAggregates);

            var request = new GetResellerMarketsRequest
            {
                ResellerId = resellerId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1),
                SortBy = null
            };

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.Datas?.TotalCount);
        }

        private static Fixture CreateBrandFixture(Guid brandId, Guid resellerId)
        {
            var fixture = new Fixture();
            fixture.Customize<Brand>(u => u
                .With(s => s.Id, brandId)
                .With(s => s.Licensee, new Licensee { ResellerId = resellerId }));
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
                .With(s => s.ReportDate, DateTime.UtcNow));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}