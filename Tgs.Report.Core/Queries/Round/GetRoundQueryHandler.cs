using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Tgs.Report.Repository;
using Tgs.Report.ViewModel.GameProvider;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round
{
    public class GetRoundQueryHandler : IRequestHandler<GetRoundHistoryRequest, GetRoundHistoryResponse>
    {
        private readonly HttpClient _httpClient;
        private readonly ITgsRepository _repository;

        public GetRoundQueryHandler(ITgsRepository repository, HttpClient httpClient)
        {
            _repository = repository;
            _httpClient = httpClient;
        }

        public async Task<GetRoundHistoryResponse> Handle(
            GetRoundHistoryRequest request,
            CancellationToken cancellationToken)
        {
            var key = $"{request.GameProviderCode}Configuration";

            var jsonConfig = await _repository.Settings
                .Where(x => x.Key == key)
                .Select(x => x.Value)
                .FirstOrDefaultAsync(cancellationToken);

            if (jsonConfig is null)
            {
                return new();
            }

            var url = JsonSerializer.Deserialize<GameProviderUrl>(jsonConfig);
            var httpRequest =
                new HttpRequestMessage(HttpMethod.Post, $"{url.ServiceEndpointUrl}/api/pluggable/history/round")
                {
                    Content =
                        new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json")
                };
            var response = await _httpClient.SendAsync(httpRequest, cancellationToken);

            response.EnsureSuccessStatusCode();

            return JsonSerializer.Deserialize<GetRoundHistoryResponse>(
                await response.Content.ReadAsStringAsync(cancellationToken),
                new JsonSerializerOptions
                    { PropertyNameCaseInsensitive = true })!;
        }
    }
}