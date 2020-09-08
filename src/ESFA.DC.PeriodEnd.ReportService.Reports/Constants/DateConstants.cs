using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Constants
{
    public class DateConstants
    {
        public const string Year = "2020/21";
        public const int AcademicYear = 2021;
        public const int PreviousFundingYear = 1920;
        public static readonly DateTime BeginningOfYear = new DateTime(2020, 8, 1);
        public static readonly DateTime EndOfYear = new DateTime(2021, 7, 31, 23, 59, 59);
    }
}
