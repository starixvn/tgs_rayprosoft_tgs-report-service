using System;
using Tgs.Report.Enum;

namespace Tgs.Report.Repository.Models
{
    public class BrandGame
    {
        public Guid Id { get; set; }
        public int GameId { get; set; }
        public virtual Game Game { get; set; }
        public Guid BrandId { get; set; }
        public virtual Brand Brand { get; set; }
        public GameStatus Status { get; set; }
        public GameTag Tag { get; set; }
        public int DisplayOrder { get; set; }
        public bool Rebate { get; set; }
        public decimal EffectiveBettingPercentage { get; set; }
    }
}