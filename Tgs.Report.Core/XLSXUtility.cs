using System.Resources;
using ClosedXML.Excel;

namespace Tgs.Report.Core;

public static class XLSXUtility
{
    public static XLWorkbook ExportWithLocalization<T>(this IEnumerable<T>? models, string sheetName,
        ResourceManager resourceManager)
    {
        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);
        var colIdx = 1;

        foreach (var item in typeof(T).GetProperties())
            sheet.Cell(1, colIdx++).Value = resourceManager.GetString(item.Name) ?? item.Name;

        if (models == null)
        {
            return workbook;
        }

        var rowIdx = 2;
        foreach (var model in models)
        {
            var columnIndex = 1;
            foreach (var propertyInfo in model!.GetType().GetProperties())
            {
                var value = propertyInfo.GetValue(model);

                if (propertyInfo.PropertyType.IsEnum)
                {
                    value = value?.ToString();
                }
                if (propertyInfo.PropertyType == typeof(bool))
                {
                    value = value?.ToString();
                }
                if (propertyInfo.PropertyType == typeof(DateTimeOffset) ||
                    propertyInfo.PropertyType == typeof(DateTime))
                {
                    value = ((DateTimeOffset)value!).ToString("d");
                }


                sheet.Cell(rowIdx, columnIndex).SetValue($"{value}");
                columnIndex++;
            }

            rowIdx++;
        }

        return workbook;
    }

    public static XLWorkbook ExportSystemRoundsWithLocalization<T>(this IEnumerable<T>? models, string sheetName,
        ResourceManager resourceManager)
    {
        var workbook = new XLWorkbook();
        var sheet = workbook.Worksheets.Add(sheetName);
        var colIdx = 1;

        foreach (var item in typeof(T).GetProperties())
            sheet.Cell(1, colIdx++).Value = resourceManager.GetString(item.Name) ?? item.Name;

        if (models == null)
        {
            return workbook;
        }

        var rowIdx = 2;
        foreach (var model in models)
        {
            var columnIndex = 1;
            foreach (var propertyInfo in model!.GetType().GetProperties())
            {
                var value = propertyInfo.GetValue(model);
                
                if (propertyInfo.PropertyType.IsEnum)
                {
                    value = value?.ToString();
                }
                if (propertyInfo.PropertyType == typeof(bool))
                {
                    value = value?.ToString();
                }
                if (propertyInfo.PropertyType == typeof(DateTimeOffset) ||
                    propertyInfo.PropertyType == typeof(DateTime))
                {
                    value = ((DateTimeOffset)value!).ToString("d");
                }

                if (columnIndex == 1 || columnIndex == 3)
                {
                    sheet.Cell(rowIdx, columnIndex).Value = $"'{value}";
                }
                else
                {
                    sheet.Cell(rowIdx, columnIndex).SetValue($"{value}");
                }

                columnIndex++;
            }

            rowIdx++;
        }

        return workbook;
    }
}