using Aspose.Cells;

namespace ESFA.DC.PeriodEnd.ReportService.Model.Styling
{
    public sealed class CellStyle
    {
        public CellStyle(Style style, StyleFlag styleFlag)
        {
            Style = style;
            StyleFlag = styleFlag;
        }

        public Style Style { get; }

        public StyleFlag StyleFlag { get; }
    }
}
