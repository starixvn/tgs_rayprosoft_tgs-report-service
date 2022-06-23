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
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();

            dbContextOptionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            var context = new TgsContext(dbContextOptionsBuilder.Options);

            var gameProviderId = Guid.NewGuid();
            var roundId = Guid.NewGuid();
            var playerCode = "playerCode";
            var externalRoundId = "externalRoundId";
            await context.Rounds.AddAsync(new Repository.Entities.Round
            {
                Id = roundId,
                CurrencyCode = "CNY",
                PlayerCode = playerCode,
                ExternalRoundId = externalRoundId,
                Status = (short)RoundStatus.Closed,
                GameProviderId = gameProviderId,
                ExternalCreatedOn = DateTime.UtcNow,
                ExternalClosedOn = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                ClosedOn = DateTime.UtcNow
            });

            await context.RoundDetails.AddAsync(new RoundDetail
            {
                RoundId = roundId,
                AdditionalData = "additionalData"
            });

            await context.Players.AddAsync(new Repository.Entities.Player
            {
                PlayerCode = playerCode,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "username",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

            await context.Games.AddAsync(new Game
            {
                AvailableCurrencies = "CNY",
                Code = "TF",
                CreatedByUsername = "username",
                ExternalId = "1",
                Name = "name",
                UpdatedByUsername = "username"
            });

            await context.SaveChangesAsync();

            var sut = new GetClosedRoundsQueryHandler(context);

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