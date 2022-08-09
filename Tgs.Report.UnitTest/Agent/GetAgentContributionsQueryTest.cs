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
    public sealed class GetAgentContributionsQueryTest
    {
        [Test]
        public async Task Can_Get_Agent_Contributions()
        {
            var fakeAgent = Substitute.For<ITgsService<Repository.Entities.Agent>>();
            var fakeWithdrawForm = Substitute.For<ITgsService<WithdrawForm>>();
            var fakePlayer = Substitute.For<ITgsService<Repository.Entities.Player>>();
            var fakePlayerAggregate = Substitute.For<ITgsService<PlayerAggregate>>();
            var fakePlayerBonusAggregate = Substitute.For<ITgsService<PlayerBonusAggregate>>();

            var sut = new GetAgentContributionsQueryHandler(fakeAgent, fakeWithdrawForm, fakePlayer,
                fakePlayerAggregate, fakePlayerBonusAggregate);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var agentUsername = "agentUsername";
            var agents = CreateAgentFixture(brandId, agentUsername).CreateMany<Repository.Entities.Agent>(1)
                .AsQueryable()
                .BuildMock();

            var withdrawForms = CreateWithdrawFormFixture(brandId).CreateMany<WithdrawForm>(1).AsQueryable()
                .BuildMock();

            var players = agents.SelectMany(x => x.Players);

            var playerAggregates = CreatePlayerAggregateFixture(brandId).CreateMany<PlayerAggregate>(1).AsQueryable()
                .BuildMock();

            var playerBonusAggregates = CreatePlayerBonusAggregateFixture(brandId).CreateMany<PlayerBonusAggregate>(1).AsQueryable()
                .BuildMock();

            var request = new GetAgentContributionsRequest
            {
                BrandId = brandId,
                AgentId = null,
                Username = agentUsername,
                SortBy = null
            };

            fakeAgent.GetQueries().Returns(agents);
            fakeWithdrawForm.GetQueries().Returns(withdrawForms);
            fakePlayer.GetQueries().Returns(players);
            fakePlayerAggregate.GetQueries().Returns(playerAggregates);
            fakePlayerBonusAggregate.GetQueries().Returns(playerBonusAggregates);

            var response = await sut.Handle(request, default);

            Assert.IsTrue(response.Valid);
            Assert.AreEqual(players.Count(), response.Items?.Length);
        }

        private static Fixture CreateAgentFixture(Guid brandId, string username)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Entities.Agent>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Username, username)
                .With(s => s.Brand, new Brand())
                .With(s => s.Players, new Repository.Entities.Player[]{}));
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

        private static Fixture CreatePlayerBonusAggregateFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerBonusAggregate>(u => u
                .With(s => s.BrandId, brandId));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}