using ClosedXML.Excel;
using FluentAssertions;
using Sardanapal.Share.Helpers;
using System.Globalization;
using Xunit;

namespace Sardanapal.Share.Tests.Unit;

public class ExcelHelperTests
{
    private class Row
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool Active { get; set; }
    }

    private static XLWorkbook Parse(byte[] bytes)
    {
        using MemoryStream stream = new MemoryStream(bytes);
        return new XLWorkbook(stream);
    }

    [Fact]
    public void ToExcel_ShouldApplyCultureShortDatePattern_ForDateTime()
    {
        // Arrange
        Row[] rows = new[]
        {
            new Row { Id = 1, Name = "a", CreatedAt = new DateTime(2026, 7, 26), Active = true }
        };

        // Act - en-GB short date pattern is dd/MM/yyyy
        byte[] bytes = ExcelHelper.ToExcel(rows, culture: CultureInfo.GetCultureInfo("en-GB"));

        // Assert
        using XLWorkbook workbook = Parse(bytes);
        IXLCell dateCell = workbook.Worksheets.First().Cell(2, 3);
        dateCell.Style.DateFormat.Format.Should().Be("dd/MM/yyyy");
    }

    [Fact]
    public void ToExcel_ShouldOmitTimePattern_WhenDateTimeHasNoTime()
    {
        // Arrange
        Row[] rows = new[]
        {
            new Row { Id = 1, Name = "a", CreatedAt = new DateTime(2026, 7, 26), Active = true }
        };

        // Act - en-US short date is M/d/yyyy, short time is h:mm tt
        byte[] bytes = ExcelHelper.ToExcel(rows, culture: CultureInfo.GetCultureInfo("en-US"));

        // Assert
        using XLWorkbook workbook = Parse(bytes);
        IXLCell dateCell = workbook.Worksheets.First().Cell(2, 3);
        dateCell.Style.DateFormat.Format.Should().Be("M/d/yyyy");
    }

    [Fact]
    public void ToExcel_ShouldIncludeTimePattern_WhenDateTimeHasTime()
    {
        // Arrange
        Row[] rows = new[]
        {
            new Row { Id = 1, Name = "a", CreatedAt = new DateTime(2026, 7, 26, 14, 30, 0), Active = true }
        };

        // Act
        byte[] bytes = ExcelHelper.ToExcel(rows, culture: CultureInfo.GetCultureInfo("en-US"));

        // Assert - .NET "tt" must be translated to Excel's "AM/PM" literal
        using XLWorkbook workbook = Parse(bytes);
        IXLCell dateCell = workbook.Worksheets.First().Cell(2, 3);
        dateCell.Style.DateFormat.Format.Should().Contain("AM/PM");
        dateCell.Style.DateFormat.Format.Should().NotContain("tt");
    }

    [Fact]
    public void ToExcel_ShouldWriteBoolAsCultureAwareText()
    {
        // Arrange
        Row[] rows = new[]
        {
            new Row { Id = 1, Name = "a", CreatedAt = DateTime.Today, Active = true }
        };

        // Act
        byte[] bytes = ExcelHelper.ToExcel(rows, culture: CultureInfo.GetCultureInfo("en-US"));

        // Assert
        using XLWorkbook workbook = Parse(bytes);
        IXLCell boolCell = workbook.Worksheets.First().Cell(2, 4);
        boolCell.DataType.Should().Be(XLDataType.Text);
        boolCell.GetValue<string>().Should().Be(Convert.ToString(true, CultureInfo.GetCultureInfo("en-US")));
    }
}
