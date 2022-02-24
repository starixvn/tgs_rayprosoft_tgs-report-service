using System;
using System.Collections.Generic;

namespace Tgs.Report.Repository.Models
{
    public class SubGameProvider
    {
        public string Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public Guid GameProviderId { get; set; }

        public virtual GameProvider GameProvider { get; set; }

        public string Localization { get; set; }

        public DateTimeOffset CreatedOn { get; set; }

        public DateTimeOffset UpdatedOn { get; set; }

        public string CreatedByUsername { get; set; }

        public string UpdatedByUsername { get; set; }

        public virtual ICollection<Game> Games { get; set; }
    }
}