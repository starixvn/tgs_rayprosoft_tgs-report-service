using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.GameAction;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel.GameAction;

namespace Tgs.Report.UnitTest.GameAction
{
    [TestFixture]
    public sealed class GetGameActionsQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Game_Actions()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetGameActionsQueryHandler(fakeRepository);

            var gameActions = CreateGameActionFixture().CreateMany<Repository.Entities.GameAction>(1)
                .AsQueryable()
                .BuildMock().Object;

            var request = new GetGameActionsRequest
            {
                RoundId = gameActions.First().RoundId,
                SortBy = null
            };

            var mockGameActions = GetDbSet(gameActions);

            fakeRepository.GameActions.Returns(mockGameActions.Object);

            var response = await sut.Handle(request, default);

            Assert.IsTrue(response.Valid);
            Assert.AreEqual(1, response.Items?.Length);
        }

        private static Fixture CreateGameActionFixture()
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Entities.GameAction>(u => u);
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}