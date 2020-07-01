using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Persistance;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsAdditionalPayments
{
    public class AppsAdditionalPaymentPersistanceMapper : IAppsAdditionalPaymentPersistanceMapper
    {
        public IEnumerable<ReportData.Model.AppsAdditionalPayment> Map(IReportServiceContext reportServiceContext, IEnumerable<AppsAdditionalPaymentReportModel> appsAdditionalPaymentReportModels, CancellationToken cancellationToken)
        {
            return appsAdditionalPaymentReportModels.Select(aaprm => new ESFA.DC.ReportData.Model.AppsAdditionalPayment
            {
                Ukprn = reportServiceContext.Ukprn,
                ReturnPeriod = reportServiceContext.ReturnPeriod,
                LearnerReferenceNumber = aaprm.RecordKey.LearnerReferenceNumber,
                UniqueLearnerNumber = aaprm.RecordKey.Uln,
                LearningStartDate = aaprm.RecordKey.LearnStartDate,
                FundingLineType = aaprm.RecordKey.LearningAimFundingLineType,
                TypeOfAdditionalPayment = aaprm.RecordKey.PaymentType,
                EmployerNameFromApprenticeshipService = aaprm.RecordKey.EmployerName,
                EmployerIdentifierFromILR = aaprm.RecordKey.EmployerId,
                FamilyName = aaprm.FamilyName,
                GivenNames = aaprm.GivenNames,
                ProviderSpecifiedLearnerMonitoringA = aaprm.ProviderSpecifiedLearnerMonitoringA,
                ProviderSpecifiedLearnerMonitoringB = aaprm.ProviderSpecifiedLearnerMonitoringB,
                AugustEarnings = aaprm.EarningsAndPayments.AugustEarnings,
                AugustR01Payments = aaprm.EarningsAndPayments.AugustR01Payments,
                SeptemberEarnings = aaprm.EarningsAndPayments.SeptemberEarnings,
                SeptemberR02Payments = aaprm.EarningsAndPayments.SeptemberR02Payments,
                OctoberEarnings = aaprm.EarningsAndPayments.OctoberEarnings,
                OctoberR03Payments = aaprm.EarningsAndPayments.OctoberR03Payments,
                NovemberEarnings = aaprm.EarningsAndPayments.NovemberEarnings,
                NovemberR04Payments = aaprm.EarningsAndPayments.NovemberR04Payments,
                DecemberEarnings = aaprm.EarningsAndPayments.DecemberEarnings,
                DecemberR05Payments = aaprm.EarningsAndPayments.DecemberR05Payments,
                JanuaryEarnings = aaprm.EarningsAndPayments.JanuaryEarnings,
                JanuaryR06Payments = aaprm.EarningsAndPayments.JanuaryR06Payments,
                FebruaryEarnings = aaprm.EarningsAndPayments.FebruaryEarnings,
                FebruaryR07Payments = aaprm.EarningsAndPayments.FebruaryR07Payments,
                MarchEarnings = aaprm.EarningsAndPayments.MarchEarnings,
                MarchR08Payments = aaprm.EarningsAndPayments.MarchR08Payments,
                AprilEarnings = aaprm.EarningsAndPayments.AprilEarnings,
                AprilR09Payments = aaprm.EarningsAndPayments.AprilR09Payments,
                MayEarnings = aaprm.EarningsAndPayments.MayEarnings,
                MayR10Payments = aaprm.EarningsAndPayments.MayR10Payments,
                JuneEarnings = aaprm.EarningsAndPayments.JuneEarnings,
                JuneR11Payments = aaprm.EarningsAndPayments.JuneR11Payments,
                JulyEarnings = aaprm.EarningsAndPayments.JulyEarnings,
                JulyR12Payments = aaprm.EarningsAndPayments.JulyR12Payments,
                R13Payments = aaprm.EarningsAndPayments.R13Payments,
                R14Payments = aaprm.EarningsAndPayments.R14Payments,
                TotalEarnings = aaprm.EarningsAndPayments.TotalEarnings,
                TotalPaymentsYearToDate = aaprm.EarningsAndPayments.TotalPaymentsYearToDate,
            });
        }
    }
}