using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Round;
using Tgs.Report.Enum;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.UnitTest.Round
{
    [TestFixture]
    public class GetClosedRoundsQueryTest
    {
        [Test]
        public async Task Can_Get_Closed_Rounds()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsRepository>();

            dbContextOptionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));
            ITgsRepository repository = new TgsRepository(dbContextOptionsBuilder.Options);

            var gameProviderId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            var playerCode = "playerCode";
            var externalRoundId = "externalRoundId";
            await repository.Rounds.AddAsync(new Repository.Entities.Round
            {
                Id = roundId,
                CurrencyCode = "CNY",
                PlayerCode = playerCode,
                ExternalRoundId = externalRoundId,
                Status = (short)RoundStatus.Closed,
                GameProviderId = gameProviderId
            });

            await repository.RoundDetails.AddAsync(new RoundDetail
            {
                RoundId = roundId,
                AdditionalData = "additionalData"
            });

            await repository.Players.AddAsync(new Repository.Entities.Player
            {
                PlayerCode = playerCode,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "username",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

            await repository.Games.AddAsync(new Game
            {
                AvailableCurrencies = "CNY",
                Code = "TF",
                CreatedByUsername = "username",
                ExternalId = "1",
                Name = "name",
                UpdatedByUsername = "username"
            });

            await repository.SaveChangesAsync();

            var sut = new GetClosedRoundsQueryHandler(repository);

            var request = new GetClosedRoundsRequest
            {
                GameProviderId = gameProviderId,
                ExternalRoundId = externalRoundId
            };

            var actual = await sut.Handle(request, default);

            Assert.AreEqual(1, actual.Items!.Length);
        }
    }
}