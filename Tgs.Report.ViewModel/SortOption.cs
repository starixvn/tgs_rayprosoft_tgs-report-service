namespace Tgs.Report.ViewModel;

public class SortOption(string? propertyName, bool isDescending)
{
    public string? PropertyName { get; init; } = propertyName;

    public bool IsDescending { get; init; } = isDescending;
}