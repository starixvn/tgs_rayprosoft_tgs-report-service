using System;

namespace Tgs.Report.ViewModel.Player
{
    public class PlayerDataModel
    {
        public Guid BrandId { get; set; }
        public Guid? AgentId { get; set; }
        public string PlayerCode { get; set; }
        public int Hour { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
        public DateTimeOffset ReportDate { get; set; }
        public decimal Contribution { get; set; }
    }
}