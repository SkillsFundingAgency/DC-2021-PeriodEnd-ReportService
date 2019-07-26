using System;

namespace ESFA.DC.ILR1819.ReportService.Model.ReportModels
{
    public class AppsAdditionalPaymentsModel
    {
        public string LearnerReferenceNumber { get; set; }

        public long UniqueLearnerNumber { get; set; }

        public string ProviderSpecifiedLearnerMonitoringA { get; set; }

        public string ProviderSpecifiedLearnerMonitoringB { get; set; }

        public DateTime LearningStartDate { get; set; }

        public string FundingLineType { get; set; }

        public string TypeOfAdditionalPayment { get; set; }

        public string EmployerNameFromApprenticeshipService { get; set; }

        public string EmployerIdentifierFromILR { get; set; }

        public decimal? AugustEarnings { get; set; }

        public decimal? AugustR01Payments { get; set; }

        public decimal? SeptemberEarnings { get; set; }

        public decimal? SeptemberR02Payments { get; set; }

        public decimal? OctoberEarnings { get; set; }

        public decimal? OctoberR03Payments { get; set; }

        public decimal? NovemberEarnings { get; set; }

        public decimal? NovemberR04Payments { get; set; }

        public decimal? DecemberEarnings { get; set; }

        public decimal? DecemberR05Payments { get; set; }

        public decimal? JanuaryEarnings { get; set; }

        public decimal? JanuaryR06Payments { get; set; }

        public decimal? FebruaryEarnings { get; set; }

        public decimal? FebruaryR07Payments { get; set; }

        public decimal? MarchEarnings { get; set; }

        public decimal? MarchR08Payments { get; set; }

        public decimal? AprilEarnings { get; set; }

        public decimal? AprilR09Payments { get; set; }

        public decimal? MayEarnings { get; set; }

        public decimal? MayR10Payments { get; set; }

        public decimal? JuneEarnings { get; set; }

        public decimal? JuneR11Payments { get; set; }

        public decimal? JulyEarnings { get; set; }

        public decimal? JulyR12Payments { get; set; }

        public decimal? R13Payments { get; set; }

        public decimal? R14Payments { get; set; }

        public decimal? TotalEarnings { get; set; }

        public decimal? TotalPaymentsYearToDate { get; set; }

        public string OfficialSensitive { get; }
    }
}