namespace Tgs.Report.Client.Models.Player
{
    public record GetPlayerAllAgentRebateDetailGameTypesRequest
    {
        public Guid BrandId { get; set; }
        public string PlayerCode { get; set; } = string.Empty;
        public DateTime SettleDateTime { get; set; }
    }
}
