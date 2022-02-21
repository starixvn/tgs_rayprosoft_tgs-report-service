using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Tgs.Report.Client.Models;
using Tgs.Report.Client.Models.Agent;
using Tgs.Report.Client.Models.GameAction;
using Tgs.Report.Client.Models.GameProvider;
using Tgs.Report.Client.Models.LicenseeMarket;
using Tgs.Report.Client.Models.Market;
using Tgs.Report.Client.Models.Player;
using Tgs.Report.Client.Models.Round;
using Tgs.Report.Client.Models.Vip;

namespace Tgs.Report.Client
{
    public class ReportClient : IReportClient
    {
        private readonly HttpClient _client;

        public ReportClient(
            HttpClient client)
        {
            _client = client;
        }

        public async ValueTask<PagedResponse<VipModel>> GetVipReportAsync(GetVipReportRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/vipReport?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<VipModel>>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<VipRebateModel>> GetVipRebateReportAsync(
            GetVipRebateReportRequest request)
        {
            var path = ClientPath.AppendWithProperties($"/api/vipReport/{request.Username}?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<VipRebateModel>>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<RoundModel>> GetRoundsAsync(GetRoundsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/roundReport?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<RoundModel>>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<OpenRoundModel>> GetOpenRoundsAsync(GetOpenRoundsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/roundReport/openRounds?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<OpenRoundModel>>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<ClosedRoundModel>> GetClosedRoundsAsync(GetClosedRoundsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/roundReport/closedRounds?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<ClosedRoundModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<GetRoundHistoryResponse> GetRoundHistoriesAsync(GetRoundHistoryRequest request)
        {
            var path = ClientPath.AppendWithProperties($"/api/roundReport/{request.RoundId}?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<GetRoundHistoryResponse>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<MarketModel>> GetMarketsAsync(GetMarketsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/marketReport?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<MarketModel>>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<AgentContributionModel>> GetAgentContributionsAsync(
            GetAgentContributionsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/agentReport/contribution?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<AgentContributionModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<PlayerContributionModel>> GetPlayerContributionsAsync(
            GetPlayerContributionsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/playerReport/contribution?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<PlayerContributionModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<TeamInfoModel>> GetAgentTeamInfoAsync(
            GetAgentTeamInfoRequest request)
        {
            var path = ClientPath.AppendWithProperties($"/api/agentReport/{request.AgentId}/teamInfo?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<TeamInfoModel>>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<AgentDividendModel>> GetAgentDividendsAsync(
            GetAgentDividendsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/agentReport/dividends?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<AgentDividendModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<AgentPlayerWinLossModel>> GetAgentPlayerWinLossAsync(
            GetAgentPlayerWinLossRequest request)
        {
            var path = ClientPath.AppendWithProperties($"/api/agentReport/{request.AgentId}/winLoss?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<AgentPlayerWinLossModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<AgentDividendDetailModel>> GetAgentDividendDetailsAsync(
            GetAgentDividendDetailsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/agentReport/dividendDetails?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<AgentDividendDetailModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<PlayerAccountChangeModel>> GetPlayerAccountChangesAsync(
            GetPlayerAccountChangesRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/playerReport/accountChanges?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<PlayerAccountChangeModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<PlayersAccountChangeModel>> GetPlayersAccountChangesAsync(
            GetPlayersAccountChangesRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/playerReport/playersAccountChanges?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<PlayersAccountChangeModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<AgentAccountChangeModel>> GetAgentAccountChangesAsync(
            GetAgentAccountChangesRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/agentReport/accountChanges?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<AgentAccountChangeModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<LicenseeMarketsResponse> GetLicenseeMarketsAsync(GetLicenseeMarketsRequest request)
        {
            var prefix = $"Report.LicenseeId.{request.LicenseeId}.GetLicenseeMarkets";

            var keyValues = new Dictionary<string, string>();
            foreach (var propertyInfo in request
                         .GetType()
                         .GetProperties()
                         .Where(x => x != null && !x.Name.Contains("CurrencyCodes",
                             StringComparison.InvariantCultureIgnoreCase))
                         .ToArray())
            {
                var value = propertyInfo.GetValue(request);

                if (value == null)
                {
                    continue;
                }

                if (propertyInfo.PropertyType == typeof(DateTimeOffset)
                    || propertyInfo.PropertyType == typeof(DateTimeOffset?))
                {
                    value = ((DateTimeOffset?)value)?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sssZ");
                }

                keyValues.Add(propertyInfo.Name, value.ToString());
            }

            var parameters = string.Join("&", keyValues.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));

            var currencyCodes = request.CurrencyCodes
                ?.Select(currencyCode => string.Format("{0}={1}", "currencyCodes", currencyCode)).ToArray();

            var currencyCodeParameters = currencyCodes == null ? string.Empty : string.Join("&", currencyCodes);

            parameters += string.IsNullOrEmpty(parameters) ? currencyCodeParameters : $"&{currencyCodeParameters}";

            var path = $"/api/LicenseeMarketReport?{parameters}";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<LicenseeMarketsResponse>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<Stream> ExportMarketsAsync(ExportMarketsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/marketReport/exportMarkets?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            return stream;
        }

        public async ValueTask<Stream> ExportPlayerContributionsAsync(ExportPlayerContributionsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/playerReport/exportPlayerContributions?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            return stream;
        }

        public async ValueTask<Stream> ExportAgentContributionsAsync(ExportAgentContributionsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/agentReport/exportAgentContributions?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            return stream;
        }

        public async ValueTask<Stream> ExportAgentDividendsAsync(ExportAgentDividendsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/agentReport/exportAgentDividends?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            return stream;
        }

        public async ValueTask<Stream> ExportVipsAsync(ExportVipReportRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/vipReport/exportVips?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            return stream;
        }

        public async ValueTask<Stream> ExportAgentDividendsTrialCalculationAsync(
            ExportAgentDividendsTrialCalculationRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/agentReport/exportAgentDividendsTrialCalculation?",
                request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            return stream;
        }

        public async ValueTask<Stream> ExportLicenseeMarketsAsync(ExportLicenseeMarketsRequest request)
        {
            var keyValues = new Dictionary<string, string>();
            foreach (var propertyInfo in request
                         .GetType()
                         .GetProperties()
                         .Where(x => x != null && !x.Name.Contains("CurrencyCodes",
                             StringComparison.InvariantCultureIgnoreCase))
                         .ToArray())
            {
                var value = propertyInfo.GetValue(request);

                if (value == null)
                {
                    continue;
                }

                if (propertyInfo.PropertyType == typeof(DateTimeOffset)
                    || propertyInfo.PropertyType == typeof(DateTimeOffset?))
                {
                    value = ((DateTimeOffset?)value)?.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.sssZ");
                }

                keyValues.Add(propertyInfo.Name, value.ToString());
            }

            var parameters = string.Join("&", keyValues.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));

            var currencyCodes = request.CurrencyCodes
                ?.Select(currencyCode => string.Format("{0}={1}", "currencyCodes", currencyCode)).ToArray();

            var currencyCodeParameters = currencyCodes == null ? string.Empty : string.Join("&", currencyCodes);

            parameters += string.IsNullOrEmpty(parameters) ? currencyCodeParameters : $"&{currencyCodeParameters}";

            var path = $"/api/LicenseeMarketReport/exportLicenseeMarkets?{parameters}";

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

            httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

            var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync();

            return stream;
        }

        public async ValueTask<GetGameProviderRoundsResponse> GetGameProviderRoundsAsync(
            GetGameProviderRoundsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/gameProviderReport?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<GetGameProviderRoundsResponse>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<AgentDividendTeamDetailModel>> GetAgentDividendTeamDetailsAsync(
            GetAgentDividendTeamDetailsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/agentReport/dividendTeamDetails?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<AgentDividendTeamDetailModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<GetAdditionalExpensesResponse> GetAdditionalExpensesAsync(
            GetAdditionalExpensesRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/marketReport/additionalExpenses?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<GetAdditionalExpensesResponse>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<GameActionModel>> GetGameActionsAsync(GetGameActionsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/gameActionReport?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<GameActionModel>>(
                await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }

        public async ValueTask<PagedResponse<RoundModel>> GetAgentPlayerRoundsAsync(GetAgentPlayerRoundsRequest request)
        {
            var path = ClientPath.AppendWithProperties("/api/roundReport/agentPlayerRounds?", request);

            var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

            _ = response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<PagedResponse<RoundModel>>(await response.Content.ReadAsStringAsync(),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        }
    }
}