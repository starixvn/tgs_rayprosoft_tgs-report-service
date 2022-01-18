using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core
{
    public class PlayerGameAggregateFactory : IPlayerGameAggregateFactory
    {
        private readonly ITgsService<PlayerGameRebateAggregate> _playerGameAggregateService;

        public PlayerGameAggregateFactory(ITgsService<PlayerGameRebateAggregate> playerGameAggregateService)
        {
            _playerGameAggregateService = playerGameAggregateService;
        }

        public ValueTask CreatePlayerGameAggregatesAsync(
            PlayerGameTypeAggregateModel[] models) =>
            _playerGameAggregateService.CreateEntitiesAsync(models.Select(model => new PlayerGameRebateAggregate
            {
                BrandId = model.BrandId,
                AgentId = model.AgentId,
                GameProviderId = model.GameProviderId,
                PlayerCode = model.PlayerCode,
                VipLevel = model.VipLevel,
                GameId = model.GameId,
                GameType = model.GameType,
                Hour = model.Hour,
                ReportDate = model.ReportDate,
                CreatedOn = model.CreatedOn,
                BetAmount = model.BetAmount.Truncate(2),
                ValidBetAmount = model.ValidBetAmount.Truncate(2),
                OriginalTurnover = model.OriginalTurnover.Truncate(2),
                Turnover = model.Turnover.Truncate(2),
                WinLoss = model.WinLoss.Truncate(2),
                VipRebateAmount = model.VipRebateAmount.Truncate(2),
                VipLiveDealerRebatePercentage = model.VipLiveDealerRebatePercentage,
                VipSlotRebatePercentage = model.VipSlotRebatePercentage,
                VipCardGameRebatePercentage = model.VipCardGameRebatePercentage,
                VipSportsRebatePercentage = model.VipSportsRebatePercentage,
                VipVirtualSportsRebatePercentage = model.VipVirtualSportsRebatePercentage,
                VipLotteryRebatePercentage = model.VipLotteryRebatePercentage
            }).ToArray());

        public ValueTask DeletePlayerGameAggregatesAsync(PlayerGameRebateAggregate[] markets) =>
            _playerGameAggregateService.DeleteEntitiesAsync(markets);
    }
}