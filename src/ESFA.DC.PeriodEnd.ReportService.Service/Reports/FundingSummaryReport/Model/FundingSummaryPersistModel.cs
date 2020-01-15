using System;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport.Model
{
    public class FundingSummaryPersistModel : AbstractReportModel
    {
        public int? Ukprn { get; set; }

        public string ContractNo { get; set; }

        public string FundingCategory { get; set; }

        public string FundingSubCategory { get; set; }

        public string FundLine { get; set; }

        public decimal Aug19 { get; set; }

        public decimal Sep19 { get; set; }

        public decimal Oct19 { get; set; }

        public decimal Nov19 { get; set; }

        public decimal Dec19 { get; set; }

        public decimal Jan20 { get; set; }

        public decimal Feb20 { get; set; }

        public decimal Mar20 { get; set; }

        public decimal Apr20 { get; set; }

        public decimal May20 { get; set; }

        public decimal Jun20 { get; set; }

        public decimal Jul20 { get; set; }

        public decimal AugMar { get; set; }

        public decimal AprJul { get; set; }

        public decimal YearToDate { get; set; }

        public decimal Total { get; set; }
    }
}
