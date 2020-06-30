using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Tests.AppsCoInvestment.Builders
{
    public class AppFindRecordBuilder : AbstractBuilder<AppFinRecord>
    {
        public const string LearnRefNumber = "LearnRefNumber";

        public const int AimSeqNumber = 1;

        public const string AFinType = "PMR";

        public const int AFinCode = 1;

        public static DateTime AFinDate = new DateTime(2020, 8, 1);

        public const int AFinAmount = 10;

        public AppFindRecordBuilder()
        {
            modelObject = new AppFinRecord()
            {
                LearnRefNumber = LearnRefNumber,
                AimSeqNumber = AimSeqNumber,
                AFinType = AFinType,
                AFinCode = AFinCode,
                AFinDate = AFinDate,
                AFinAmount = AFinAmount
            };
        }
    }
}
