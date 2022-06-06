using MediatR;
using Tgs.Report.ViewModel;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core.Queries.Player
{
    public class
        GetPlayerAccountChangeQueryHandler : IRequestHandler<GetPlayerAccountChangesRequest,
            PagedResponse<PlayerAccountChangeModel>>
    {
        private readonly IPlayerFactory _playerFactory;

        public GetPlayerAccountChangeQueryHandler(IPlayerFactory playerFactory)
        {
            _playerFactory = playerFactory;
        }

        public async Task<PagedResponse<PlayerAccountChangeModel>> Handle(
            GetPlayerAccountChangesRequest request,
            CancellationToken cancellationToken)
        {
            var (playerAccountChanges, totalCount) = await _playerFactory.GetPlayerAccountChangeAsync(request);



            return await playerAccountChanges.CreatePagedResponseAsync(totalCount);
        }
    }
}