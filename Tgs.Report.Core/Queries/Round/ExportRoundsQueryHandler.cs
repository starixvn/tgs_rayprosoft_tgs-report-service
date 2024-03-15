using System.Resources;
using MediatR;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round;

internal class ExportRoundsQueryHandler(IMediator mediator) : IRequestHandler<ExportRoundsRequest, MemoryStream>
{
    public async Task<MemoryStream> Handle(
        ExportRoundsRequest request,
        CancellationToken cancellationToken)
    {
        var getRoundsRequest = new GetRoundsRequest
        {
            BrandId = request.BrandId,
            Username = request.Username,
            PlayerCode = request.PlayerCode,
            ExternalRoundId = request.ExternalRoundId,
            RoundId = request.RoundId,
            From = request.From,
            To = request.To,
            ClosedFrom = request.ClosedFrom,
            ClosedTo = request.ClosedTo,
            Status = request.Status,
            GameProviderId = request.GameProviderId,
            GameType = request.GameType,
            Page = request.Page,
            Rows = request.Rows,
            OrderBy = request.OrderBy,
            SortBy = request.SortBy
        };

        var rounds = await mediator.Send(getRoundsRequest, cancellationToken).ConfigureAwait(false);

        var exportData = rounds.Items?.Select(x => new ExportRoundsModel
        {
            PlayerName = x.PlayerName!,
            ExternalRoundId = x.ExternalRoundId!,
            Id = x.Id,
            CreatedOn = x.CreatedOn.ToString("o"),
            ClosedOn = x.ClosedOn?.ToString("o"),
            GameProviderName = x.GameProviderName!,
            SubGameProviderName = x.SubGameProviderName!,
            GameName = x.GameName!,
            CurrencyCode = x.CurrencyCode!,
            BetAmount = x.BetAmount!.Value.Truncate(2),
            ValidBetAmount = x.ValidBetAmount!.Value.Truncate(2),
            Turnover = x.Turnover!.Value.Truncate(2),
            SettleAmount = x.SettleAmount!.Value.Truncate(2),
            WinLoss = x.WinLoss!.Value.Truncate(2),
            PlayerCode = x.PlayerCode,
            ExternalCreatedOn = x.ExternalCreatedOn?.ToString("o"),
            ExternalClosedOn = x.ExternalClosedOn?.ToString("o")
        });

        var resource = new ResourceManager(typeof(Resources.Round));

        var xlsx = exportData?.ExportSystemRoundsWithLocalization("rounds", resource);

        await using var stream = new MemoryStream();

        xlsx?.SaveAs(stream);

        return new(stream.ToArray());
    }
}