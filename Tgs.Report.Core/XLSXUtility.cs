using System.Resources;
using ClosedXML.Excel;

namespace Tgs.Report.Core
{
    public static class XLSXUtility
    {
        public static XLWorkbook Export<T>(this IEnumerable<T> models, string sheetName,
            ResourceManager? resourceManager = null)
        {
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add(sheetName);
            var colIdx = 1;

            foreach (var item in typeof(T).GetProperties())
            {
                sheet.Cell(1, colIdx++).Value = resourceManager?.GetString(item.Name) ?? item.Name;
            }

            var rowIdx = 2;
            foreach (var model in models)
            {
                var columnIndex = 1;
                foreach (var propertyInfo in model!.GetType().GetProperties())
                {
                    sheet.Cell(rowIdx, columnIndex).Value = propertyInfo.GetValue(model);
                    columnIndex++;
                }

                rowIdx++;
            }

            return workbook;
        }

        public static XLWorkbook ExportDepositLocalization<T>(this IEnumerable<T>? models, string sheetName,
            decimal sum,
            ResourceManager resourceManager)
        {
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add(sheetName);
            var colIdx = 1;

            foreach (var item in typeof(T).GetProperties())
            {
                sheet.Cell(1, colIdx++).Value = resourceManager.GetString(item.Name) ?? item.Name;
            }

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
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        value = value?.ToString();
                    }

                    sheet.Cell(rowIdx, columnIndex).Value = value;
                    columnIndex++;
                }

                rowIdx++;
            }

            sheet.Cell(rowIdx + 1, 6).Value = "Sum";
            sheet.Cell(rowIdx + 1, 7).Value = sum;
            return workbook;
        }

        public static XLWorkbook ExportWithdrawLocalization<T>(this IEnumerable<T>? models, string sheetName,
            decimal sum,
            ResourceManager resourceManager)
        {
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add(sheetName);
            var colIdx = 1;

            foreach (var item in typeof(T).GetProperties())
            {
                sheet.Cell(1, colIdx++).Value = resourceManager.GetString(item.Name) ?? item.Name;
            }

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
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        value = value?.ToString();
                    }

                    sheet.Cell(rowIdx, columnIndex).Value = value;
                    columnIndex++;
                }

                rowIdx++;
            }

            sheet.Cell(rowIdx + 1, 5).Value = "Sum";
            sheet.Cell(rowIdx + 1, 6).Value = sum;
            return workbook;
        }

        public static XLWorkbook ExportWithLocalization<T>(this IEnumerable<T>? models, string sheetName,
            ResourceManager resourceManager)
        {
            var workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add(sheetName);
            var colIdx = 1;

            foreach (var item in typeof(T).GetProperties())
            {
                sheet.Cell(1, colIdx++).Value = resourceManager.GetString(item.Name) ?? item.Name;
            }

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
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        value = value?.ToString();
                    }

                    sheet.Cell(rowIdx, columnIndex).Value = value;                    
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
            {
                sheet.Cell(1, colIdx++).Value = resourceManager.GetString(item.Name) ?? item.Name;
            }

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
                    else if (propertyInfo.PropertyType == typeof(bool))
                    {
                        value = value?.ToString();
                    }

                    if(columnIndex == 1 || columnIndex == 3)
                        sheet.Cell(rowIdx, columnIndex).Value = $"'{value}";
                    else
                        sheet.Cell(rowIdx, columnIndex).Value = value;
                    columnIndex++;
                }

                rowIdx++;
            }

            return workbook;
        }
    }
}