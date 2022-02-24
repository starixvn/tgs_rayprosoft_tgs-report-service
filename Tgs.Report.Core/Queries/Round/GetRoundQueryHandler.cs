using System.Text.Json;
using MediatR;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round
{
    public class GetRoundQueryHandler : IRequestHandler<GetRoundHistoryRequest, JsonDocument?>
    {
        private readonly IGameProvidersBackofficePluggableClient _client;

        public GetRoundQueryHandler(IGameProvidersBackofficePluggableClient client) => _client = client;

        public async Task<JsonDocument?> Handle(
            GetRoundHistoryRequest request,
            CancellationToken cancellationToken)
            => await _client.GameRoundDetailGetAsync(request.RoundId, request.Language);
    }
}