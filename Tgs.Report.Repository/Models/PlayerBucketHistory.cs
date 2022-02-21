using System;

namespace Tgs.Report.Repository.Models
{
    public class PlayerBucketHistory
    {
        public int Id { get; set; }
        public string PlayerCode { get; set; }
        public decimal Turnover { get; set; }
        public DateTimeOffset From { get; set; }
        public DateTimeOffset To { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}