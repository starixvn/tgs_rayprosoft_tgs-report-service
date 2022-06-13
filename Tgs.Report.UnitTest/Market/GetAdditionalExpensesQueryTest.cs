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
    public sealed class GetAdditionalExpensesQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Additional_Expenses()
        {
            var fakeDbContext = Substitute.For<TgsContext>();

            var sut = new GetAdditionalExpensesQueryHandler(fakeDbContext);

            var brandId = Guid.NewGuid();
            var reportDate = DateTimeOffset.UtcNow;

            var playerAggregates = CreatePlayerAggregateFixture(brandId, reportDate).CreateMany<PlayerAggregate>(1)
                .AsQueryable()
                .BuildMock();

            var request = new GetAdditionalExpensesRequest
            {
                BrandId = brandId,
                ReportDate = reportDate
            };

            var mockPlayerAggregates = GetDbSet(playerAggregates);

            fakeDbContext.PlayerAggregates.Returns(mockPlayerAggregates.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual(playerAggregates.Sum(x => x.AllAgentRebateAmount),
                response.AdditionalExpenses.First().Amount);
        }

        private static Fixture CreatePlayerAggregateFixture(Guid brandId, DateTimeOffset reportDate)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerAggregate>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.ReportDate, reportDate.UtcDateTime));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}