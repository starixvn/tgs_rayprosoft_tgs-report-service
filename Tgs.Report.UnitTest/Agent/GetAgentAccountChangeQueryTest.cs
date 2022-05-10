using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.UnitTest.Agent
{
    [TestFixture]
    public sealed class GetGameActionsQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Agent_Account_Changes()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetAgentAccountChangeQueryHandler(fakeRepository);

            var agentTransactions = CreateAgentTransactionFixture().CreateMany<AgentTransaction>(1).AsQueryable()
                .BuildMock();

            var request = new GetAgentAccountChangesRequest
            {
                BrandId = agentTransactions.First().BrandId,
                AgentId = agentTransactions.First().AgentId,
                TransactionType = (TransactionType)agentTransactions.First().Type,
                From = agentTransactions.First().RecordedOn.AddDays(-1),
                To = agentTransactions.First().RecordedOn.AddDays(1),
                SortBy = null
            };

            var mockAgentTransactions = GetDbSet(agentTransactions);

            fakeRepository.AgentTransactions.Returns(mockAgentTransactions.Object);

            var response = await sut.Handle(request, default);

            Assert.IsTrue(response.Valid);
            Assert.AreEqual(1, response.Items?.Length);
        }

        private static Fixture CreateAgentTransactionFixture()
        {
            var fixture = new Fixture();
            fixture.Customize<AgentTransaction>(u => u
                .With(s => s.Brand, new Brand()));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}