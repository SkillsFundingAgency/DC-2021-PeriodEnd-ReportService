using System;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Extensions
{
    public static class DateTimeExtensions
    {
        public static string ShortDateStringFormat(this DateTime source)
        {
            return source.Date.ToString(FormattingConstants.ShortDateStringFormat);
        }

        public static string LongDateStringFormat(this DateTime source)
        {
            return source.ToString(FormattingConstants.LongDateTimeStringFormat);
        }

        public static string TimeOfDayOnDateStringFormat(this DateTime source)
        {
            return source.ToString(FormattingConstants.TimeofDayOnDateStringFormat);
        }
    }
}
