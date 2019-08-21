namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment
{
    public class PaymentInfo
    {
        public string LearnerReferenceNumber { get; set; }

        public string LearningAimReference { get; set; }

        public long LearnerUln { get; set; }

        //missing LearningStartDate, LegalEntityName

        public int LearningAimProgrammeType { get; set; }

        public int LearningAimStandardCode { get; set; }

        public int LearningAimFrameworkCode { get; set; }

        public int LearningAimPathwayCode { get; set; }

        public byte ContractType { get; set; }

        public byte DeliveryPeriod { get; set; }

        public byte CollectionPeriod { get; set; }

        public short AcademicYear { get; set; }

        public byte TransactionType { get; set; }

        public byte FundingSource { get; set; }
    }
}