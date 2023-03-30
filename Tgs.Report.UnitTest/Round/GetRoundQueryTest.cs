using System;
using System.Text.Json;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.Report.Core.Queries.Round;
using Tgs.Report.Repository.Entities;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.UnitTest.Round;

[TestFixture]
public sealed class GetRoundQueryTest : BaseTest
{
    [Test]
    public async Task Can_Get_Round()
    {
        var fakeDbContext = Substitute.For<TgsContext>();
        var fakeClient = Substitute.For<IGameProvidersBackofficePluggableClient>();

        var sut = new GetRoundQueryHandler(fakeClient);

        var roundId = new Guid("aac02111-20f6-4f0d-6982-08d9dbdc0473");

        const string gameProviderCode = "key";

        var request = new GetRoundHistoryRequest
        {
            RoundId = roundId,
            GameProviderCode = gameProviderCode
        };

        var json = JsonDocument.Parse("[{\"type\":3,\"value\":\"test\"}]");

        fakeClient.GameRoundDetailGetAsync(Arg.Any<Guid>(), Arg.Any<string>()).Returns(json);

        var response = await sut.Handle(request, default);

        Assert.IsNotNull(response);
    }
}