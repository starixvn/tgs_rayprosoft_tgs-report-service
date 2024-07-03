namespace Tgs.Report.Client.Models.Player
{
    public record GetPlayerAllAgentRebateTotalAmountRequest
    {
        public Guid BrandId { get; set; }
        public string? Username { get; set; }
        public string? PlayerCode { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
