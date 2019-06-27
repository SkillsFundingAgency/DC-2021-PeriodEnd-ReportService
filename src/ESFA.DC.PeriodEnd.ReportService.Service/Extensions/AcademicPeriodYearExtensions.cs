using System.Globalization;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Extensions
{
    public static class AcademicPeriodYearExtensions
    {
        public static string ToCollectionPeriodName(this byte periodNumber, string year)
        {
            return $"{year}-R{periodNumber.ToString("00", NumberFormatInfo.InvariantInfo)}";
        }
    }
}
