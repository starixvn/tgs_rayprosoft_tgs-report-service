using System;

namespace Tgs.Report.Repository;

public record AgentWithNLevelInfo(
    Guid Id,
    string Path,
    int NLevel);