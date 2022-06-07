using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MockQueryable.Moq;
using NSubstitute;
using Tgs.Report.Client;
using Tgs.Report.Repository;
using Tgs.Report.Repository.Entities;

namespace Tgs.Report.IntegrationTest;

[TestClass]
public class MarketingTests
{
	[TestMethod]
	public async Task Can_Get_Marketing_List()
	{
		await using var web = new TestReportApplication();

		var httpClient = web.CreateClient();

		var sut = new ReportClient(httpClient);

		var brandId = Guid.NewGuid();

        var actual = await sut.GetMarketingListAsync(new Client.Models.Marketing.GetMarketingListRequest
		{
			BrandId = brandId,
            AgentUsername = "agentUsername",
            From = DateTimeOffset.Now,
            To = DateTimeOffset.Now
		});

		Assert.IsNotNull(actual);
	}

    [TestMethod]
    public async Task Can_Get_Marketing_Register_List()
    {
        await using var web = new TestReportApplication();

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var brandId = Guid.NewGuid();

        var actual = await sut.GetMarketingRegisterDetailsAsync(new Client.Models.Marketing.GetMarketingRegisterDetailRequest
        {
            BrandId = brandId,
            AgentUsername = "agentUsername",
            Date = DateTimeOffset.Now
        });

        Assert.IsNotNull(actual);
    }
    
    [TestMethod]
    public async Task Can_Get_Marketing_First_Deposit_List()
    {
        await using var web = new TestReportApplication();

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var brandId = Guid.NewGuid();

        var actual = await sut.GetMarketingFirstDepositDetailsAsync(new Client.Models.Marketing.GetMarketingFirstDepositDetailRequest
        {
            BrandId = brandId,
            AgentUsername = "agentUsername",
            Date = DateTimeOffset.Now
        });

        Assert.IsNotNull(actual);
    }

    [TestMethod]
    public async Task Can_Get_Marketing_Gaming_List()
    {
        await using var web = new TestReportApplication();

        var httpClient = web.CreateClient();

        var sut = new ReportClient(httpClient);

        var brandId = Guid.NewGuid();

        var actual = await sut.GetMarketingGamingDetailsAsync(new Client.Models.Marketing.GetMarketingGamingDetailRequest
        {
            BrandId = brandId,
            AgentUsername = "agentUsername",
            Date = DateTimeOffset.Now
        });

        Assert.IsNotNull(actual);
    }
}
