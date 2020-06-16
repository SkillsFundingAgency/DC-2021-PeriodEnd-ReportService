using System.Collections.Generic;
using System.Drawing;
using Aspose.Cells;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.FundingSummary
{
    public class FundingSummaryRenderService : IRenderService<FundingSummaryReportModel>
    {
        private const int StartYear = 19;
        private const int EndYear = 20;

        private const string NotApplicable = "N/A";
        private const string DecimalFormat = "#,##0.00";

        private const int StartColumn = 0;
        private const int ColumnCount = 18;

        private readonly Style _defaultStyle;
        private readonly Style _textWrappedStyle;
        private readonly Style _futureMonthStyle;
        private readonly Style _fundingCategoryStyle;
        private readonly Style _fundingSubCategoryStyle;
        private readonly Style _fundLineGroupStyle;
        private readonly Style _headerAndFooterStyle;

        private readonly StyleFlag _styleFlag = new StyleFlag()
        {
            All = true,
        };

        private readonly StyleFlag _italicStyleFlag = new StyleFlag()
        {
            FontItalic = true
        };

        public FundingSummaryRenderService()
        {
            var cellsFactory = new CellsFactory();

            _defaultStyle = cellsFactory.CreateStyle();
            _textWrappedStyle = cellsFactory.CreateStyle();
            _futureMonthStyle = cellsFactory.CreateStyle();
            _fundingCategoryStyle = cellsFactory.CreateStyle();
            _fundingSubCategoryStyle = cellsFactory.CreateStyle();
            _fundLineGroupStyle = cellsFactory.CreateStyle();
            _headerAndFooterStyle = cellsFactory.CreateStyle();

            ConfigureStyles();
        }

        public Worksheet Render(FundingSummaryReportModel fundingSummaryReport, Worksheet worksheet)
        {
            worksheet.Workbook.DefaultStyle = _defaultStyle;
            worksheet.Cells.StandardWidth = 20;
            worksheet.Cells.Columns[0].Width = 65;

            RenderHeader(worksheet, NextRow(worksheet), fundingSummaryReport);

            foreach (var fundingCategory in fundingSummaryReport.FundingCategories)
            {
                RenderFundingCategory(worksheet, fundingCategory);
            }

            RenderFooter(worksheet, NextRow(worksheet) + 1, fundingSummaryReport);

            worksheet.AutoFitColumn(0);
            worksheet.AutoFitColumn(1);

            return worksheet;
        }

        private Worksheet RenderHeader(Worksheet worksheet, int row, FundingSummaryReportModel fundingSummaryReport) =>
            RenderHeaderOrFooterArray(worksheet, row, fundingSummaryReport.HeaderData);

        private Worksheet RenderFooter(Worksheet worksheet, int row, FundingSummaryReportModel fundingSummaryReport) =>
            RenderHeaderOrFooterArray(worksheet, row, fundingSummaryReport.FooterData);

        private Worksheet RenderFundingCategory(Worksheet worksheet, FundingCategory fundingCategory)
        {
            var row = NextRow(worksheet) + 1;

            worksheet.Cells.ImportObjectArray(new object[] { "Contract No.", fundingCategory.FundingCategoryTitle }, row, 0, false);
            ApplyStyleToRow(worksheet, row, _fundingCategoryStyle);

            foreach (var fundingSubCategory in fundingCategory.FundingSubCategories)
            {
                RenderFundingSubCategory(worksheet, fundingSubCategory, fundingCategory.ContractAllocationNumber);
            }

            row = NextRow(worksheet) + 1;
            RenderFundingSummaryReportRow(worksheet, row, fundingCategory, fundingCategory.ContractAllocationNumber);
            ApplyStyleToRow(worksheet, row, _fundingCategoryStyle);
            ApplyFutureMonthStyleToRow(worksheet, row, fundingCategory.CurrentPeriod);

            row = NextRow(worksheet);
            worksheet.Cells.ImportObjectArray
            (
                new object[]
                {
                    fundingCategory.ContractAllocationNumber,
                    fundingCategory.CumulativeFundingCategoryTitle,
                    fundingCategory.CumulativePeriod1,
                    fundingCategory.CumulativePeriod2,
                    fundingCategory.CumulativePeriod3,
                    fundingCategory.CumulativePeriod4,
                    fundingCategory.CumulativePeriod5,
                    fundingCategory.CumulativePeriod6,
                    fundingCategory.CumulativePeriod7,
                    fundingCategory.CumulativePeriod8,
                    fundingCategory.CumulativePeriod9,
                    fundingCategory.CumulativePeriod10,
                    fundingCategory.CumulativePeriod11,
                    fundingCategory.CumulativePeriod12,
                    NotApplicable,
                    NotApplicable,
                    NotApplicable,
                    NotApplicable,
                }, row,
                0,
                false
            );
            ApplyStyleToRow(worksheet, row, _fundingCategoryStyle);
            ApplyFutureMonthStyleToRow(worksheet, row, fundingCategory.CurrentPeriod);

            if (!string.IsNullOrWhiteSpace(fundingCategory.Note))
            {
                row = NextRow(worksheet);
                worksheet.Cells[row, 1].PutValue(fundingCategory.Note);
                ApplyStyleToRow(worksheet, row, _textWrappedStyle);
            }

            return worksheet;
        }

        private Worksheet RenderFundingSummaryReportRow(Worksheet worksheet, int row, IFundingSummaryReportRow fundingSummaryReportRow, string contractAllocationNumber)
        {
            worksheet.Cells.ImportObjectArray(
            new object[]
            {
                contractAllocationNumber,
                fundingSummaryReportRow.Title,
                fundingSummaryReportRow.Period1,
                fundingSummaryReportRow.Period2,
                fundingSummaryReportRow.Period3,
                fundingSummaryReportRow.Period4,
                fundingSummaryReportRow.Period5,
                fundingSummaryReportRow.Period6,
                fundingSummaryReportRow.Period7,
                fundingSummaryReportRow.Period8,
                fundingSummaryReportRow.Period9,
                fundingSummaryReportRow.Period10,
                fundingSummaryReportRow.Period11,
                fundingSummaryReportRow.Period12,
                fundingSummaryReportRow.Period1To8,
                fundingSummaryReportRow.Period9To12,
                fundingSummaryReportRow.YearToDate,
                fundingSummaryReportRow.Total,
            }, row, 0, false);

            return worksheet;
        }

        private Worksheet RenderFundingSubCategory(Worksheet worksheet, FundingSubCategory fundingSubCategory, string contractAllocationNumber)
        {
            var row = NextRow(worksheet) + 1;

            worksheet.Cells.ImportObjectArray(
            new object[]
            {
                fundingSubCategory.FundingSubCategoryTitle,
                $"Aug-{StartYear}",
                $"Sep-{StartYear}",
                $"Oct-{StartYear}",
                $"Nov-{StartYear}",
                $"Dec-{StartYear}",
                $"Jan-{EndYear}",
                $"Feb-{EndYear}",
                $"Mar-{EndYear}",
                $"Apr-{EndYear}",
                $"May-{EndYear}",
                $"Jun-{EndYear}",
                $"Jul-{EndYear}",
                "Aug - Mar",
                "Apr - Jul",
                "Year To Date",
                "Total",
            }, row, 1, false);
            ApplyStyleToRow(worksheet, row, _fundingSubCategoryStyle);

            var renderFundLineGroupTotals = fundingSubCategory.FundLineGroups.Count > 1;

            foreach (var fundLineGroup in fundingSubCategory.FundLineGroups)
            {
                RenderFundLineGroup(worksheet, fundLineGroup, renderFundLineGroupTotals, contractAllocationNumber);
            }

            row = NextRow(worksheet);
            RenderFundingSummaryReportRow(worksheet, row, fundingSubCategory, contractAllocationNumber);
            ApplyStyleToRow(worksheet, row, _fundingSubCategoryStyle);
            ApplyFutureMonthStyleToRow(worksheet, row, fundingSubCategory.CurrentPeriod);

            return worksheet;
        }

        private Worksheet RenderFundLineGroup(Worksheet worksheet, FundLineGroup fundLineGroup, bool renderFundLineGroupTotal, string contractAllocationNumber)
        {
            foreach (var fundLine in fundLineGroup.FundLines)
            {
                RenderFundLine(worksheet, fundLine, contractAllocationNumber);
            }

            if (renderFundLineGroupTotal)
            {
                var row = NextRow(worksheet);
                RenderFundingSummaryReportRow(worksheet, row, fundLineGroup, contractAllocationNumber);
                ApplyStyleToRow(worksheet, row, _fundLineGroupStyle);
                ApplyFutureMonthStyleToRow(worksheet, row, fundLineGroup.CurrentPeriod);
            }

            return worksheet;
        }

        private Worksheet RenderFundLine(Worksheet worksheet, FundLine fundLine, string contractAllocationNumber)
        {
            var row = NextRow(worksheet);

            RenderFundingSummaryReportRow(worksheet, row, fundLine, contractAllocationNumber);
            ApplyFutureMonthStyleToRow(worksheet, row, fundLine.CurrentPeriod);

            return worksheet;
        }

        private Worksheet RenderHeaderOrFooterArray(Worksheet worksheet, int row, IDictionary<string, string> data)
        {
            foreach (var entry in data)
            {
                worksheet.Cells.ImportTwoDimensionArray(new object[,]
                {
                    { entry.Key, entry.Value }
                }, row, 0);

                ApplyStyleToRow(worksheet, row, _headerAndFooterStyle);

                row++;
            }

            return worksheet;
        }

        private int NextRow(Worksheet worksheet)
        {
            return worksheet.Cells.MaxRow + 1;
        }

        private void ApplyStyleToRow(Worksheet worksheet, int row, Style style)
        {
            worksheet.Cells.CreateRange(row, StartColumn, 1, ColumnCount).ApplyStyle(style, _styleFlag);
        }

        private void ApplyFutureMonthStyleToRow(Worksheet worksheet, int row, int currentPeriod)
        {
            var columnCount = 12 - currentPeriod;

            var column = 2 + currentPeriod;

            if (columnCount > 0)
            {
                worksheet.Cells.CreateRange(row, column, 1, columnCount).ApplyStyle(_futureMonthStyle, _italicStyleFlag);
            }
        }

        private void ConfigureStyles()
        {
            _defaultStyle.Font.Size = 10;
            _defaultStyle.Font.Name = "Arial";
            _defaultStyle.SetCustom(DecimalFormat, false);

            _textWrappedStyle.Font.Size = 10;
            _textWrappedStyle.Font.Name = "Arial";
            _textWrappedStyle.IsTextWrapped = true;

            _futureMonthStyle.Font.IsItalic = true;

            _fundingCategoryStyle.ForegroundColor = Color.FromArgb(191, 191, 191);
            _fundingCategoryStyle.Pattern = BackgroundType.Solid;
            _fundingCategoryStyle.Font.Size = 13;
            _fundingCategoryStyle.Font.IsBold = true;
            _fundingCategoryStyle.Font.Name = "Arial";
            _fundingCategoryStyle.SetCustom(DecimalFormat, false);

            _fundingSubCategoryStyle.ForegroundColor = Color.FromArgb(242, 242, 242);
            _fundingSubCategoryStyle.Pattern = BackgroundType.Solid;
            _fundingSubCategoryStyle.Font.Size = 11;
            _fundingSubCategoryStyle.Font.IsBold = true;
            _fundingSubCategoryStyle.Font.Name = "Arial";
            _fundingSubCategoryStyle.SetCustom(DecimalFormat, false);

            _fundLineGroupStyle.Font.Size = 11;
            _fundLineGroupStyle.Font.IsBold = true;
            _fundLineGroupStyle.Font.Name = "Arial";
            _fundLineGroupStyle.SetCustom(DecimalFormat, false);

            _headerAndFooterStyle.Font.Size = 10;
            _headerAndFooterStyle.Font.Name = "Arial";
            _headerAndFooterStyle.Font.IsBold = true;
        }
    }
}
