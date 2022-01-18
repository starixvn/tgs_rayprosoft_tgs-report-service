using System;
using Tgs.Report.ViewModel;

namespace Tgs.Report.Client.Models.GameAction
{
    public class GetGameActionsRequest : PageableRequest
    {
        public Guid RoundId { get; set; }
    }
}