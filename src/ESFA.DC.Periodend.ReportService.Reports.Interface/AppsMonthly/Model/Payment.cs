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

        public Guid? EarningEventId { get; set; }

        public byte CollectionPeriod { get; set; }

        public byte DeliveryPeriod { get; set; }

        public decimal Amount { get; set; }

        public byte TransactionType { get; set; }

        public byte FundingSource { get; set; }
    }
}
