using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.BrandGameProvider;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.BrandGameProvider;

namespace Tgs.Report.UnitTest.BrandGameProvider
{
    public sealed class GetBrandGameProviderRoundsQueryTest
    {
        [Test]
        public async Task Can_Get_Game_Provider_Rounds()
        {
            var fakeBrandGameProviderAggregate = Substitute.For<ITgsService<BrandGameProviderAggregate>>();
            var fakeBrand = Substitute.For<ITgsService<Brand>>();
            var fakeGameProvider = Substitute.For<ITgsService<Repository.Entities.GameProvider>>();

            var sut = new GetBrandGameProviderRoundsQueryHandler(fakeBrandGameProviderAggregate, fakeBrand,
                fakeGameProvider);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var gameProviderId = new Guid("aac11120-20f6-4f0d-6982-08d9dbdc0473");

            var brands = CreateBrandFixture(brandId)
                .CreateMany<Brand>(1)
                .AsQueryable()
                .BuildMock();

            var gameProviders = CreateGameProviderFixture(gameProviderId)
                .CreateMany<Repository.Entities.GameProvider>(1)
                .AsQueryable()
                .BuildMock();

            var brandGameProviderAggregate = CreateBrandGameProviderAggregateFixture(brandId, gameProviderId)
                .CreateMany<BrandGameProviderAggregate>(1)
                .AsQueryable()
                .BuildMock();

            fakeBrand.GetQueries().Returns(brands);
            fakeGameProvider.GetQueries().Returns(gameProviders);
            fakeBrandGameProviderAggregate.GetQueries().Returns(brandGameProviderAggregate);

            var request = new GetBrandGameProviderRoundsRequest
            {
                BrandId = brandId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1),
                SortBy = null
            };

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.Datas!.TotalCount);
        }

        private static Fixture CreateBrandFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<Brand>(u => u
                .With(s => s.Id, brandId)
                .With(s => s.Code, "GameProviderCode"));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
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

        private static Fixture CreateBrandGameProviderAggregateFixture(Guid brandId, Guid gameProviderId)
        {
            var fixture = new Fixture();
            fixture.Customize<BrandGameProviderAggregate>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.GameProviderId, gameProviderId)
                .With(s => s.ReportDate, DateTimeOffset.UtcNow));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}