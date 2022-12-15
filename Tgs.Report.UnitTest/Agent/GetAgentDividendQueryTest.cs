using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.UnitTest.Agent
{
    [TestFixture]
    public sealed class GetAgentDividendQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Sub_Agent_Dividends()
        {
            var fakeDbContext = Substitute.For<TgsContext>();

            var sut = new GetAgentDividendQueryHandler(fakeDbContext);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var agentId = new Guid("aac11120-20f6-4f0d-6982-08d9dbdc0473");
            var childAgentId = new Guid("bbc11120-20f6-4f0d-6982-08d9dbdc0473");

            var agent1 = CreateAgentFixture(brandId, agentId)
                .Create<Repository.Entities.Agent>();

            var agent2 = CreateAgentFixture(brandId, childAgentId)
                .Create<Repository.Entities.Agent>();

            agent2.ParentAgentId = agentId;

            var agents = new Repository.Entities.Agent[]
            {
                agent1,
                agent2
            }.AsQueryable()
                .BuildMock();

            var agentDividends = CreateAgentDividendFixture(brandId, childAgentId).CreateMany<AgentDividend>(1).AsQueryable()
                .BuildMock();

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

            var playerDailyPaymentStatistics = 
                CreatePlayerDailyPaymentStatisticsFixture().CreateMany<PlayerDailyPaymentStatistic>(5).AsQueryable()
                .BuildMock();

            var mockAgents = GetDbSet(agents);
            var mockAgentDividends = GetDbSet(agentDividends);
            var mockPlayers = GetDbSet(players);
            var mockAgentDividendMultipleLayers = GetDbSet(agentDividendMultipleLayers);
            var mockPlayerDailyPaymentStatistics = GetDbSet(playerDailyPaymentStatistics);

            fakeDbContext.Agents.Returns(mockAgents.Object);
            fakeDbContext.AgentDividends.Returns(mockAgentDividends.Object);
            fakeDbContext.Players.Returns(mockPlayers.Object);
            fakeDbContext.AgentDividendMultipleLayers.Returns(mockAgentDividendMultipleLayers.Object);
            fakeDbContext.PlayerDailyPaymentStatistics.Returns(mockPlayerDailyPaymentStatistics.Object);

            var response = await sut.Handle(request, default);

            Assert.IsTrue(response.Valid);
            Assert.AreEqual(1, response.Items?.Length);
        }

        private static Fixture CreateAgentFixture(Guid brandId, Guid agentId)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Entities.Agent>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Id, agentId)
                .With(s => s.Brand, new Brand())
                .With(s => s.Players, new Repository.Entities.Player[] { }));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateAgentDividendFixture(Guid brandId, Guid agentId)
        {
            var fixture = new Fixture();
            fixture.Customize<AgentDividend>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.AgentId, agentId)
                .With(s => s.From, DateTime.UtcNow)
                .With(s => s.To, DateTime.UtcNow)
                .With(s => s.AgentDividendMultipleLayers, new AgentDividendMultipleLayer[]{}));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreatePlayerDailyPaymentStatisticsFixture()
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerDailyPaymentStatistic>(u => u
                .With(s => s.ReportDate, 
                    new DateOnly(DateTimeOffset.UtcNow.Year,DateTimeOffset.UtcNow.Month,DateTimeOffset.UtcNow.Day)));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}