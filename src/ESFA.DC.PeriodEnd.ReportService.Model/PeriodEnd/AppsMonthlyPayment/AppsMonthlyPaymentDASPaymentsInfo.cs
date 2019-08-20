using System;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment
{
    public class AppsMonthlyPaymentDASPaymentInfo
    {
        public string Ukprn { get; set; }       // value used to confirm the database results are the Ukprn we requested

        public string LearnerReferenceNumber { get; set; }

        public string LearnerUln { get; set; }

        public string LearningAimReference { get; set; }

        public string LearningAimProgrammeType { get; set; }

        public string LearningAimStandardCode { get; set; }

        public string LearningAimFrameworkCode { get; set; }

        public string LearningAimPathwayCode { get; set; }

        public string PriceEpisodeId { get; set; }

        private string _priceEpisodeStartDate = string.Empty;
        public string PriceEpisodeStartDate
        {
            get
            {
                if (!string.IsNullOrEmpty(PriceEpisodeId))
                {
                    string _priceEpisodeStartDate = PriceEpisodeId.Substring((PriceEpisodeId.Length - 10), 10); ;
                }
                return _priceEpisodeStartDate;
            }
        }
        public string LearningAimFundingLineType { get; set; }

        public string ReportingAimFundingLineType { get; set; }

        public string PriceEpisodeIdentifier { get; set; }

        public string ContractType { get; set; }

        public string TransactionType { get; set; }

        public string FundingSource { get; set; }

        public string DeliveryPeriod { get; set; }

        public string CollectionPeriod { get; set; }

        public string AcademicYear { get; set; }

        public Decimal Amount { get; set; }
        public string LearningStartDate { get; set; }
        public Guid EarningEventId { get; set; }
    }
}