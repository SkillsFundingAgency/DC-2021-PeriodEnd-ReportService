using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model
{
    public class AppsCoInvestmentRecord
    {
        public AppsCoInvestmentRecordKey RecordKey { get; set; }

        public long? UniqueLearnerNumber { get; set; }

        public string FamilyName { get; set; }

        public string GivenNames { get; set; }

        public LearningDelivery LearningDelivery { get; set; }

        public byte? ApprenticeshipContractType { get; set; }

        public int? EmployerIdentifierAtStartOfLearning { get; set; }

        public string EmployerNameFromApprenticeshipService { get; set; }

        public string LDM356Or361 { get; set; }


        //public decimal? TotalPMRPreviousFundingYears { get; set; }

        //public decimal? TotalCoInvestmentDueFromEmployerInPreviousFundingYears { get; set; }

        //public decimal? TotalPMRThisFundingYear { get; set; }

        //public decimal? TotalCoInvestmentDueFromEmployerThisFundingYear { get; set; }

        //public decimal PercentageOfCoInvestmentCollected { get; set; }

        public EarningsAndPayments EarningsAndPayments { get; set; }

        //public CoInvestmentPaymentsDueFromEmployer CoInvestmentPaymentsDueFromEmployer { get; set; }

        //public decimal CompletionEarningThisFundingYear { get; set; }

        //public decimal CompletionPaymentsThisFundingYear { get; set; }

        //public decimal? EmployerCoInvestmentPercentage { get; set; }

        public AECLearningDelivery AecLearningDelivery { get; set; }
    }
}
