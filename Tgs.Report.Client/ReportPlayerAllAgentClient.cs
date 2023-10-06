using System.Text.Json;
using Tgs.Report.Client.Models.Player;

namespace Tgs.Report.Client;

public class ReportPlayerAllAgentClient : IReportPlayerAllAgentClient
{
    private readonly HttpClient _client;
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true, DefaultBufferSize = 128 };

    public ReportPlayerAllAgentClient(HttpClient client)
    {
        _client = client;
    }

    public async IAsyncEnumerable<PlayerAllAgentRebateModel> GetPlayerAllAgentRebatesAsync(
        GetPlayerAllAgentRebatesRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerReport/player-all-agent-rebates?", request);

        var response = await _client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        var playerAllAgentRebates = JsonSerializer.DeserializeAsyncEnumerable<PlayerAllAgentRebateModel>(
            responseStream,
            _jsonSerializerOptions);

        await foreach (var playerAllAgentRebate in playerAllAgentRebates)
        {
            yield return playerAllAgentRebate!;
        }
    }

    public async Task<PlayerAllAgentRebateTotalAmountModel> GetPlayerAllAgentRebatesTotalAmountAsync(GetPlayerAllAgentRebateTotalAmountRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerReport/player-all-agent-rebates-total-amount?", request);

        var response = await _client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        return JsonSerializer.Deserialize<PlayerAllAgentRebateTotalAmountModel>(
            await response.Content.ReadAsStringAsync(),
            _jsonSerializerOptions)!;
    }

    public async IAsyncEnumerable<PlayerAllAgentGameTypeModel> GetPlayerAllAgentGameTypesAsync(GetPlayerAllAgentGameTypesRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerReport/player-all-agent-game-types?", request);

        var response = await _client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        var playerAllGameTypes = JsonSerializer.DeserializeAsyncEnumerable<PlayerAllAgentGameTypeModel>(
            responseStream,
            _jsonSerializerOptions);

        await foreach (var playerAllGameType in playerAllGameTypes)
        {
            yield return playerAllGameType!;
        }
    }

    public async IAsyncEnumerable<PlayerAllAgentRebateDetailModel> GetPlayerAllAgentRebateDetailAsync(GetPlayerAllAgentRebatesDetailsRequest request)
    {
        var path = ClientPath.AppendWithProperties("/api/playerReport/player-all-agent-rebate-details?", request);

        var response = await _client.GetAsync(path, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var responseStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);

        var playerAllAgentRebateDetails = JsonSerializer.DeserializeAsyncEnumerable<PlayerAllAgentRebateDetailModel>(
            responseStream,
            _jsonSerializerOptions);

        await foreach (var playerAllAgentRebateDetail in playerAllAgentRebateDetails)
        {
            yield return playerAllAgentRebateDetail!;
        }
    }
}