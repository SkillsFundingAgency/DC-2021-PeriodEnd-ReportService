using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model
{
    public class AppFinRecord
    {
        public string LearnRefNumber { get; set; }

        public int AimSeqNumber { get; set; }

        public string AFinType { get; set; }

        public int AFinCode { get; set; }

        public DateTime AFinDate { get; set; }

        public int AFinAmount { get; set; }
    }
}
