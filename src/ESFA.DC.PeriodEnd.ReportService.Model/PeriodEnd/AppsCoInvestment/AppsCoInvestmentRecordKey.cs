using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment
{
    public struct AppsCoInvestmentRecordKey
    {
        public AppsCoInvestmentRecordKey(
            string learnRefNumber,
            DateTime? learnStartDate,
            int progType,
            int stdCode,
            int fworkCode,
            int pwayCode)
        {
            LearningAimReference = "ZPROG001";
            LearnerReferenceNumber = learnRefNumber;
            LearningStartDate = learnStartDate;
            LearningAimProgrammeType = progType;
            LearningAimStandardCode = stdCode;
            LearningAimFrameworkCode = fworkCode;
            LearningAimPathwayCode = pwayCode;
        }

        public string LearningAimReference { get; set; }

        public string LearnerReferenceNumber { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public int LearningAimProgrammeType { get; set; }

        public int LearningAimStandardCode { get; set; }

        public int LearningAimFrameworkCode { get; set; }

        public int LearningAimPathwayCode { get; set; }
    }
}
