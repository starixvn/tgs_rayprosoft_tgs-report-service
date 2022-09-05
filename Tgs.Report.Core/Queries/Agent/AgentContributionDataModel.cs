namespace Tgs.Report.Core.Queries.Agent
{
    internal class AgentContributionDataModel
    {
        public Guid? AgentId { get; set; }
        public string PlayerCode { get; set; } = default!;
        public decimal DepositAmount { get; set; }
        public decimal WithdrawalAmount {get; set; }
        public decimal CompanyAdjustAmount { get; set; }
        public decimal AgentAdjustAmount { get; set; }
        public decimal BonusAdjustAmount { get; set; }
        public decimal BonusAmount { get; set; }
        public decimal BetAmount { get; set; }
        public decimal ValidBetAmount { get; set; }
        public decimal Turnover { get; set; }
        public decimal WinLossAmount { get; set; }
        public decimal NetProfit { get; set; }
        public decimal UpgradeAmount { get; set; }
        public decimal SeasonAmount { get; set; }
        public decimal BirthdayAmount { get; set; }
    }
}
