using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core;
using Tgs.Report.Core.Models;
using Tgs.Report.Core.Queries.Player;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.UnitTest.Player;

[TestFixture]
[Parallelizable]
public sealed class GetPlayersAccountChangeQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Players_Account_Change()
    {
        var fakePlayerFactory = Substitute.For<IPlayerFactory>();

        var sut = new GetPlayersAccountChangesQueryHandler(fakePlayerFactory);

        var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");

        var fixture = new Fixture();

        var playerAccountChanges = fixture.CreateMany<PlayersAccountChangeModel>(20).ToArray();

        _ = fakePlayerFactory.GetPlayersAccountChangesQueryable(Arg.Any<GetPlayersAccountChangesQuery>())
            .Returns(playerAccountChanges.AsQueryable().BuildMock());

        var request = new GetPlayersAccountChangesRequest
        {
            BrandId = brandId,
            From = DateTimeOffset.UtcNow.AddDays(-1),
            To = DateTimeOffset.UtcNow.AddDays(1)
        };

        var response = await sut.Handle(request, default);

        Assert.That(response.Items?.Length, Is.EqualTo(20));
    }
}