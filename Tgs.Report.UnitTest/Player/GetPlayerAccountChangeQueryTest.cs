using System;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using NSubstitute;
using NUnit.Framework;
using Tgs.Report.Core;
using Tgs.Report.Core.Queries.Player;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.UnitTest.Player
{
    [TestFixture]
    public sealed class GetPlayerAccountChangeQueryTest : BaseTest
    {
        [Test]
        public async Task Can_Get_Player_Account_Change()
        {
            var fakeRepository = Substitute.For<IPlayerFactory>();

            var sut = new GetPlayerAccountChangeQueryHandler(fakeRepository);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");
            var reportDate = new DateTimeOffset(2022, 1, 21, 0, 0, 0, new TimeSpan(8, 0, 0));

            var request = new GetPlayerAccountChangesRequest
            {
                BrandId = brandId,
                PlayerCode = "PlayerCode",
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1)
            };

            var models = CreatePlayerAccountChangeModelFixture(reportDate).CreateMany<PlayerAccountChangeModel>(1)
                .AsEnumerable();

            _ = fakeRepository.GetPlayerAccountChangeAsync(request).Returns((models.ToArray(), 1));

            var response = await sut.Handle(request, default);

            Assert.AreEqual(1, response.TotalCount);
        }

        [Test]
        public async Task Can_Get_Player_Account_Change_Total_Amount()
        {
            var fakeRepository = Substitute.For<IPlayerFactory>();

            var sut = new GetPlayerAccountChangeTotalAmountQueryHandler(fakeRepository);

            var brandId = new Guid("aac02420-20f6-4f0d-6982-08d9dbdc0473");

            var request = new GetPlayerAccountChangesTotalAmountRequest
            {
                BrandId = brandId,
                PlayerCode = "PlayerCode",
                From = DateTimeOffset.UtcNow.AddDays(-1),
                To = DateTimeOffset.UtcNow.AddDays(1)
            };

            _ = await sut.Handle(request, default);

            _ = fakeRepository
                .Received(1)
                .GetPlayerAccountChangeTotalAmountAsync(request);
        }

        private static Fixture CreatePlayerAccountChangeModelFixture(DateTimeOffset date)
        {
            var fixture = new Fixture();
            fixture.Customize<PlayerAccountChangeModel>(u => u
                .With(s => s.RecordedOn, date));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
            return fixture;
        }
    }
}