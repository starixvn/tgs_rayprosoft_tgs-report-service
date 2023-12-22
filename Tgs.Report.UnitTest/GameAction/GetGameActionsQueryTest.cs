using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.GameAction;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.GameAction;

namespace Tgs.Report.UnitTest.GameAction;

[TestFixture]
[Parallelizable]
public sealed class GetGameActionsQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Game_Actions()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var sut = new GetGameActionsQueryHandler(fakeDbContext);

        var gameActions = CreateGameActionFixture().CreateMany<Repository.Entities.GameAction>(1)
            .AsQueryable()
            .BuildMock();

        var request = new GetGameActionsRequest
        {
            RoundId = gameActions.First().RoundId,
            SortBy = null
        };

        var mockGameActions = GetDbSet(gameActions);

        fakeDbContext.GameActions.Returns(mockGameActions.Object);

        var response = await sut.Handle(request, default);

        Assert.That(response.Valid, Is.True);
        Assert.That(response.Items?.Length, Is.EqualTo(1));
    }

    private static Fixture CreateGameActionFixture()
    {
        var fixture = new Fixture();
        fixture.Customize<Repository.Entities.GameAction>(u => u);
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        return fixture;
    }
}