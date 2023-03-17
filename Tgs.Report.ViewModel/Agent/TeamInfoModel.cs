using System;

namespace Tgs.Report.ViewModel.Agent;

public record TeamInfoModel
{
    public decimal Contribution { get; init; }

    public DateTimeOffset? RegisteredOn { get; init; }

    public string Username { get; init; } = default!;
}