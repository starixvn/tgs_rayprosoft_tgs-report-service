using System;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core;
using Tgs.Report.Core.Queries.AgentWinLoss;
using Tgs.Report.Enum;
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
            CreateAgentPlayerSpecialDetailFixture().Create<AgentPlayerSpecialFeesModel>();


        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var agentId = new Guid("aac22220-20f6-4f0d-6982-08d9dbdc0473");

        _ = fakeAgentFactory.GetAgentPlayerSpecialFeesAsync(Arg.Any<GetAgentPlayerSpecialFeesDataRequest>())
            .Returns(agentPlayerSpecialDetail);

        var sut = new GetAgentPlayerSpecialDetailQueryHandler(fakeAgentFactory);

        var request = new GetAgentPlayerSpecialDetailRequest
        {
            BrandId = brandId,
            AgentId = agentId,
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now
        };

        var response = await sut.Handle(request, default);

        Assert.That(response.Bonus, Is.EqualTo(agentPlayerSpecialDetail.Bonus));
        Assert.That(response.CompanyAdjust, Is.EqualTo(agentPlayerSpecialDetail.CompanyAdjust));
        Assert.That(response.BonusAdjust, Is.EqualTo(agentPlayerSpecialDetail.BonusAdjust));
        Assert.That(response.AgentAdjust, Is.EqualTo(agentPlayerSpecialDetail.AgentAdjust));
    }

    private static Fixture CreateAgentPlayerSpecialDetailFixture()
    {
        var fixture = new Fixture();
        fixture.Build<AgentPlayerSpecialFeesModel>();
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }

    private static Fixture CreateProductFeeDataModelFixture(Guid agentId, decimal productFeePercentage, GameType gameType, decimal winLoss)
    {
        var fixture = new Fixture();
        fixture.Customize<ProductFeeDataModel>(u => u
            .With(s => s.AgentId, agentId)
            .With(s => s.ProduceFeePercentage, productFeePercentage)
            .With(s => s.GameType, gameType)
            .With(s => s.WinLossAmount, winLoss));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}