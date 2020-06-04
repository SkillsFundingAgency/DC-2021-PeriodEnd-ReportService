using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.Periodend.ReportService.Reports.Interface.AppsMonthly.Model
{
    public class Payment
    {
        public string LearnerReferenceNumber { get; set; }

        public long LearnerUln { get; set; }

        public string LearningAimReference { get; set; }

        public DateTime? LearningStartDate { get; set; }

        public int LearningAimProgrammeType { get; set; }

        public int LearningAimStandardCode { get; set; }

        public int LearningAimFrameworkCode { get; set; }

        public int LearningAimPathwayCode { get; set; }

        public string ReportingAimFundingLineType { get; set; }

        public string PriceEpisodeIdentifier { get; set; }
    }
}
