using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR1920.DataStore.EF.Valid;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders.PeriodEnd
{
    public class AppsAdditionalPaymentsModelBuilder : IAppsAdditionalPaymentsModelBuilder
    {
        public IEnumerable<AppsAdditionalPaymentsModel> BuildModel(
             AppsAdditionalPaymentILRInfo appsAdditionalPaymentIlrInfo,
             AppsAdditionalPaymentRulebaseInfo appsAdditionalPaymentRulebaseInfo,
             AppsAdditionalPaymentDasPaymentsInfo appsAdditionalPaymentDasPaymentsInfo)
        {
            List<AppsAdditionalPaymentsModel> appsAdditionalPaymentsModels = new List<AppsAdditionalPaymentsModel>();

            foreach (var paymentInfo in appsAdditionalPaymentDasPaymentsInfo.Payments)
            {
                var model = new AppsAdditionalPaymentsModel()
                {
                    LearnerReferenceNumber = paymentInfo.LearnerReferenceNumber,
                    UniqueLearnerNumber = paymentInfo.LearnerUln,
                    ProviderSpecifiedLearnerMonitoringA = GetProviderSpecMonitor(appsAdditionalPaymentIlrInfo, Generics.ProviderSpecifiedLearnerMonitoringA),
                    ProviderSpecifiedLearnerMonitoringB = GetProviderSpecMonitor(appsAdditionalPaymentIlrInfo, Generics.ProviderSpecifiedLearnerMonitoringB),
                    LearningStartDate = paymentInfo.LearningStartDate,
                    FundingLineType = paymentInfo.LearningAimFundingLineType,
                    EmployerNameFromApprenticeshipService = paymentInfo.EmployerName,
                    TypeOfAdditionalPayment = paymentInfo.TypeOfAdditionalPayment,
                    AugustR01Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.AugustR01),
                    SeptemberR02Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.SeptemberR02),
                    OctoberR03Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.OctoberR03),
                    NovemberR04Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.NovemberR04),
                    DecemberR05Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.DecemberR05),
                    JanuaryR06Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.JanuaryR06),
                    FebruaryR07Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.FebruaryR07),
                    MarchR08Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.MarchR08),
                    AprilR09Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.AprilR09),
                    MayR10Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.MayR10),
                    JuneR11Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.JuneR11),
                    JulyR12Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.JulyR12),
                    R13Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.R13),
                    R14Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.R14)
                };

                foreach (var learner in appsAdditionalPaymentIlrInfo.Learners)
                {
                    var appsAdditionalPaymentLearningDeliveryInfo = learner.LearningDeliveries.SingleOrDefault(x => x.UKPRN == paymentInfo.UkPrn &&
                                                                                                                    x.LearnRefNumber.CaseInsensitiveEquals(paymentInfo.LearnerReferenceNumber) &&
                                                                                                                    x.LearnAimRef.CaseInsensitiveEquals(paymentInfo.LearningAimReference) &&
                                                                                                                    x.LearnStartDate == paymentInfo.LearningStartDate &&
                                                                                                                    x.ProgType == paymentInfo.LearningAimProgrammeType &&
                                                                                                                    x.StdCode == paymentInfo.LearningAimStandardCode &&
                                                                                                                    x.FworkCode == paymentInfo.LearningAimFrameworkCode &&
                                                                                                                    x.PwayCode == paymentInfo.LearningAimPathwayCode);
                    var aecLearningDeliveryInfo = appsAdditionalPaymentLearningDeliveryInfo == null ? null
                        : appsAdditionalPaymentRulebaseInfo.AECLearningDeliveries.SingleOrDefault(x =>
                        x.UKPRN == appsAdditionalPaymentLearningDeliveryInfo.UKPRN &&
                        x.LearnRefNumber.Equals(appsAdditionalPaymentLearningDeliveryInfo.LearnRefNumber, StringComparison.OrdinalIgnoreCase) &&
                        x.AimSeqNumber == appsAdditionalPaymentLearningDeliveryInfo.AimSeqNumber);

                    var aecApprenticeshipPriceEpisodePeriodisedValuesInfo = appsAdditionalPaymentLearningDeliveryInfo == null ? null
                        : appsAdditionalPaymentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues.Where(x =>
                        x.UKPRN == appsAdditionalPaymentLearningDeliveryInfo.UKPRN &&
                        x.LearnRefNumber.Equals(appsAdditionalPaymentLearningDeliveryInfo.LearnRefNumber, StringComparison.OrdinalIgnoreCase) &&
                        x.AimSeqNumber == appsAdditionalPaymentLearningDeliveryInfo.AimSeqNumber).ToList();

                    model.EmployerIdentifierFromILR = GetEmployerIdentifier(aecLearningDeliveryInfo, paymentInfo);
                    model.AugustEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.August);
                    model.SeptemberEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.September);
                    model.OctoberEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.October);
                    model.NovemberEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.November);
                    model.DecemberEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.December);
                    model.JanuaryEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.January);
                    model.FebruaryEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.February);
                    model.MarchEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.March);
                    model.AprilEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.April);
                    model.MayEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.May);
                    model.JuneEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.June);
                    model.JulyEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.July);
                }

                model.TotalEarnings = BuildTotalEarnings(model);
                model.TotalPaymentsYearToDate = BuildTotalPayments(model);
                appsAdditionalPaymentsModels.Add(model);
            }

            return BuildAppsAdditionalPaymentsResultModel(appsAdditionalPaymentsModels);
        }

        private string GetProviderSpecMonitor(AppsAdditionalPaymentILRInfo appsAdditionalPaymentIlrInfo, string providerSpecifiedLearnerMonitoring)
        {
            return appsAdditionalPaymentIlrInfo.Learners
                .Where(l => l.ProviderSpecLearnerMonitorings != null)
                .SelectMany(x => x.ProviderSpecLearnerMonitorings)
                .Where(x => x.ProvSpecLearnMonOccur.Equals(providerSpecifiedLearnerMonitoring, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault()?.ProvSpecLearnMon ?? string.Empty;
        }

        private List<AppsAdditionalPaymentsModel> BuildAppsAdditionalPaymentsResultModel(
            List<AppsAdditionalPaymentsModel> appsAdditionalPaymentsModels)
        {
            return appsAdditionalPaymentsModels.GroupBy(
                x => new
            {
                    LearnerReferenceNumber = (x.LearnerReferenceNumber ?? string.Empty).ToLowerInvariant(),
                    x.UniqueLearnerNumber,
                    x.LearningStartDate,
                    FundingLineType = (x.FundingLineType ?? string.Empty).ToLowerInvariant(),
                    TypeOfAdditionalPayment = (x.TypeOfAdditionalPayment ?? string.Empty).ToLowerInvariant(),
                    EmployerNameFromApprenticeshipService = (x.EmployerNameFromApprenticeshipService ?? string.Empty).ToLowerInvariant(),
                    EmployerIdentifierFromILR = (x.EmployerIdentifierFromILR ?? string.Empty).ToLowerInvariant()
            })
            .Select(x => new AppsAdditionalPaymentsModel()
            {
                LearnerReferenceNumber = x.Key.LearnerReferenceNumber,
                UniqueLearnerNumber = x.Key.UniqueLearnerNumber,
                LearningStartDate = x.Key.LearningStartDate,
                FundingLineType = x.Key.FundingLineType,
                TypeOfAdditionalPayment = x.Key.TypeOfAdditionalPayment,
                EmployerNameFromApprenticeshipService = x.Key.EmployerNameFromApprenticeshipService,
                EmployerIdentifierFromILR = x.Key.EmployerIdentifierFromILR,
                ProviderSpecifiedLearnerMonitoringA = x.First().ProviderSpecifiedLearnerMonitoringA,
                ProviderSpecifiedLearnerMonitoringB = x.First().ProviderSpecifiedLearnerMonitoringB,
                AugustEarnings = x.Sum(e => e.AugustEarnings),
                SeptemberEarnings = x.Sum(e => e.SeptemberEarnings),
                OctoberEarnings = x.Sum(e => e.OctoberEarnings),
                NovemberEarnings = x.Sum(e => e.NovemberEarnings),
                DecemberEarnings = x.Sum(e => e.DecemberEarnings),
                JanuaryEarnings = x.Sum(e => e.JanuaryEarnings),
                FebruaryEarnings = x.Sum(e => e.FebruaryEarnings),
                MarchEarnings = x.Sum(e => e.MarchEarnings),
                AprilEarnings = x.Sum(e => e.AprilEarnings),
                MayEarnings = x.Sum(e => e.MayEarnings),
                JuneEarnings = x.Sum(e => e.JuneEarnings),
                JulyEarnings = x.Sum(e => e.JulyEarnings),
                AugustR01Payments = x.Sum(p => p.AugustR01Payments),
                SeptemberR02Payments = x.Sum(p => p.SeptemberR02Payments),
                OctoberR03Payments = x.Sum(p => p.OctoberR03Payments),
                NovemberR04Payments = x.Sum(p => p.NovemberR04Payments),
                DecemberR05Payments = x.Sum(p => p.DecemberR05Payments),
                JanuaryR06Payments = x.Sum(p => p.JanuaryR06Payments),
                FebruaryR07Payments = x.Sum(p => p.FebruaryR07Payments),
                MarchR08Payments = x.Sum(p => p.MarchR08Payments),
                AprilR09Payments = x.Sum(p => p.AprilR09Payments),
                MayR10Payments = x.Sum(p => p.MayR10Payments),
                JuneR11Payments = x.Sum(p => p.JuneR11Payments),
                JulyR12Payments = x.Sum(p => p.JulyR12Payments),
                R13Payments = x.Sum(p => p.R13Payments),
                R14Payments = x.Sum(p => p.R14Payments),
                TotalEarnings = x.Sum(p => p.TotalEarnings),
                TotalPaymentsYearToDate = x.Sum(p => p.TotalPaymentsYearToDate)
            })
            .OrderBy(a => a.LearnerReferenceNumber)
            .ToList();
        }

        private decimal BuildTotalEarnings(AppsAdditionalPaymentsModel model)
        {
            return model.AugustEarnings +
                model.SeptemberEarnings +
                model.OctoberEarnings +
                model.NovemberEarnings +
                model.DecemberEarnings +
                model.JanuaryEarnings +
                model.FebruaryEarnings +
                model.MarchEarnings +
                model.AprilEarnings +
                model.MayEarnings +
                model.JuneEarnings +
                model.JulyEarnings;
        }

        private decimal BuildTotalPayments(AppsAdditionalPaymentsModel model)
        {
            return model.AugustR01Payments +
                model.SeptemberR02Payments +
                model.OctoberR03Payments +
                model.NovemberR04Payments +
                model.DecemberR05Payments +
                model.JanuaryR06Payments +
                model.FebruaryR07Payments +
                model.MarchR08Payments +
                model.AprilR09Payments +
                model.MayR10Payments +
                model.JuneR11Payments +
                model.JulyR12Payments +
                model.R13Payments +
                model.R14Payments;
        }

        private decimal GetMonthlyPayments(DASPaymentInfo paymentInfo, string collectionPeriodName)
        {
            return paymentInfo.CollectionPeriod.ToCollectionPeriodName(paymentInfo.AcademicYear.ToString()).Equals(collectionPeriodName, StringComparison.OrdinalIgnoreCase) ? paymentInfo.Amount : 0;
        }

        private decimal GetMonthlyEarnings(
            DASPaymentInfo paymentInfo,
            List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aecApprenticeshipPriceEpisodePeriodisedValuesInfo,
            int month)
        {
            decimal? result = 0;
            if (paymentInfo.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive || paymentInfo.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive)
            {
                result = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Periods[month] ?? 0 +
                         aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Periods[month] ?? 0;
            }

            if (paymentInfo.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive || paymentInfo.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive)
            {
                result = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Periods[month] ?? 0 +
                         aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Periods[month] ?? 0;
            }

            if (paymentInfo.TransactionType == Constants.DASPayments.TransactionType.Apprenticeship)
            {
                result = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName, StringComparison.OrdinalIgnoreCase))?.Periods[month] ?? 0;
            }

            return result.GetValueOrDefault();
        }

        private string GetEmployerIdentifier(AECLearningDeliveryInfo aecLearningDeliveryInfo, DASPaymentInfo paymentInfo)
        {
            var identifier = 0;
            if (aecLearningDeliveryInfo != null)
            {
                if (paymentInfo.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive) // 4
                {
                    identifier = aecLearningDeliveryInfo.LearnDelEmpIdFirstAdditionalPaymentThreshold.GetValueOrDefault();
                }

                if (paymentInfo.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive) //6
                {
                    identifier = aecLearningDeliveryInfo.LearnDelEmpIdSecondAdditionalPaymentThreshold.GetValueOrDefault();
                }
            }

            return identifier == 0 ? Generics.NotAvailable : identifier.ToString();
        }
    }
}
