namespace Tgs.Report.Enum
{
    public enum TransactionType
    {
        Deposit = 0,
        Withdrawal = 1,

        CompanyAdjust = 2,
        Bonus = 3,
        Rebate = 4,
        Dividend = 5,
        Promotion = 6,
        GameProviderFundOut = 7,
        GameProviderFundIn = 8,
        CancelWithdrawal = 9,
        GameProviderDeposit = 10,
        GameProviderWithdrawal = 11,
        UnsettledBetManual = 12,

        Other = 99,

        /// <summary>
        /// 存款 1000~1999
        /// </summary>
        //Deposit = 1000,
        /// <summary>
        /// 提款 2000~2999
        /// </summary>
        //Withdraw = 2000,
        /// <summary>
        /// 活動 3000~3999
        /// </summary>
        //Promotion = 3000,

        /// <summary>
        ///     The promotion vip level upgrade
        /// </summary>
        PromotionVipLevelUpgrade = 3800,

        /// <summary>
        ///     The promotion vip level season
        /// </summary>
        PromotionVipLevelSeason = 3850,
        PromotionVipLevelBirthday = 3860,

        /// <summary>
        /// 分紅 4000~4999
        /// </summary>
        //Dividend = 4000,
        /// <summary>
        ///     The dividend all agent rebate
        /// </summary>
        DividendAllAgentRebate = 4100,

        /// <summary>
        ///     調帳 5000~5999
        /// </summary>
        //Adjust = 5000,
        AgentAdjust = 5100,
        SystemAdjust = 5200,
        TransferAdjust = 5300,
        BonusAdjust = 5400,

        /// <summary>
        ///     已結算單再調整
        /// </summary>
        AdjustResettledBetManual = 5010,

        /// <summary>
        ///     遊戲 10000~10999
        /// </summary>
        Game = 10000,

        GamePlaceBet = 11000,

        // 結算 12000~12999

        GameWinBet = 12100,
        GameLoseBet = 12200,
        GameFreeBet = 12300,
        GameTieBet = 12400,
        GameWinJackpot = 12500,

        // 取消 13000~13999

        GameCancelTransaction = 13100,

        // 鎖定相關 14000~14999

        GameLockAmount = 14100,
        GameUnlockAmount = 14200
    }
}