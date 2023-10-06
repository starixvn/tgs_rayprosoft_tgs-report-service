namespace Tgs.Report.Client.Models.Player
{
    public record GetPlayerAllAgentRebatesRequest : PageableRequest
    {
        public Guid BrandId { get; set; }
        public string? Username { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }
}
