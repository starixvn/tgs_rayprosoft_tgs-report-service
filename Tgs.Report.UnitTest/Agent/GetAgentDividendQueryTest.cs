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
    public sealed class GetAgentDividendQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Agent_Dividends()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetAgentDividendQueryHandler(fakeRepository);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var agentId = new Guid("aac11120-20f6-4f0d-6982-08d9dbdc0473");
            var agents = CreateAgentFixture(brandId, agentId).CreateMany<Repository.Configurations.Agent>(1)
                .AsQueryable()
                .BuildMock().Object;

            var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                .BuildMock().Object;

            var agentDividends = CreateAgentDividendFixture(brandId, agentId).CreateMany<AgentDividend>(1).AsQueryable()
                .BuildMock().Object;

            var agentDividendMultipleLayers = agentDividends.SelectMany(x => x.AgentDividendMultipleLayers);

            var players = agents.SelectMany(x => x.Players);

            var request = new GetAgentDividendsRequest
            {
                BrandId = brandId,
                AgentId = agentId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1),
                SortBy = null
            };

            var mockAgents = GetDbSet(agents);
            var mockWithdrawForms = GetDbSet(withdrawForms);
            var mockAgentDividends = GetDbSet(agentDividends);
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

        private static Fixture CreateAgentDividendFixture(Guid brandId, Guid agentId)
        {
            var fixture = new Fixture();
            fixture.Customize<AgentDividend>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.AgentId, agentId)
                .With(s => s.CreatedOn, DateTime.UtcNow));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}