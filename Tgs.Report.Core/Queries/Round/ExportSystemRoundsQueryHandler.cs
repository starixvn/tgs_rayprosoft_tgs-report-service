using System.Resources;
using MediatR;
using Tgs.Report.Enum;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core.Queries.Round
{
    internal class ExportSystemRoundsQueryHandler : IRequestHandler<ExportSystemRoundsRequest, MemoryStream>
    {
        private readonly IMediator _mediator;

        public ExportSystemRoundsQueryHandler(IMediator mediator) => _mediator = mediator;

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

            var rounds = await _mediator.Send(getSystemRoundsRequest, cancellationToken).ConfigureAwait(false);
            
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
                BetAmount = x.BetAmount,
                WinLoss = x.WinLoss
            });

            var resource = new ResourceManager(typeof(Resources.Round));

            var xlsx = exportData?.ExportWithLocalization("rounds", resource);

            await using var stream = new MemoryStream();

            xlsx?.SaveAs(stream);

            return new MemoryStream(stream.ToArray());
        }
    }
}