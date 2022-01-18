namespace Tgs.Report.Enum
{
    public enum AccountChangeType
    {
        Deposit = 0,
        Withdrawal = 1,
        Adjust = 2,
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
        AllAgentRebate = 100000,
        PlaceBet = 100,
        WinBet = 101,
        LoseBet = 102,
        FreeBet = 103,
        TieBet = 104,
        CancelTransaction = 105,
        LockAmount = 106,
        UnlockAmount = 107,
        WinJackpot = 108
    }
}