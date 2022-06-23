using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Round;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.UnitTest.Round
{
    internal class GetRoundsTotalQueryTest
    {
        [Test]
        public async Task Can_Get_Rounds_Sum()
        {
            var dbContextOptionsBuilder = new DbContextOptionsBuilder<TgsContext>();
            dbContextOptionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning));

            var context = new TgsContext(dbContextOptionsBuilder.Options);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var roundId = new Guid("aac02111-20f6-4f0d-6982-08d9dbdc0473");
            var gameProviderId = new Guid("aac02112-20f6-4f0d-6982-08d9dbdc0473");
            var gameId = 1;
            var playerCode = "PlayerCode";

            _ = context.Games.Add(new Game
            {
                Id = gameId,
                GameProviderId = gameProviderId,
                AvailableCurrencies = "",
                Code = "Lobby",
                CreatedByUsername = "username",
                ExternalId = "externalId",
                Name = "Lobby",
                UpdatedByUsername = "username"
            });

            _ = context.Players.Add(new Repository.Entities.Player
            {
                BrandId = brandId,
                PlayerCode = playerCode,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "username",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            });

            _ = context.Rounds.Add(new Repository.Entities.Round
            {
                Id = roundId,
                GameId = gameId,
                BrandId = brandId,
                GameProviderId = gameProviderId,
                BetAmount = 10m,
                Turnover = 10m,
                WinLoss = -15m,
                CreatedOn = DateTime.UtcNow,
                Status = (short)RoundStatus.Open,
                PlayerCode = playerCode,
                CurrencyCode = "CNY",
                ExternalRoundId = "externalRoundId"
            });

            _ = context.RoundDetails.Add(new RoundDetail { RoundId = roundId, AdditionalData = "{}" });

            _ = await context.SaveChangesAsync();

            var sut = new GetRoundsTotalQueryHandler(context);

            var request = new GetRoundsTotalRequest
            {
                BrandId = brandId,
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1)
            };

            var response = await sut.Handle(request, default);

            Assert.AreEqual(10, response.BetAmount);
            Assert.AreEqual(10, response!.ValidBetAmount);
            Assert.AreEqual(15, response!.WinLoss);
        }
    }
}