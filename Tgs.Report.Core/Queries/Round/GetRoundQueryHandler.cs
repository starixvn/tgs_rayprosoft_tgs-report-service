using System.Text.Json;
using MediatR;
using Tgs.GameProviders.Pluggable.Client;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round;

public class GetRoundQueryHandler(IGameProvidersBackofficePluggableClient client)
    : IRequestHandler<GetRoundHistoryRequest, JsonDocument?>
{
    public async Task<JsonDocument?> Handle(
        GetRoundHistoryRequest request,
        CancellationToken cancellationToken)
        => await client.GameRoundDetailGetAsync(request.RoundId, request.Language ?? "");
}