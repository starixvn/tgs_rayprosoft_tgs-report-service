using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Market;
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
            var fakeDbContext = Substitute.For<TgsContext>();

            var sut = new GetMarketsQueryHandler(fakeDbContext);

            var brandId = Guid.NewGuid();
            var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

            var playerAggregates = CreatePlayerAggregateFixture(brandId, reportDate).CreateMany<PlayerAggregate>(1)
                .AsQueryable()
                .BuildMock();

            var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                .BuildMock();

            var playerLoginReocrds = CreatePlayerLoginRecordsFixture(brandId).CreateMany<PlayerLoginRecord>(1).AsQueryable()
                .BuildMock();

            var dailyStatistic = CreateBrandDailyPaymentStatisticFixture(brandId, reportDate).CreateMany<BrandDailyPaymentStatistic>(1).AsQueryable()
                .BuildMock();

            var playerBonusAggregates = CreatePlayerBonusAggregateFixture(brandId, reportDate).CreateMany<PlayerBonusAggregate>(1).AsQueryable()
                .BuildMock();

            var request = new GetMarketsRequest
            {
                BrandId = brandId,
                From = reportDate,
                To = reportDate.AddDays(1)
            };

            var playerDailyPaymentStatistics = 
                CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                    .BuildMock();

            var players = CreatePlayerFixture(brandId).CreateMany<Repository.Entities.Player>(1).AsQueryable()
                .BuildMock();

            var mockPlayerAggregates = GetDbSet(playerAggregates);
            var mockWithdrawForms = GetDbSet(withdrawForms);
            var mockPlayerLoginRecords = GetDbSet(playerLoginReocrds);
            var mockDailyStatistic = GetDbSet(dailyStatistic);
            var mockPlayerBonusAggregates = GetDbSet(playerBonusAggregates);
            var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);
            var mockPlayers = GetDbSet(players);

            fakeDbContext.WithdrawForms.Returns(mockWithdrawForms.Object);
            fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);
            fakeDbContext.PlayerLoginRecords.Returns(mockPlayerLoginRecords.Object);
            fakeDbContext.BrandDailyPaymentStatistics.Returns(mockDailyStatistic.Object);
            fakeDbContext.PlayerBonusAggregates.Returns(mockPlayerBonusAggregates.Object);
            fakeDbContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);
            fakeDbContext.Players.Returns(mockPlayers.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual(playerAggregates.First().RegisteredCount,
                response.Items?.First().RegisteredCount);
        }

        private static Fixture CreatePlayerAggregateFixture(Guid brandId, DateTimeOffset reportDate)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerAggregate>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.ReportDate, reportDate)
                .With(s => s.CreatedOn, reportDate));
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

        private static Fixture CreatePlayerLoginRecordsFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerLoginRecord>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.Brand, new Brand())
            .With(s => s.PlayerCodeNavigation, new Repository.Entities.Player()));

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateBrandDailyPaymentStatisticFixture(Guid brandId,DateTimeOffset reportDate)
        {
            var fixture = new Fixture();
            fixture.Customize<BrandDailyPaymentStatistic>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.ReportDate, new DateOnly(reportDate.Year, reportDate.Month, reportDate.Day)));

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreatePlayerBonusAggregateFixture(Guid brandId, DateTimeOffset reportDate)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerBonusAggregate>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.ReportDate, reportDate));
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