using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using Tgs.Report.Client.Models;
using Tgs.Report.Client.Models.Agent;
using Tgs.Report.Client.Models.AgentWinLoss;
using Tgs.Report.Client.Models.BrandGameProvider;
using Tgs.Report.Client.Models.GameAction;
using Tgs.Report.Client.Models.GameProvider;
using Tgs.Report.Client.Models.LicenseeMarket;
using Tgs.Report.Client.Models.Market;
using Tgs.Report.Client.Models.Marketing;
using Tgs.Report.Client.Models.Player;
using Tgs.Report.Client.Models.PlayerDeposit;
using Tgs.Report.Client.Models.ResellerMarket;
using Tgs.Report.Client.Models.Round;
using Tgs.Report.Client.Models.Vip;

namespace Tgs.Report.Client;

public class ReportClient : IReportClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public ReportClient(
        HttpClient client)
        => _client = client;

    public async ValueTask<PagedResponse<VipModel>> GetVipReportAsync(GetVipReportRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/vipReport?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<VipModel>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<VipRebateModel>> GetVipRebateReportAsync(
        GetVipRebateReportRequest request)
    {
        var path = ClientPath.AppendWithProperties($"/api/vipReport/{request.Username}?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<VipRebateModel>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<RoundModel>> GetRoundsAsync(GetRoundsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/roundReport?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<RoundModel>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<SystemRoundModel>> GetSystemRoundsAsync(GetSystemRoundsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/roundReport/system?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<SystemRoundModel>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<Stream> ExportSystemRoundsAsync(ExportSystemRoundsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/roundReport/exportSystemRounds?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

        httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

        var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        _ = response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        return stream;
    }

    public async ValueTask<RoundTotalModel> GetRoundsTotalAsync(GetRoundsTotalRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/roundReport/roundsTotal?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<RoundTotalModel>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<SystemRoundTotalModel> GetSystemRoundsTotalAsync(GetSystemRoundsTotalRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/roundReport/systemRoundsTotal?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<SystemRoundTotalModel>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<GetBetRecordsResponse> GetBetRecordsAsync(GetBetRecordsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/roundReport/betRecords?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<GetBetRecordsResponse>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<OpenRoundModel>> GetOpenRoundsAsync(GetOpenRoundsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/roundReport/openRounds?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<OpenRoundModel>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<ClosedRoundModel>> GetClosedRoundsAsync(GetClosedRoundsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/roundReport/closedRounds?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<ClosedRoundModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<JsonDocument?> GetRoundHistoriesAsync(GetRoundHistoryRequest request)
    {
        var path = ClientPath.AppendWithProperties($"/api/roundReport/{request.RoundId}?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<JsonDocument?>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<MarketModel>> GetMarketsAsync(GetMarketsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/marketReport?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<MarketModel>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<AgentContributionModel>> GetAgentContributionsAsync(
        GetAgentContributionsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentReport/contribution?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

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

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<PlayerContributionModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<AgentDividendModel>> GetAgentDividendsAsync(
        GetAgentDividendsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentReport/dividends?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<AgentDividendModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<AgentDividendModel>> GetSingleAgentDividendsAsync(
        GetSingleAgentDividendsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentReport/singleAgentDividends?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PagedResponse<AgentDividendModel>>(_jsonSerializerOptions))!;
    }

    public async ValueTask<PagedResponse<AgentDividendModel>> GetSubAgentDividendsAsync(
        GetSubAgentDividendsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentReport/subAgentDividends?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<PagedResponse<AgentDividendModel>>(_jsonSerializerOptions))!;
    }

    public async ValueTask<PagedResponse<AgentPlayerWinLossModel>> GetAgentPlayerWinLossAsync(
        GetAgentPlayerWinLossRequest request)
    {
        var path = ClientPath.AppendWithProperties($"/api/agentReport/{request.AgentId}/winLoss?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

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

        _ = response.EnsureSuccessStatusCode();

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

        _ = response.EnsureSuccessStatusCode();

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

        _ = response.EnsureSuccessStatusCode();

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

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<AgentAccountChangeModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<LicenseeMarketsResponse> GetLicenseeMarketsAsync(GetLicenseeMarketsRequest request)
    {
        var prefix = $"Report.LicenseeId.{request.LicenseeId}.GetLicenseeMarkets";

        var keyValues = new Dictionary<string, string?>();
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

            keyValues.Add(propertyInfo.Name, value!.ToString());
        }

        var parameters = string.Join("&", keyValues.Select(kvp => string.Format("{0}={1}", kvp.Key, kvp.Value)));

        var currencyCodes = request.CurrencyCodes
            ?.Select(currencyCode => string.Format("{0}={1}", "currencyCodes", currencyCode)).ToArray();

        var currencyCodeParameters = currencyCodes == null ? string.Empty : string.Join("&", currencyCodes);

        parameters += string.IsNullOrEmpty(parameters) ? currencyCodeParameters : $"&{currencyCodeParameters}";

        var path = $"/api/LicenseeMarketReport?{parameters}";

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<LicenseeMarketsResponse>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<Stream> ExportMarketsAsync(ExportMarketsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/marketReport/exportMarkets?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

        httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

        var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        _ = response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        return stream;
    }

    public async ValueTask<Stream> ExportPlayerContributionsAsync(ExportPlayerContributionsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerReport/exportPlayerContributions?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

        httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

        var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        _ = response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        return stream;
    }

    public async ValueTask<Stream> ExportAgentContributionsAsync(ExportAgentContributionsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentReport/exportAgentContributions?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

        httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

        var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        _ = response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        return stream;
    }

    public async ValueTask<Stream> ExportAgentDividendsAsync(ExportAgentDividendsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentReport/exportAgentDividends?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

        httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

        var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        _ = response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        return stream;
    }

    public async ValueTask<Stream> ExportVipsAsync(ExportVipReportRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/vipReport/exportVips?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

        httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

        var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        _ = response.EnsureSuccessStatusCode();

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

        _ = response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        return stream;
    }

    public async ValueTask<Stream> ExportLicenseeMarketsAsync(ExportLicenseeMarketsRequest request)
    {
        var keyValues = new Dictionary<string, string?>();
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

            keyValues.Add(propertyInfo.Name, value!.ToString());
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

        _ = response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        return stream;
    }

    public async ValueTask<ResellerMarketsResponse> GetResellerMarketsAsync(GetResellerMarketsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/resellerMarketReport?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<ResellerMarketsResponse>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<Stream> ExportResellerMarketsAsync(ExportResellerMarketsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/resellerMarketReport/exportResellerMarkets?",
            request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);

        httpRequest.Headers.Add("Accept-Language", CultureInfo.CurrentCulture.Name);

        var response = await _client.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead);

        _ = response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();

        return stream;
    }

    public async ValueTask<PagedResponse<PlayerDepositModel>> GetPlayerDepositsAsync(GetPlayerDepositsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerDepositReport?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<PlayerDepositModel>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<PlayerFirstDepositDetailModel>> GetPlayerFirstDepositDetailsAsync(
        GetPlayerFirstDepositDetailsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerDepositReport/playerFirstDepositDetails?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<PlayerFirstDepositDetailModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<PlayerNoSecondDepositDetailModel>> GetPlayerNoSecondDepositDetailsAsync(
        GetPlayerNoSecondDepositDetailsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerDepositReport/playerNoSecondDepositDetails?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<PlayerNoSecondDepositDetailModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<MarketingModel>> GetMarketingListAsync(GetMarketingListRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/marketingReport?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<MarketingModel>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<MarketingRegisterDetailModel>> GetMarketingRegisterDetailsAsync(
        GetMarketingRegisterDetailRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/marketingReport/registerDetail?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<MarketingRegisterDetailModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<MarketingFirstDepositDetailModel>> GetMarketingFirstDepositDetailsAsync(
        GetMarketingFirstDepositDetailRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/marketingReport/FirstDepositDetail?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<MarketingFirstDepositDetailModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<MarketingGamingDetailModel>> GetMarketingGamingDetailsAsync(
        GetMarketingGamingDetailRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/marketingReport/GamingDetail?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<MarketingGamingDetailModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<GetBrandGameProviderRoundsResponse> GetBrandGameProviderRoundsAsync(
        GetBrandGameProviderRoundsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/brandGameProviderReport?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<GetBrandGameProviderRoundsResponse>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<AgentDividendTeamDetailModel>> GetAgentDividendTeamDetailsAsync(
        GetAgentDividendTeamDetailsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentReport/dividendTeamDetails?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

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

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<GetAdditionalExpensesResponse>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<GameActionModel>> GetGameActionsAsync(GetGameActionsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/gameActionReport?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

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

    public async ValueTask<PagedResponse<GameProviderRoundsModel>> GetGameProviderRoundsAsync(
        GetGameProviderRoundsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/gameProviderReport?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<GameProviderRoundsModel>>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<decimal> GetPlayerAccountChangesTotalAmountAsync(
        GetPlayerAccountChangesTotalRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerReport/accountChangesTotal?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<decimal>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PlayerContributionTotalAmountModel> GetPlayerContributionsTotalAmountAsync(
        GetPlayerContributionsTotalAmountRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerReport/contributionTotalAmount?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PlayerContributionTotalAmountModel>(
            await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<VipTotalAmountModel> GetVipReportTotalAmountAsync(GetVipReportTotalAmountRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/vipReport/vipTotalAmount?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<VipTotalAmountModel>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public async ValueTask<PagedResponse<AgentWinLossModel>> GetAgentWinLossAsync(GetAgentWinLossRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentWinLossReport/agentWinLoss?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<AgentWinLossModel>>(await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }

    public async ValueTask<PagedResponse<AgentWinLossModel>> GetDownLineAgentWinLossAsync(GetDownLineAgentWinLossRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentWinLossReport/downLineAgentWinLoss?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<AgentWinLossModel>>(await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }

    public async ValueTask<PagedResponse<AgentWinLossModel>> GetMasterAgentWinLossAsync(GetMasterAgentWinLossRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentWinLossReport/masterAgentWinLoss?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<AgentWinLossModel>>(await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }

    public async ValueTask<PagedResponse<AgentWinLossModel>> GetAgentPlayerWinLossAsync(GetPlayerWinLossRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentWinLossReport/agentPlayerWinLoss?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<AgentWinLossModel>>(await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }

    public async ValueTask<IEnumerable<BreadcrumbModel>> GetAgentWinLossBreadcrumbAsync(GetBreadcrumbRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentWinLossReport/breadcrumb?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<IEnumerable<BreadcrumbModel>>(await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }

    public async ValueTask<AgentPlayerSpecialDetailModel> GetAgentPlayerSpecialDetailAsync(GetAgentPlayerSpecialDetailRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentWinLossReport/agentPlayerSpecialDetail?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<AgentPlayerSpecialDetailModel>(await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }

    public async ValueTask<IEnumerable<AgentTotalDetailModel>> GetAgentTotalDetailAsync(GetAgentTotalDetailRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentWinLossReport/agentTotalDetail?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<IEnumerable<AgentTotalDetailModel>>(await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }

    public async ValueTask<IEnumerable<CompanyTotalDetailModel>> GetCompanyTotalDetailAsync(GetCompanyTotalDetailRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/agentWinLossReport/companyTotalDetail?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<IEnumerable<CompanyTotalDetailModel>>(await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }

    public async ValueTask<PagedResponse<MarketAgentProfitModel>> GetMarketAgentProfitsAsync(GetMarketAgentProfitsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/marketReport/agentProfit?", request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, path);
        var response = await _client.SendAsync(httpRequest).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PagedResponse<MarketAgentProfitModel>>(await response.Content.ReadAsStringAsync(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }
}