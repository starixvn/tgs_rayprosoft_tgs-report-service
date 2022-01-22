namespace Tgs.Report.ViewModel.PlayerRebate
{
    public record CreatePlayerRebateResponse : MessageResponse
    {
        public PlayerRebateModel[] PlayerRebates { get; init; }
    }
}