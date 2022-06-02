using Tgs.Report.ViewModel.Market;
using Tgs.Report.ViewModel.Round;

namespace Tgs.Report.Core;

public interface IPlayerDepositFactory
{
    Task<IEnumerable<PlayerFirstDepositDetailModel>> GetPlayerFirstDepositDetailsAsync(Guid brandId, DateTimeOffset start,
        DateTimeOffset end);

    Task<IEnumerable<PlayerNoSecondDepositDetailModel>> GetPlayerNoSecondDepositDetailsAsync(Guid brandId, DateTimeOffset start,
        DateTimeOffset end);

}