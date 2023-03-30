using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Round;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.UnitTest.Round;

internal class GetRoundsTotalQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Rounds_Sum()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
        var roundId = new Guid("aac02111-20f6-4f0d-6982-08d9dbdc0473");
        var gameProviderId = new Guid("aac02112-20f6-4f0d-6982-08d9dbdc0473");
        var gameId = 1;
        var playerCode = "PlayerCode";

        var games = new[]
        {
            new Game
            {
                Id = gameId,
                GameProviderId = gameProviderId,
                AvailableCurrencies = "",
                Code = "Lobby",
                CreatedByUsername = "username",
                ExternalId = "externalId",
                Name = "Lobby",
                UpdatedByUsername = "username"
            }
        }.AsQueryable().BuildMock();

        var players = new[]
        {
            new Repository.Entities.Player
            {
                BrandId = brandId,
                PlayerCode = playerCode,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "username",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            }
        }.AsQueryable().BuildMock();

        var rounds = new[]
        {
            new Repository.Entities.Round
            {
                Id = roundId,
                GameId = gameId,
                BrandId = brandId,
                GameProviderId = gameProviderId,
                BetAmount = 10m,
                ValidBetAmount = 10m,
                Turnover = 10m,
                WinLoss = -15m,
                CreatedOn = DateTime.UtcNow,
                Status = (short)RoundStatus.Open,
                PlayerCode = playerCode,
                CurrencyCode = "CNY",
                ExternalRoundId = "externalRoundId"
            }
        }.AsQueryable().BuildMock();

        var roundDetails = new[]
        {
            new RoundDetail
            {
                RoundId = roundId,
                AdditionalData = "additionalData"
            }
        }.AsQueryable().BuildMock();

        var mockRounds = GetDbSet(rounds);
        var mockRoundDetails = GetDbSet(roundDetails);
        var mockPlayers = GetDbSet(players);
        var mockGames = GetDbSet(games);

        fakeDbContext.Rounds.Returns(mockRounds.Object);
        fakeDbContext.RoundDetails.Returns(mockRoundDetails.Object);
        fakeDbContext.Players.Returns(mockPlayers.Object);
        fakeDbContext.Games.Returns(mockGames.Object);

        var sut = new GetRoundsTotalQueryHandler(fakeDbContext);

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