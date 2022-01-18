using System;

namespace Tgs.Report.ViewModel.Agent
{
    public class TeamInfoModel
    {
        public decimal Contribution { get; set; }

        public DateTimeOffset? RegisteredOn { get; set; }

        public string Username { get; set; }
    }
}