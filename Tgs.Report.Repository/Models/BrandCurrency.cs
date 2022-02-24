using System;

namespace Tgs.Report.Repository.Models
{
    public class BrandCurrency
    {
        public Guid BrandId { get; set; }

        public virtual Brand Brand { get; set; }

        public string CurrencyCode { get; set; }

        public virtual Currency Currency { get; set; }

        public string BrandCurrencyCode { get; set; }
    }
}