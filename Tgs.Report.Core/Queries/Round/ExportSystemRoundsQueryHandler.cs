using System.Resources;
using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round;

internal class ExportSystemRoundsQueryHandler(IMediator mediator)
    : IRequestHandler<ExportSystemRoundsRequest, MemoryStream>
{
    public async Task<MemoryStream> Handle(
        ExportSystemRoundsRequest request,
        CancellationToken cancellationToken)
    {
        var getSystemRoundsRequest = new GetSystemRoundsRequest
        {
            Username = request.Username,
            PlayerCode = request.PlayerCode,
            ExternalRoundId = request.ExternalRoundId,
            RoundId = request.RoundId,
            ExternalFrom = request.ExternalFrom,
            ExternalTo = request.ExternalTo,
            ExternalClosedFrom = request.ExternalClosedFrom,
            ExternalClosedTo = request.ExternalClosedTo,
            From = request.From,
            To = request.To,
            ClosedFrom = request.ClosedFrom,
            ClosedTo = request.ClosedTo,
            Status = (ClientRoundStatus?)request.Status,
            BrandId = request.BrandId,
            GameProviderId = request.GameProviderId,
            SubGameProviderCode = request.SubGameProviderCode,
            GameType = (GameType?)request.GameType,
            CurrencyCode = request.CurrencyCode,
            ShowAll = true
        };

        var rounds = await mediator.Send(getSystemRoundsRequest, cancellationToken).ConfigureAwait(false);

        var exportData = rounds.Items?.Select(x => new ExportSystemRoundsModel
        {
            PlayerName = x.PlayerName,
            PlayerCode = x.PlayerCode,
            ExternalRoundId = x.ExternalRoundId,
            ExternalCreatedOn = x.ExternalCreatedOn?.ToString("o"),
            ExternalClosedOn = x.ExternalClosedOn?.ToString("o"),
            Id = x.Id,
            CreatedOn = x.CreatedOn.ToString("o"),
            ClosedOn = x.ClosedOn?.ToString("o"),
            BetAmount = x.BetAmount?.Truncate(2),
            WinLoss = x.WinLoss?.Truncate(2)
        });

        var resource = new ResourceManager(typeof(Resources.SystemRound));

        var xlsx = exportData?.ExportSystemRoundsWithLocalization("rounds", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}