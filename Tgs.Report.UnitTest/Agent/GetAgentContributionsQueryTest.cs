using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.UnitTest.Agent
{
    [TestFixture]
    public sealed class GetAgentContributionsQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Agent_Contributions()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetAgentContributionsQueryHandler(fakeRepository);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var agentUsername = "agentUsername";

            var agents = new[]
            {
                new Repository.Configurations.Agent
                {
                    Id = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0472"),
                    BrandId = brandId,
                    Username = agentUsername
                }
            }.AsQueryable();

            var withdrawForms = new[]
            {
                new WithdrawForm
                {
                    BrandId = brandId
                }
            }.AsQueryable();

            var players = new[]
            {
                new Repository.Configurations.Player
                {
                    BrandId = brandId,
                    AgentId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0472")
                }
            }.AsQueryable();

            var playerAggregates = new[]
            {
                new PlayerAggregate
                {
                    BrandId = brandId,
                    AgentId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0472")
                }
            }.AsQueryable();

            var request = new GetAgentContributionsRequest
            {
                BrandId = brandId,
                AgentId = null,
                Username = agentUsername,
                SortBy = null
            };

            var mockAgents = GetDbSet(agents);
            var mockWithdrawForms = GetDbSet(withdrawForms);
            var mockPlayers = GetDbSet(players);
            var mockPlayerAggregates = GetDbSet(playerAggregates);

            fakeRepository.Agents.Returns(mockAgents.Object);
            fakeRepository.WithdrawForms.Returns(mockWithdrawForms.Object);
            fakeRepository.Players.Returns(mockPlayers.Object);
            fakeRepository.PlayerAggregates.Returns(mockPlayerAggregates.Object);

            var response = await sut.Handle(request, default);

            Assert.IsTrue(response.Valid);
            Assert.AreEqual(players.Count(), response.Items.Length);
        }

        private static Fixture CreateAgentFixture(Guid brandId, string username)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Configurations.Agent>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Username, username)
                .With(s => s.Brand, new Brand()));
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

        private static Fixture CreatePlayerAggregateFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerAggregate>(u => u
                .With(s => s.BrandId, brandId));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}