using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model
{
    public class EarningsAndPayments
    {
        public decimal? TotalCoInvestmentDueFromEmployerThisFundingYear { get; set; }
        public decimal? TotalCoInvestmentDueFromEmployerInPreviousFundingYears { get; set; }
        public decimal? TotalPMRPreviousFundingYears { get; set; }
        public decimal? TotalPMRThisFundingYear { get; set; }
        public decimal PercentageOfCoInvestmentCollected { get; set; }
        public decimal? EmployerCoInvestmentPercentage { get; set; }
        public decimal CompletionEarningThisFundingYear { get; set; }
        public decimal CompletionPaymentsThisFundingYear { get; set; }

        public CoInvestmentPaymentsDueFromEmployer CoInvestmentPaymentsDueFromEmployer { get; set; }
    }
}
