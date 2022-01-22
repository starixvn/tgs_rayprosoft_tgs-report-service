using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Player;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.UnitTest.Player
{
    [TestFixture]
    public sealed class GetPlayersAccountChangeQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Player_Contributions()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var sut = new GetPlayersAccountChangeQueryHandler(fakeRepository);

            var brandId = Guid.NewGuid();
            var userId = Guid.NewGuid();
            var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

            var transactions = CreateTransactionFixture(brandId, userId).CreateMany<Transaction>(1).AsQueryable()
                .BuildMock().Object;

            var players = CreatePlayerFixture(brandId).CreateMany<Repository.Configurations.Player>(1).AsQueryable()
                .BuildMock().Object;

            var users = CreateUserFixture(userId).CreateMany<User>(1).AsQueryable()
                .BuildMock().Object;

            var request = new GetPlayersAccountChangesRequest
            {
                BrandId = brandId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1)
            };

            var mockTransactions = GetDbSet(transactions);
            var mockPlayers = GetDbSet(players);
            var mockUsers = GetDbSet(users);

            fakeRepository.Transactions.Returns(mockTransactions.Object);
            fakeRepository.Players.Returns(mockPlayers.Object);
            fakeRepository.Users.Returns(mockUsers.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.Items.Length);
        }

        private static Fixture CreateTransactionFixture(Guid brandId, Guid userId)
        {
            var fixture = new Fixture();
            fixture.Customize<Transaction>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.Brand, new Brand())
                .With(s => s.PlayerCode, "PlayerCode")
                .With(s => s.Type, (short)TransactionType.Adjust)
                .With(s => s.CreatedByUserId, userId)
                .With(s => s.Description, JsonSerializer.Serialize(new AdjustRemarkModel()))
                .With(s => s.RecordedOn, DateTimeOffset.UtcNow));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreatePlayerFixture(Guid brandId)
        {
            var fixture = new Fixture();
            fixture.Customize<Repository.Configurations.Player>(u => u
                .With(s => s.BrandId, brandId)
                .With(s => s.PlayerCode, "PlayerCode")
                .With(s => s.Brand, new Brand()));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }

        private static Fixture CreateUserFixture(Guid userId)
        {
            var fixture = new Fixture();
            fixture.Customize<User>(u => u
                .With(s => s.Id, userId));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}