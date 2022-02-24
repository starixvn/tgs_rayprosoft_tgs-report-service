using System;
using Tgs.Report.Enum;

namespace Tgs.Report.Repository.Models
{
    public class PlayerBucket
    {
        public Guid Id { get; set; }
        public string PlayerCode { get; set; }
        public decimal BucketAmount { get; set; }
        public decimal CurrentBucketAmount { get; set; }
        public BucketType BucketType { get; set; }
        public bool IsFull { get; set; }
        public DateTimeOffset CreatedOn { get; set; }
    }
}