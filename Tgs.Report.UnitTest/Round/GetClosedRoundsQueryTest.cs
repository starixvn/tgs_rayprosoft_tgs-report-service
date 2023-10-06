using System;
using System.Linq;
using System.Threading.Tasks;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Round;
using Tgs.Report.Enum;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.UnitTest.Round;

[TestFixture]
[Parallelizable]
public class GetClosedRoundsQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Closed_Rounds()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var gameProviderId = Guid.NewGuid();
        var roundId = Guid.NewGuid();
        var brandId = Guid.NewGuid();
        var playerCode = "playerCode";
        var externalRoundId = "externalRoundId";

        var rounds = new[]
{
            new Repository.Entities.Round
            {
                Id = roundId,
                BrandId = brandId,
                CurrencyCode = "CNY",
                PlayerCode = playerCode,
                ExternalRoundId = externalRoundId,
                Status = (short)RoundStatus.Closed,
                GameProviderId = gameProviderId,
                GameId = 1,
                ExternalCreatedOn = DateTime.UtcNow,
                ExternalClosedOn = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                ClosedOn = DateTime.UtcNow,
                Turnover = 1
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

        var players = new[]
        {
            new Repository.Entities.Player
            {
                PlayerCode = playerCode,
                Brand = new Brand{Code = "code", Name = "name"},
                Type = (short)PlayerType.RealPlayer,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "username",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>()
            }
        }.AsQueryable().BuildMock();

        var games = new[]
        {
            new Game
            {
                Id = 1,
                GameProviderId= gameProviderId,
                GameProvider = new Repository.Entities.GameProvider{ Code = "code"},
                SubGameProviderCodeNavigation = new SubGameProvider{Name = "name"},
                AvailableCurrencies = "CNY",
                Code = "TF",
                CreatedByUsername = "username",
                ExternalId = "1",
                Name = "name",
                UpdatedByUsername = "username"
            }
        }.AsQueryable().BuildMock();

        var gameProviders = new[]
        {
            new Repository.Entities.GameProvider
            {
                Id = gameProviderId
            }
        }.AsQueryable().BuildMock();

        var brandGames = new[]
        {
            new BrandGame
            {
                BrandId = brandId,
                GameId = 1,
                EffectiveBettingPercentage = 1,
            }
        }.AsQueryable().BuildMock();

        var mockRounds = GetDbSet(rounds);
        var mockRoundDetails = GetDbSet(roundDetails);
        var mockPlayers = GetDbSet(players);
        var mockGames = GetDbSet(games);
        var mockGameProviders = GetDbSet(gameProviders);
        var mockBrandGames = GetDbSet(brandGames);

        fakeDbContext.Rounds.Returns(mockRounds.Object);
        fakeDbContext.RoundDetails.Returns(mockRoundDetails.Object);
        fakeDbContext.Players.Returns(mockPlayers.Object);
        fakeDbContext.Games.Returns(mockGames.Object);
        fakeDbContext.GameProviders.Returns(mockGameProviders.Object);
        fakeDbContext.BrandGames.Returns(mockBrandGames.Object);

        var sut = new GetClosedRoundsQueryHandler(fakeDbContext);

        var request = new GetClosedRoundsRequest
        {
            GameProviderId = gameProviderId,
            ExternalRoundId = externalRoundId
        };

        var actual = await sut.Handle(request, default);

        Assert.AreEqual(1, actual.Items!.Length);
    }
}