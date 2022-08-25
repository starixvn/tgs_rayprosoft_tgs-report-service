using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Agent;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Agent;

namespace Tgs.Report.UnitTest.Agent
{
    [TestFixture]
    public sealed class GetAgentDividendTeamDetailQueryTest
    {
        [Test]
        public async Task Can_Get_Agent_Dividend_Team_Details()
        {
            var fakeAgentRepository = Substitute.For<ITgsService<Repository.Entities.Agent>>();
            var fakeAgentDividendRepository = Substitute.For<ITgsService<AgentDividend>>();

            var sut = new GetAgentDividendTeamDetailQueryHandler(fakeAgentRepository, fakeAgentDividendRepository);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var agentId = new Guid("aac11120-20f6-4f0d-6982-08d9dbdc0473");
            var childAgentId = new Guid("aac22220-20f6-4f0d-6982-08d9dbdc0473");
            var from = DateTimeOffset.UtcNow;
            var to = DateTimeOffset.UtcNow;

            var agents = CreateAgentFixture(brandId, agentId).CreateMany<Repository.Entities.Agent>(1)
                .AsQueryable()
                .BuildMock();


            var childAgents = CreateAgentFixture(brandId, childAgentId).CreateMany<Repository.Entities.Agent>(1)
                .AsQueryable()
                .BuildMock();

            fakeAgentRepository.GetQueries().Returns(agents.Union(childAgents));

            var agentDividends = CreateAgentDividendFixture(brandId, agentId, childAgentId, from, to)
                .CreateMany<AgentDividend>(1)
                .AsQueryable()
                .BuildMock();

            var childAgentDividends = CreateAgentDividendFixture(brandId, childAgentId, Guid.NewGuid(), from, to)
                .CreateMany<AgentDividend>(1).AsQueryable()
                .BuildMock();

            fakeAgentDividendRepository.GetQueries().Returns(agentDividends.Union(childAgentDividends));

            var request = new GetAgentDividendTeamDetailsRequest
            {
                AgentId = agentId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1),
                SortBy = null
            };

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
                        DividendProviderAgentId = childAgentId,
                        Dividend = 100
                    }
                }));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}