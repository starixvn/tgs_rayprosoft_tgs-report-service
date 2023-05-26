using System;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core;
using Tgs.Report.Core.Queries.AgentWinLoss;
using Tgs.Report.ViewModel.AgentWinLoss;

namespace Tgs.Report.UnitTest.AgentWinLoss;

[TestFixture]
public sealed class GetMasterAgentWinLossQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Master_Agent_Win_Loss()
    {
        var fakeAgentFactory = Substitute.For<IAgentFactory>();
        var agentWinLosses = CreateAgentWinLossFixture().CreateMany<AgentWinLossModel>(10);

        fakeAgentFactory.GetMasterAgentWinLossAsync(Arg.Any<GetMasterAgentWinLossDataRequest>())
            .Returns(agentWinLosses);

        var sut = new GetMasterAgentWinLossQueryHandler(fakeAgentFactory);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");

        var request = new GetMasterAgentWinLossRequest
        {
            BrandId = brandId,
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now
        };

        var response = await sut.Handle(request, default);

        Assert.IsTrue(response.Valid);
        Assert.AreEqual(10, response.Items?.Length);
    }

    private static Fixture CreateAgentWinLossFixture()
    {
        var fixture = new Fixture();
        fixture.Build<AgentWinLossModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}