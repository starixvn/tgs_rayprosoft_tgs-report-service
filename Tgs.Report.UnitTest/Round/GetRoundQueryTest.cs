using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AutoFixture;
using MockQueryable.Moq;
using Moq;
using Moq.Protected;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core.Queries.Round;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.UnitTest.Round
{
    [TestFixture]
    public sealed class GetRoundQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Open_Rounds()
        {
            var fakeRepository = Substitute.For<ITgsRepository>();

            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            var httpClient = new HttpClient(mockHttpMessageHandler.Object);

            httpClient.BaseAddress = new Uri("http://test");

            var sut = new GetRoundQueryHandler(fakeRepository, httpClient);

            var roundId = new Guid("aac02111-20f6-4f0d-6982-08d9dbdc0473");

            const string gameProviderCode = "key";

            var settings = CreateSettingFixture(gameProviderCode).CreateMany<Setting>(1).AsQueryable()
                .BuildMock().Object;

            var httpResponse = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"gpname\":\"GameProviderName\"}")
            };

            mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(httpResponse);

            var request = new GetRoundHistoryRequest
            {
                RoundId = roundId,
                GameProviderCode = gameProviderCode
            };

            var mockSettings = GetDbSet(settings);

            fakeRepository.Settings.Returns(mockSettings.Object);

            var response = await sut.Handle(request, default);

            Assert.AreEqual("GameProviderName", response.GameProviderName);
        }

        private static Fixture CreateSettingFixture(string gameProviderCode)
        {
            var fixture = new Fixture();
            fixture.Customize<Setting>(u => u
                .With(s => s.Key, $"{gameProviderCode}Configuration")
                .With(s => s.Value, "{\"ServiceEndpointUrl\":\"http://url\"}"));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}