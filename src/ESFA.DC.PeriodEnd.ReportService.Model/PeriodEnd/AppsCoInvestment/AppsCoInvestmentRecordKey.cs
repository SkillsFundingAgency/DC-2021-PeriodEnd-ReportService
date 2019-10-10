using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment
{
    public class AppsCoInvestmentRecordKey
    {
        private const string ZPROG001 = "ZPROG001";

        public string LearnerReferenceNumber { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public int LearningAimProgrammeType { get; set; }

        public int LearningAimStandardCode { get; set; }

        public int LearningAimFrameworkCode { get; set; }

        public int LearningAimPathwayCode { get; set; }

        public string LearningAimReference => ZPROG001;
    }
}
