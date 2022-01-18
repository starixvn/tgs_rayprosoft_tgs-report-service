using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.Player
{
    public class GetPlayerContributionsRequest : PageableRequest
    {
        public Guid BrandId { get; set; }
        public string Username { get; set; }
        public int? Level { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
    }
}