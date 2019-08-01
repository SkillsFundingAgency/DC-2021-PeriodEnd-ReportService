using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders.PeriodEnd
{
    public class AppsAdditionalPaymentsModelBuilder : IAppsAdditionalPaymentsModelBuilder
    {
        public List<AppsAdditionalPaymentsModel> BuildModel(
            AppsAdditionalPaymentILRInfo appsAdditionalPaymentIlrInfo,
            AppsAdditionalPaymentRulebaseInfo appsAdditionalPaymentRulebaseInfo,
            AppsAdditionalPaymentDasPaymentsInfo appsAdditionalPaymentDasPaymentsInfo)
        {
            List<AppsAdditionalPaymentsModel> appsAdditionalPaymentsModels = new List<AppsAdditionalPaymentsModel>();

            foreach (var learner in appsAdditionalPaymentIlrInfo.Learners)
            {
                foreach (var paymentInfo in appsAdditionalPaymentDasPaymentsInfo.Payments)
                {
                    var appsAdditionalPaymentLearningDeliveryInfo = learner.LearningDeliveries.SingleOrDefault(x => x.UKPRN == paymentInfo.UkPrn &&
                                                                                                                     x.LearnRefNumber ==
                                                                                                                     paymentInfo.LearnerReferenceNumber &&
                                                                                                                     x.LearnAimRef == paymentInfo.LearningAimReference &&
                                                                                                                     x.LearnStartDate == paymentInfo.LearningStartDate &&
                                                                                                                     x.ProgType == paymentInfo.LearningAimProgrammeType &&
                                                                                                                     x.StdCode == paymentInfo.LearningAimStandardCode &&
                                                                                                                     x.FworkCode == paymentInfo.LearningAimFrameworkCode &&
                                                                                                                     x.PwayCode == paymentInfo.LearningAimPathwayCode);
                    var aecLearningDeliveryInfo = appsAdditionalPaymentRulebaseInfo.AECLearningDeliveries.SingleOrDefault(x =>
                        x.UKPRN == appsAdditionalPaymentLearningDeliveryInfo?.UKPRN &&
                        x.LearnRefNumber == appsAdditionalPaymentLearningDeliveryInfo.LearnRefNumber &&
                        x.AimSeqNumber == appsAdditionalPaymentLearningDeliveryInfo.AimSeqNumber);

                    var aecApprenticeshipPriceEpisodePeriodisedValuesInfo = appsAdditionalPaymentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues.Where(x =>
                        x.UKPRN == appsAdditionalPaymentLearningDeliveryInfo?.UKPRN &&
                        x.LearnRefNumber == appsAdditionalPaymentLearningDeliveryInfo.LearnRefNumber &&
                        x.AimSeqNumber == appsAdditionalPaymentLearningDeliveryInfo.AimSeqNumber).ToList();

                    var model = new AppsAdditionalPaymentsModel()
                    {
                        LearnerReferenceNumber = paymentInfo.LearnerReferenceNumber,
                        UniqueLearnerNumber = paymentInfo.LearnerUln,
                        ProviderSpecifiedLearnerMonitoringA = learner.ProviderSpecLearnerMonitorings?.SingleOrDefault(psm =>
                            string.Equals(psm.ProvSpecLearnMonOccur, "A", StringComparison.OrdinalIgnoreCase))?.ProvSpecLearnMon,
                        ProviderSpecifiedLearnerMonitoringB = learner.ProviderSpecLearnerMonitorings?.SingleOrDefault(psm =>
                            string.Equals(psm.ProvSpecLearnMonOccur, "B", StringComparison.OrdinalIgnoreCase))?.ProvSpecLearnMon,
                        LearningStartDate = paymentInfo.LearningStartDate,
                        FundingLineType = paymentInfo.LearningAimFundingLineType,
                        EmployerNameFromApprenticeshipService = paymentInfo.EmployerName,
                        EmployerIdentifierFromILR = GetEmployerIdentifier(aecLearningDeliveryInfo, paymentInfo),
                        TypeOfAdditionalPayment = paymentInfo.TypeOfAdditionalPayment,
                        AugustEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 0),
                        SeptemberEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 1),
                        OctoberEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 2),
                        NovemberEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 3),
                        DecemberEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 4),
                        JanuaryEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 5),
                        FebruaryEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 6),
                        MarchEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 7),
                        AprilEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 8),
                        MayEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 9),
                        JuneEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 10),
                        JulyEarnings = GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, 11),
                        AugustR01Payments = GetMonthlyPayments(paymentInfo, "1819-R01"),
                        SeptemberR02Payments = GetMonthlyPayments(paymentInfo, "1819-R02"),
                        OctoberR03Payments = GetMonthlyPayments(paymentInfo, "1819-R03"),
                        NovemberR04Payments = GetMonthlyPayments(paymentInfo, "1819-R04"),
                        DecemberR05Payments = GetMonthlyPayments(paymentInfo, "1819-R05"),
                        JanuaryR06Payments = GetMonthlyPayments(paymentInfo, "1819-R06"),
                        FebruaryR07Payments = GetMonthlyPayments(paymentInfo, "1819-R07"),
                        MarchR08Payments = GetMonthlyPayments(paymentInfo, "1819-R08"),
                        AprilR09Payments = GetMonthlyPayments(paymentInfo, "1819-R09"),
                        MayR10Payments = GetMonthlyPayments(paymentInfo, "1819-R10"),
                        JuneR11Payments = GetMonthlyPayments(paymentInfo, "1819-R11"),
                        JulyR12Payments = GetMonthlyPayments(paymentInfo, "1819-R12"),
                        R13Payments = GetMonthlyPayments(paymentInfo, "1819-R13"),
                        R14Payments = GetMonthlyPayments(paymentInfo, "1819-R14")
                    };
                    model.TotalEarnings = BuildTotalEarnings(model);
                    model.TotalPaymentsYearToDate = BuildTotalPayments(model);
                    appsAdditionalPaymentsModels.Add(model);
                }
            }

            appsAdditionalPaymentsModels = BuildAppsAdditionalPaymentsResultModel(appsAdditionalPaymentsModels);

            return appsAdditionalPaymentsModels;
        }

        private List<AppsAdditionalPaymentsModel> BuildAppsAdditionalPaymentsResultModel(List<AppsAdditionalPaymentsModel> appsAdditionalPaymentsModels)
        {
            appsAdditionalPaymentsModels = appsAdditionalPaymentsModels.GroupBy(x => new
            {
                x.LearnerReferenceNumber,
                x.UniqueLearnerNumber,
                x.LearningStartDate,
                x.FundingLineType,
                x.TypeOfAdditionalPayment,
                x.EmployerNameFromApprenticeshipService,
                x.EmployerIdentifierFromILR
            }).Select(x => new AppsAdditionalPaymentsModel()
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
            }).ToList();
            return appsAdditionalPaymentsModels;
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
            return paymentInfo.CollectionPeriod.ToCollectionPeriodName(paymentInfo.AcademicYear.ToString()).Equals(collectionPeriodName) ? paymentInfo.Amount : 0;
        }

        private decimal GetMonthlyEarnings(DASPaymentInfo paymentInfo, List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aecApprenticeshipPriceEpisodePeriodisedValuesInfo, int month)
        {
            decimal? result = 0;
            if (paymentInfo.TransactionType == 4 || paymentInfo.TransactionType == 6)
            {
                result = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Constants.Fm36PriceEpisodeFirstEmp1618PayAttributeName))?.Periods[month] ?? 0 +
                         aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Constants.Fm36PriceEpisodeSecondEmp1618PayAttributeName))?.Periods[month] ?? 0;
            }

            if (paymentInfo.TransactionType == 5 || paymentInfo.TransactionType == 7)
            {
                result = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Constants.Fm36PriceEpisodeFirstProv1618PayAttributeName))?.Periods[month] ?? 0 +
                         aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Constants.Fm36PriceEpisodeSecondProv1618PayAttributeName))?.Periods[month] ?? 0;
            }

            if (paymentInfo.TransactionType == 16)
            {
                result = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Constants.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName))?.Periods[month] ?? 0;
            }

            return result.GetValueOrDefault();
        }

        private string GetEmployerIdentifier(AECLearningDeliveryInfo aecLearningDeliveryInfo, DASPaymentInfo paymentInfo)
        {
            var identifier = 0;
            if (aecLearningDeliveryInfo != null)
            {
                if (paymentInfo.TransactionType == 4)
                {
                    identifier = aecLearningDeliveryInfo.LearnDelEmpIdFirstAdditionalPaymentThreshold.GetValueOrDefault();
                }

                if (paymentInfo.TransactionType == 6)
                {
                    identifier = aecLearningDeliveryInfo.LearnDelEmpIdSecondAdditionalPaymentThreshold.GetValueOrDefault();
                }
            }

            return identifier == 0 ? Constants.NotAvailable : identifier.ToString();
        }
    }
}
