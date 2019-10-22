using System;
using System.Collections.Generic;

namespace ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment
{
    public class AppsAdditionalPaymentExtendedPaymentModel
    {
        // ---------------------------------------------------------------------
        // Report grouping key
        // ---------------------------------------------------------------------
        public string PaymentLearnerReferenceNumber { get; set; }

        public long PaymentUniqueLearnerNumber { get; set; }

        public DateTime? PaymentLearningStartDate { get; set; }

        public string PaymentLearningAimFundingLineType { get; set; }

        public string PaymentTypeOfAdditionalPayment { get; set; }

        public string AppsServiceEmployerName { get; set; }

        public string ilrEmployerIdentifier { get; set; }

        public string PaymentLearningAimReference { get; set; }

        // ---------------------------------------------------------------------
        // Payments2.Payments fields
        // ---------------------------------------------------------------------
        public byte PaymentTransactionType { get; set; }

        public string LegalEntityName { get; set; }

        public int PaymentLearningAimProgrammeType { get; set; }

        public int PaymentLearningAimStandardCode { get; set; }

        public int PaymentLearningAimFrameworkCode { get; set; }

        public int PaymentLearningAimPathwayCode { get; set; }

        public byte PaymentContractType { get; set; }

        public byte PaymentDeliveryPeriod { get; set; }

        public byte PaymentCollectionPeriod { get; set; }

        public short PaymentAcademicYear { get; set; }

        public decimal? PaymentAmount { get; set; }

        public byte PaymentFundingSource { get; set; }

        public string ProviderSpecifiedLearnerMonitoringA { get; set; }

        public string ProviderSpecifiedLearnerMonitoringB { get; set; }

        public decimal? EarningAmount { get; set; }
    }
}
