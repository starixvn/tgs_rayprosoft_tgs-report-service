using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.UnitTest.Agent
{
    [TestFixture]
    public sealed class GetAgentDividendTeamDetailQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Agent_Dividend_Team_Details()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetAgentDividendTeamDetailQueryHandler(fakeRepository);

            var brandId = Guid.NewGuid();
            var agentId = Guid.NewGuid();
            var childAgentId = Guid.NewGuid();
            var from = DateTimeOffset.UtcNow;
            var to = DateTimeOffset.UtcNow;

            var agents = CreateAgentFixture(brandId, agentId).CreateMany<Repository.Configurations.Agent>(1)
                .AsQueryable()
                .BuildMock().Object;

            var childAgents = CreateAgentFixture(brandId, childAgentId).CreateMany<Repository.Configurations.Agent>(1)
                .AsQueryable()
                .BuildMock().Object;

            var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                .BuildMock().Object;

            var agentDividends = CreateAgentDividendFixture(brandId, agentId, childAgentId, from, to)
                .CreateMany<AgentDividend>(1)
                .AsQueryable()
                .BuildMock().Object;

            var childAgentDividends = CreateAgentDividendFixture(brandId, childAgentId, Guid.NewGuid(), from, to)
                .CreateMany<AgentDividend>(1).AsQueryable()
                .BuildMock().Object;

            var agentDividendMultipleLayers = agentDividends.SelectMany(x => x.AgentDividendMultipleLayers);

            var players = agents.SelectMany(x => x.Players);

            var request = new GetAgentDividendTeamDetailsRequest
            {
                AgentId = agentId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1),
                SortBy = null
            };

            var mockAgents = GetDbSet(agents.Union(childAgents));
            var mockWithdrawForms = GetDbSet(withdrawForms);
            var mockAgentDividends = GetDbSet(agentDividends.Union(childAgentDividends));
            var mockPlayers = GetDbSet(players);
            var mockAgentDividendMultipleLayers = GetDbSet(agentDividendMultipleLayers);

            fakeRepository.Agents.Returns(mockAgents.Object);
            fakeRepository.WithdrawForms.Returns(mockWithdrawForms.Object);
            fakeRepository.AgentDividends.Returns(mockAgentDividends.Object);
            fakeRepository.Players.Returns(mockPlayers.Object);
            fakeRepository.AgentDividendMultipleLayers.Returns(mockAgentDividendMultipleLayers.Object);

            var response = await sut.Handle(request, default);

            Assert.IsTrue(response.Valid);
            Assert.AreEqual(1, response.Items.Length);
        }

        private static Fixture CreateAgentFixture(Guid brandId, Guid agentId)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Configurations.Agent>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Id, agentId)
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

        private static Fixture CreateAgentDividendFixture(Guid brandId, Guid agentId, Guid childAgentId,
            DateTimeOffset from, DateTimeOffset to)
        {
            var fixture = new Fixture();
            fixture.Customize<AgentDividend>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.AgentId, agentId)
                .With(s => s.CreatedOn, DateTime.UtcNow)
                .With(s => s.From, from.UtcDateTime)
                .With(s => s.To, to.UtcDateTime)
                .With(s => s.AgentDividendMultipleLayers, new[]
                {
                    new AgentDividendMultipleLayer
                    {
                        AgentId = agentId,
                        Layer = $"{agentId},{childAgentId}",
                        DividendProviderAgentId = childAgentId
                    }
                }));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateAgentDividendMultipleLayerFixture()
        {
            var fixture = new Fixture();
            fixture.Customize<AgentDividendMultipleLayer>(u => u
                .With(s => s.AgentDividend, new AgentDividend()));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}