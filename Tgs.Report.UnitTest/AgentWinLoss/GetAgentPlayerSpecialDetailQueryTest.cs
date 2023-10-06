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
public sealed class GetAgentPlayerSpecialDetailQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Agent_Player_Special_Detail()
    {
        var fakeAgentFactory = Substitute.For<IAgentFactory>();
        var agentPlayerSpecialDetail =
            CreateAgentPlayerSpecialDetailFixture().CreateMany<AgentPlayerSpecialDetailModel>(1);

        fakeAgentFactory.GetAgentPlayerSpecialDetailAsync(Arg.Any<GetAgentPlayerSpecialDetailDataRequest>())
            .Returns(agentPlayerSpecialDetail);

        var sut = new GetAgentPlayerSpecialDetailQueryHandler(fakeAgentFactory);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var agentId = new Guid("aac22220-20f6-4f0d-6982-08d9dbdc0473");

        var request = new GetAgentPlayerSpecialDetailRequest
        {
            BrandId = brandId,
            AgentId = agentId,
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now
        };

        var response = await sut.Handle(request, default);

        Assert.AreEqual(agentPlayerSpecialDetail.First().Bonus, response.Bonus);
    }

    private static Fixture CreateAgentPlayerSpecialDetailFixture()
    {
        var fixture = new Fixture();
        fixture.Build<AgentPlayerSpecialDetailModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}