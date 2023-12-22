using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core;
using Tgs.Report.Core.Queries.AgentWinLoss;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.UnitTest.AgentWinLoss;

[TestFixture]
[Parallelizable]
public sealed class GetCompanyTotalDetailQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Company_Total_Detail()
    {
        var fakeAgentFactory = Substitute.For<IAgentFactory>();
        var agentWinLosses = new[]
        {
            CreateAgentTotalDetailFixture(DateTimeOffset.Parse("2023/04/01 00:00:00+8"), 70)
                .Create<SpecialDetailDataModel>(),
            CreateAgentTotalDetailFixture(DateTimeOffset.Parse("2023/04/01 01:00:00+8"), 70)
                .Create<SpecialDetailDataModel>(),
            CreateAgentTotalDetailFixture(DateTimeOffset.Parse("2023/04/05 03:00:00+8"), 50)
                .Create<SpecialDetailDataModel>(),
            CreateAgentTotalDetailFixture(DateTimeOffset.Parse("2023/04/06 01:00:00+8"), 50)
                .Create<SpecialDetailDataModel>(),
            CreateAgentTotalDetailFixture(DateTimeOffset.Parse("2023/04/02 01:00:00+8"), 70)
                .Create<SpecialDetailDataModel>(),
            CreateAgentTotalDetailFixture(DateTimeOffset.Parse("2023/04/04 01:00:00+8"), 70)
                .Create<SpecialDetailDataModel>(),
            CreateAgentTotalDetailFixture(DateTimeOffset.Parse("2023/04/07 01:00:00+8"), 70)
                .Create<SpecialDetailDataModel>(),
            CreateAgentTotalDetailFixture(DateTimeOffset.Parse("2023/04/08 01:00:00+8"), 70)
                .Create<SpecialDetailDataModel>()
        };

        fakeAgentFactory.GetSpecialDetailAsync(Arg.Any<GetSpecialDetailDataRequest>())
            .Returns(agentWinLosses);

        var sut = new GetCompanyTotalDetailQueryHandler(fakeAgentFactory);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var agentId = new Guid("aac22220-20f6-4f0d-6982-08d9dbdc0473");

        var request = new GetCompanyTotalDetailRequest
        {
            BrandId = brandId,
            AgentId = agentId,
            From = DateTimeOffset.Parse("2023/04/01 00:00:00+8"),
            To = DateTimeOffset.Parse("2023/04/10 00:00:00+8")
        };

        var response = await sut.Handle(request, default);

        Assert.That(response.Count(), Is.EqualTo(3));
        Assert.That(-response.Where(x => x.Percentage == 30).Sum(x => x.PlayerWinLoss),
            Is.EqualTo(agentWinLosses.Where(x => x.Percentage == 70).Sum(x => x.WinLoss)));
    }

    private static Fixture CreateAgentTotalDetailFixture(DateTimeOffset reportDate, decimal percentage)
    {
        var fixture = new Fixture();
        fixture.Customize<SpecialDetailDataModel>(u => u
            .With(s => s.ReportDate, reportDate)
            .With(s => s.Percentage, percentage));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}