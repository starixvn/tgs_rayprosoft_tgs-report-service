using Microsoft.AspNetCore.Mvc;
using Tgs.Report.Client;
using Tgs.Report.Client.Models.ResellerMarket;

namespace Tgs.Report.ClientSample.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ResellerController : ControllerBase
    {
        private readonly ILogger<ResellerController> _logger;
        private readonly IReportClient _client;

        public ResellerController(ILogger<ResellerController> logger, IReportClient client)
        {
            _logger = logger;
            _client = client;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public async Task<ResellerMarketsResponse> Get(Guid resellerId) => await _client.GetResellerMarketsAsync(
            new GetResellerMarketsRequest
            {
                CurrencyCode = null,
                CurrencyCodes = null,
                ResellerId = resellerId,
                From = new DateTimeOffset(2022, 1, 1, 0, 0, 0, new(8, 0, 0)),
                To = DateTimeOffset.Now,
                Page = 1,
                Rows = 20,
                OrderBy = 1,
                SortBy = "currencyCode",
                ShowAll = false
            });
    }
}