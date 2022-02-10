using System.Text.Json;
using MediatR;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.Report.ViewModel.Round;
using Tgs.Report.ViewModel.Round.BetHistories;

namespace Tgs.Report.Core.Queries.Round
{
    public class GetRoundQueryHandler : IRequestHandler<GetRoundHistoryRequest, GetRoundHistoryResponse>
    {
        private readonly IGameProvidersBackofficePluggableClient _client;

        public GetRoundQueryHandler(IGameProvidersBackofficePluggableClient client) => _client = client;

        public async Task<GetRoundHistoryResponse> Handle(
            GetRoundHistoryRequest request,
            CancellationToken cancellationToken)
        {
            var jsonData = await _client.GameRoundDetailGetAsync(request.RoundId, request.Language);

            if (jsonData == null)
            {
                return new GetRoundHistoryResponse();
            }

            return new GetRoundHistoryResponse
            {
                Data = jsonData!.Deserialize<BaseData[]>(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!
            };
        }
    }
}