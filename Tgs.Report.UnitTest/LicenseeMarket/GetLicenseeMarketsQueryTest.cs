using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.LicenseeMarket;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.LicenseeMarket;

namespace Tgs.Report.UnitTest.LicenseeMarket
{
    [TestFixture]
    public sealed class GetLicenseeMarketsQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Licensee_Markets()
        {
            var fakeDbContext = Substitute.For<TgsContext>();

            var sut = new GetLicenseeMarketsQueryHandler(fakeDbContext);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var licenseeId = new Guid("aac11120-20f6-4f0d-6982-08d9dbdc0473");

            var brands = CreateBrandFixture(brandId, licenseeId).CreateMany<Brand>(1)
                .AsQueryable()
                .BuildMock();

            var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                .BuildMock();

            var playerAggregates = CreatePlayerAggregateFixture(brandId).CreateMany<PlayerAggregate>(1)
                .AsQueryable()
                .BuildMock();

            var request = new GetLicenseeMarketsRequest
            {
                LicenseeId = licenseeId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1),
                SortBy = null
            };

            var mockBrands = GetDbSet(brands);
            var mockPlayerAggregates = GetDbSet(playerAggregates);
            var mockWithdrawForms = GetDbSet(withdrawForms);

            fakeDbContext.Brands.Returns(mockBrands.Object);
            fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);
            fakeDbContext.WithdrawForms.Returns(mockWithdrawForms.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.Datas?.TotalCount);
        }

        private static Fixture CreateBrandFixture(Guid brandId, Guid licenseeId)
        {
            var fixture = new Fixture();
            fixture.Customize<Brand>(u => u
                .With(s => s.Id, brandId)
                .With(s => s.LicenseeId, licenseeId)
                .With(s => s.VipLevels, new VipLevel[] { })
                .With(s => s.Transactions, new Transaction[] { })
                .With(s => s.BrandCurrencies, new BrandCurrency[] { })
                .With(s => s.Agents, new Repository.Entities.Agent[] { })
                .With(s => s.Players, new Repository.Entities.Player[] { })
                .With(s => s.DepositForms, new DepositForm[] { })
                .With(s => s.PlayerLoginRecords, new PlayerLoginRecord[] { }));

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