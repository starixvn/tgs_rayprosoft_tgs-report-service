using System;

namespace Tgs.Report.Repository.Models
{
    public class RoundDetail
    {
        public Guid RoundId { get; set; }
        public virtual Round Round { get; set; }
        public string AdditionalData { get; set; }
    }
}