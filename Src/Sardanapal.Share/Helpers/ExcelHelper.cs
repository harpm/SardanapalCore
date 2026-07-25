// Licensed under the MIT license.

using System.Globalization;
using System.Reflection;
using ClosedXML.Excel;

namespace Sardanapal.Share.Helpers;

public static class ExcelHelper
{
    public static byte[] ToExcel<T>(IEnumerable<T>? data, string[]? columns = null, string sheetName = "Sheet1", CultureInfo? culture = null)
    {
        CultureInfo resolvedCulture = culture ?? CultureInfo.CurrentCulture;
        PropertyInfo[] properties = ResolveProperties<T>(columns);

        using XLWorkbook workbook = new XLWorkbook();
        IXLWorksheet worksheet = workbook.Worksheets.Add(sheetName);

        for (int i = 0; i < properties.Length; i++)
        {
            worksheet.Cell(1, i + 1).Value = properties[i].Name;
        }
        worksheet.Row(1).Style.Font.Bold = true;

        int rowIndex = 2;
        foreach (T item in data ?? Enumerable.Empty<T>())
        {
            for (int c = 0; c < properties.Length; c++)
            {
                object? value = properties[c].GetValue(item);
                WriteValue(worksheet.Cell(rowIndex, c + 1), value, resolvedCulture);
            }
            rowIndex++;
        }

        worksheet.Columns().AdjustToContents();

        using MemoryStream stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private static PropertyInfo[] ResolveProperties<T>(string[]? columns)
    {
        PropertyInfo[] all = typeof(T)
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        if (columns is null || columns.Length == 0)
        {
            return all;
        }

        return columns
            .Select(name => all.FirstOrDefault(p => p.Name == name))
            .Where(p => p is not null)
            .Select(p => p!)
            .ToArray();
    }

    private static void WriteValue(IXLCell cell, object? value, CultureInfo culture)
    {
        switch (value)
        {
            case null:
                break;
            case DateTime dt:
                cell.Value = dt;
                cell.Style.DateFormat.Format = ResolveDateTimeFormat(dt, culture);
                break;
            case bool b:
                cell.Value = Convert.ToString(b, culture);
                break;
            case string s:
                cell.Value = s;
                break;
            case decimal dec:
                cell.Value = dec;
                break;
            case double d:
                cell.Value = d;
                break;
            case float f:
                cell.Value = f;
                break;
            case IConvertible c:
                cell.Value = Convert.ToDouble(c, culture);
                break;
            default:
                cell.Value = Convert.ToString(value, culture);
                break;
        }
    }

    private static string ResolveDateTimeFormat(DateTime dt, CultureInfo culture)
    {
        DateTimeFormatInfo format = culture.DateTimeFormat;
        string pattern = format.ShortDatePattern;

        if (dt.TimeOfDay != TimeSpan.Zero)
        {
            // Excel uses the "AM/PM" literal instead of .NET's "tt" designator token
            string timePattern = format.ShortTimePattern.Replace("tt", "AM/PM");
            pattern = string.Concat(pattern, " ", timePattern);
        }

        return pattern;
    }
}
