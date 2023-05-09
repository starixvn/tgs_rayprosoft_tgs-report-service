using System.Resources;
using MediatR;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round;

internal class ExportRoundsQueryHandler : IRequestHandler<ExportRoundsRequest, MemoryStream>
{
    private readonly IMediator _mediator;

    public ExportRoundsQueryHandler(IMediator mediator) => _mediator = mediator;

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

        var rounds = await _mediator.Send(getRoundsRequest, cancellationToken).ConfigureAwait(false);

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
            BetAmount = x.BetAmount!.Value,
            ValidBetAmount = x.ValidBetAmount!.Value,
            Turnover = x.Turnover!.Value,
            SettleAmount = x.SettleAmount!.Value,
            WinLoss = x.WinLoss!.Value,
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