using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Vip;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Vip;

namespace Tgs.Report.UnitTest.Vip;

[TestFixture]
[Parallelizable]
public sealed class GetVipRebateReportQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_vip_rebate_report()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var sut = new GetVipRebateReportQueryHandler(fakeDbContext);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");

        var aggregates = CreatePlayerGameRebateAggregateFixture(brandId).CreateMany<PlayerGameRebateAggregate>(1)
            .AsQueryable()
            .BuildMock();

        var players = CreatePlayerFixture(brandId)
            .CreateMany<Repository.Entities.Player>(1).AsQueryable()
            .BuildMock();

        var request = new GetVipRebateReportRequest
        {
            BrandId = brandId,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1)
        };

        var mockAggregates = GetDbSet(aggregates);
        var mockPlayers = GetDbSet(players);

        fakeDbContext.PlayerGameRebateAggregates.Returns(mockAggregates.Object);
        fakeDbContext.Players.Returns(mockPlayers.Object);

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

    private static Fixture CreatePlayerGameRebateAggregateFixture(Guid brandId)
    {
        var fixture = new Fixture();
        fixture.Customize<PlayerGameRebateAggregate>(u => u
            .With(s => s.BrandId, brandId)
            .With(s => s.ReportDate, DateTime.UtcNow));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}