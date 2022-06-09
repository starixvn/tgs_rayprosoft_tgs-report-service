namespace Tgs.Report.Enum
{
    public enum AccountChangeType
    {
        Deposit = 0,
        Withdrawal = 1,
        CompanyAdjust = 2,
        AgentAdjust = 5100,
        SystemAdjust = 5200,
        TransferAdjust = 5300,
        BonusAdjust = 5400,
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
        DividendAllAgentRebate = 5010,
        PlaceBet = 100,
        WinBet = 101,
        LoseBet = 102,
        FreeBet = 103,
        TieBet = 104,
        CancelTransaction = 105,
        LockAmount = 106,
        UnlockAmount = 107,
        WinJackpot = 108,

        /// <summary>
        ///     The promotion vip level upgrade
        /// </summary>
        PromotionVipLevelUpgrade = 3800,

        /// <summary>
        ///     The promotion vip level season
        /// </summary>
        PromotionVipLevelSeason = 3850,
        PromotionVipLevelBirthday = 3860
    }
}