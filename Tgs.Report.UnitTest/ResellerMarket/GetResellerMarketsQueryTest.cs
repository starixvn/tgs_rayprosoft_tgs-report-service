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
    public sealed class GetResellerMarketsQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Reseller_Markets()
        {
            var fakeBrand = Substitute.For<ITgsService<Brand>>();
            var fakeWithdrawForm = Substitute.For<ITgsService<WithdrawForm>>();
            var fakePlayerAggregate = Substitute.For<ITgsService<PlayerAggregate>>();
            var fakePlayerBonusAggregate = Substitute.For<ITgsService<PlayerBonusAggregate>>();
            var fakeContext = Substitute.For<TgsContext>();

            var sut = new GetResellerMarketsQueryHandler(fakeContext, fakeBrand, fakeWithdrawForm, fakePlayerAggregate, fakePlayerBonusAggregate);

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

            var playerBonusAggregates = CreatePlayerBonusAggregateFixture(brandId).CreateMany<PlayerBonusAggregate>(1).AsQueryable()
                .BuildMock();

            var playerDailyPaymentStatistics = 
                CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                    .BuildMock();

            var deposits = CreateDepositFormFixture(brandId).CreateMany<DepositForm>(5).AsQueryable()
                .BuildMock();

            var players = CreatePlayerFixture(brandId).CreateMany<Repository.Entities.Player>(1).AsQueryable()
                .BuildMock();

            var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);
            var mockDepositForms = GetDbSet(deposits);
            var mockPlayers = GetDbSet(players);

            fakeBrand.GetQueries().Returns(brands);
            fakeWithdrawForm.GetQueries().Returns(withdrawForms);
            fakePlayerAggregate.GetQueries().Returns(playerAggregates);
            fakePlayerBonusAggregate.GetQueries().Returns(playerBonusAggregates);
            fakeContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);
            fakeContext.DepositForms.Returns(mockDepositForms.Object);
            fakeContext.Players.Returns(mockPlayers.Object);

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
                .With(s => s.Licensee, new Licensee { ResellerId = resellerId })
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

        private static Fixture CreateDepositFormFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<DepositForm>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Brand, new Brand())
                .With(s => s.PlayerCodeNavigation, new Repository.Entities.Player()));
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

        private static Fixture CreatePlayerBonusAggregateFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerBonusAggregate>(u => u
                .With(s => s.BrandId, brandId));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreatePlayerDailyPaymentStatisticsFixture()
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerDailyPaymentStatistic>(u => u
                .With(s => s.ReportDate, 
                    new DateOnly(DateTimeOffset.UtcNow.Year,DateTimeOffset.UtcNow.Month,DateTimeOffset.UtcNow.Day)));
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