using Tgs.Report.Repository.Configurations;
using Tgs.Report.ViewModel.Player;

namespace Tgs.Report.Core
{
    public class PlayerAggregateFactory : IPlayerAggregateFactory
    {
        private readonly ITgsService<PlayerAggregate> _playerAggregateService;

        public PlayerAggregateFactory(ITgsService<PlayerAggregate> playerAggregateService)
        {
            _playerAggregateService = playerAggregateService;
        }

        public ValueTask CreatePlayerAggregatesAsync(PlayerAggregateModel[] models) =>
            _playerAggregateService.CreateEntitiesAsync(models.Select(model => new PlayerAggregate
            {
                BrandId = model.BrandId,
                AgentId = model.AgentId,
                PlayerCode = model.PlayerCode,
                Hour = model.Hour,
                ReportDate = model.ReportDate,
                CreatedOn = model.CreatedOn,
                RegisteredCount = model.RegisteredCount,
                FirstFundInCount = model.FirstFundInCount,
                DepositAmount = model.DepositAmount.Truncate(2),
                WithdrawalAmount = model.WithdrawalAmount.Truncate(2),
                AdjustmentAmount = model.AdjustmentAmount.Truncate(2),
                BonusAmount = model.BonusAmount.Truncate(2),
                RebateAmount = model.RebateAmount.Truncate(2),
                PromotionAmount = model.PromotionAmount.Truncate(2),
                DividendAmount = model.DividendAmount.Truncate(2),
                BetAmount = model.BetAmount.Truncate(2),
                ValidBetAmount = model.ValidBetAmount.Truncate(2),
                OriginalTurnover = model.OriginalTurnover.Truncate(2),
                Turnover = model.Turnover.Truncate(2),
                WinLossAmount = model.WinLossAmount.Truncate(2),
                AllAgentRebateAmount = model.AllAgentRebateAmount.Truncate(2)
            }).ToArray());

        public ValueTask DeletePlayerAggregatesAsync(PlayerAggregate[] markets) =>
            _playerAggregateService.DeleteEntitiesAsync(markets);
    }
}