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

[TestFixture]
public class GetAgentPlayerRoundsQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Agent_Belong_Player_Rounds()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var agentId = Guid.NewGuid();

        var playerCode1 = "playerCode";
        var playerCode2 = "playerCode2";

        var rounds = new[]
        {
            new Repository.Entities.Round
            {
                Id = Guid.Parse("7284B324-7065-CDD8-87A4-69A17C5DF20D"),
                CurrencyCode = "CNY",
                PlayerCode = playerCode1,
                ExternalRoundId = "externalRoundId1",
                Status = (short)RoundStatus.Closed,
                GameProviderId = Guid.Parse("3924FF4E-8ECA-A25F-ABDC-9B8606A46316"),
                ExternalCreatedOn = DateTime.UtcNow,
                ExternalClosedOn = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                ClosedOn = DateTime.UtcNow
            },
            new Repository.Entities.Round
            {
                Id = Guid.Parse("DBE0C089-A956-4F14-B325-125F7935CB30"),
                CurrencyCode = "CNY",
                PlayerCode = playerCode2,
                ExternalRoundId = "externalRoundId2",
                Status = (short)RoundStatus.Open,
                GameProviderId = Guid.Parse("3924FF4E-8ECA-A25F-ABDC-9B8606A46316"),
                ExternalCreatedOn = DateTime.UtcNow,
                ExternalClosedOn = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                ClosedOn = DateTime.UtcNow
            }
        }.AsQueryable().BuildMock();

        var roundDetails = new[]
        {
            new RoundDetail
            {
                RoundId = Guid.Parse("7284B324-7065-CDD8-87A4-69A17C5DF20D"),
                AdditionalData = "additionalData"
            }
        }.AsQueryable().BuildMock();

        var players = new[]
        {
            new Repository.Entities.Player
            {
                PlayerCode = playerCode1,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "username1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                AgentId = agentId
            },
            new Repository.Entities.Player
            {
                PlayerCode = playerCode2,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "username2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                AgentId = agentId
            }
        }.AsQueryable().BuildMock();

        var games = new[]
        {
            new Game
            {
                Id = 1,
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
                Id = Guid.Parse("3924FF4E-8ECA-A25F-ABDC-9B8606A46316")
            }
        }.AsQueryable().BuildMock();

        var brandGames = new[]
        {
            new BrandGame
            {
                BrandId = Guid.NewGuid(),
                GameId = 1
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

        var sut = new GetAgentPlayerRoundsQueryHandler(fakeDbContext);

        var request = new GetAgentPlayerRoundsRequest { AgentId = agentId };

        //Act
        var actual = await sut.Handle(request, default);

        //Assert
        Assert.AreEqual(2, actual.Items!.Length);
    }

    [Test]
    public async Task Can_Not_Get_Rounds_By_Other_Agent()
    {
        var fakeDbContext = Substitute.For<TgsContext>();

        var playerCode1 = "playerCode";
        var playerCode2 = "playerCode2";


        var rounds = new[]
        {
            new Repository.Entities.Round
            {
                Id = Guid.Parse("7284B324-7065-CDD8-87A4-69A17C5DF20D"),
                CurrencyCode = "CNY",
                PlayerCode = playerCode1,
                ExternalRoundId = "externalRoundId1",
                Status = (short)RoundStatus.Closed,
                GameProviderId = Guid.Parse("3924FF4E-8ECA-A25F-ABDC-9B8606A46316")
            }, new Repository.Entities.Round
            {
                Id = Guid.Parse("DBE0C089-A956-4F14-B325-125F7935CB30"),
                CurrencyCode = "CNY",
                PlayerCode = playerCode2,
                ExternalRoundId = "externalRoundId2",
                Status = (short)RoundStatus.Open,
                GameProviderId = Guid.Parse("3924FF4E-8ECA-A25F-ABDC-9B8606A46316")
            }
        }.AsQueryable().BuildMock();

        var roundDetails = new[]
        {
            new RoundDetail
            {
                RoundId = Guid.Parse("7284B324-7065-CDD8-87A4-69A17C5DF20D"),
                AdditionalData = "additionalData"            }
        }.AsQueryable().BuildMock();

        var players = new[]
        {
            new Repository.Entities.Player
            {
                PlayerCode = playerCode1,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "username1",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                AgentId = Guid.Parse("50DD70BE-38F8-6724-E446-BF1380E00B64")
            }, new Repository.Entities.Player
            {
                PlayerCode = playerCode2,
                DefaultWalletCurrencyCode = "CNY",
                Password = Array.Empty<byte>(),
                Salt = Array.Empty<byte>(),
                Username = "username2",
                WithdrawPassword = Array.Empty<byte>(),
                WithdrawSalt = Array.Empty<byte>(),
                AgentId = Guid.Parse("50DD70BE-38F8-6724-E446-BF1380E00B64")
            }        }.AsQueryable().BuildMock();

        var games = new[]
        {
            new Game
            {
                Id = 1,
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
                Id = Guid.Parse("3924FF4E-8ECA-A25F-ABDC-9B8606A46316")
            }
        }.AsQueryable().BuildMock();

        var brandGames = new[]
        {
            new BrandGame
            {
                BrandId = Guid.NewGuid(),
                GameId = 1
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


        var sut = new GetAgentPlayerRoundsQueryHandler(fakeDbContext);

        var request = new GetAgentPlayerRoundsRequest { AgentId = Guid.Parse("0FC83890-6802-54F5-FB74-BADE02BDE7A9") };

        //Act
        var actual = await sut.Handle(request, default);

        //Assert
        Assert.AreEqual(0, actual.Items!.Length);
    }
}