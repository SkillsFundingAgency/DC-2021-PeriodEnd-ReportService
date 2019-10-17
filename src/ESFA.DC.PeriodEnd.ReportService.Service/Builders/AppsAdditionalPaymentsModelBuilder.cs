using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class AppsAdditionalPaymentsModelBuilder : IAppsAdditionalPaymentsModelBuilder
    {
        public IEnumerable<AppsAdditionalPaymentsModel> BuildModel(
            AppsAdditionalPaymentILRInfo appsAdditionalPaymentIlrInfo,
            AppsAdditionalPaymentRulebaseInfo appsAdditionalPaymentRulebaseInfo,
            AppsAdditionalPaymentDasPaymentsInfo appsAdditionalPaymentDasPaymentsInfo)
        {
            List<AppsAdditionalPaymentsModel> appsAdditionalPaymentsModels = new List<AppsAdditionalPaymentsModel>();

            // Create an extended payments model that includes the related ILR data
            var extendedPayments = BuildAdditionalPaymentsExtendedPaymentsModel(
                appsAdditionalPaymentIlrInfo,
                appsAdditionalPaymentRulebaseInfo,
                appsAdditionalPaymentDasPaymentsInfo);

            /*
             * Group the payments by BR1:
             *    LearnerReferenceNumber
             *    UniqueLearnerNumber
             *    LearningStartDate
             *    FundingLineType
             *    TypeOfAdditionalPayment
             *    AppServiceEmployerName
             *    IlrEmployerIdentifier
             */
            var additionalPaymentGroups = extendedPayments
                .GroupBy(x => new
                {
                    x.PaymentLearnerReferenceNumber,
                    x.PaymentUniqueLearnerNumber,
                    x.PaymentLearningStartDate,
                    x.PaymentLearningAimFundingLineType,
                    x.PaymentTypeOfAdditionalPayment,
                    x.AppsServiceEmployerName,
                    x.ilrEmployerIdentifier
                })
                .OrderBy(o => o.Key.PaymentLearnerReferenceNumber)
                .ThenBy(o => o.Key.PaymentUniqueLearnerNumber)
                .ThenBy(o => o.Key.PaymentLearningStartDate)
                .ThenBy(o => o.Key.PaymentLearningAimFundingLineType)
                .ThenBy(o => o.Key.PaymentTypeOfAdditionalPayment)
                .ThenBy(o => o.Key.AppsServiceEmployerName)
                .ThenBy(o => o.Key.ilrEmployerIdentifier)
                .Select(g => new AppsAdditionalPaymentsModel
                {
                    // group key fields
                    LearnerReferenceNumber = g.Key.PaymentLearnerReferenceNumber,
                    UniqueLearnerNumber = g.Key.PaymentUniqueLearnerNumber,
                    LearningStartDate = g.Key.PaymentLearningStartDate,
                    FundingLineType = g.Key.PaymentLearningAimFundingLineType,
                    TypeOfAdditionalPayment = g.Key.PaymentTypeOfAdditionalPayment,
                    EmployerNameFromApprenticeshipService = g.Key.AppsServiceEmployerName,
                    EmployerIdentifierFromILR = g.Key.ilrEmployerIdentifier,

                    // other fields
                    ProviderSpecifiedLearnerMonitoringA = g?.FirstOrDefault()?.ProviderSpecifiedLearnerMonitoringA,
                    ProviderSpecifiedLearnerMonitoringB = g?.FirstOrDefault()?.ProviderSpecifiedLearnerMonitoringB,

                    // period totals
                    AugustEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    AugustR01Payments = g.Where(p => PeriodPaymentsPredicate(p, 1)).Sum(c => c.PaymentAmount ?? 0m),
                    SeptemberEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    SeptemberR02Payments = g.Where(p => PeriodPaymentsPredicate(p, 2)).Sum(c => c.PaymentAmount ?? 0m),
                    OctoberEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    OctoberR03Payments = g.Where(p => PeriodPaymentsPredicate(p, 3)).Sum(c => c.PaymentAmount ?? 0m),
                    NovemberEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    NovemberR04Payments = g.Where(p => PeriodPaymentsPredicate(p, 4)).Sum(c => c.PaymentAmount ?? 0m),
                    DecemberEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    DecemberR05Payments = g.Where(p => PeriodPaymentsPredicate(p, 5)).Sum(c => c.PaymentAmount ?? 0m),
                    JanuaryEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    JanuaryR06Payments = g.Where(p => PeriodPaymentsPredicate(p, 6)).Sum(c => c.PaymentAmount ?? 0m),
                    FebruaryEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    FebruaryR07Payments = g.Where(p => PeriodPaymentsPredicate(p, 7)).Sum(c => c.PaymentAmount ?? 0m),
                    MarchEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    MarchR08Payments = g.Where(p => PeriodPaymentsPredicate(p, 8)).Sum(c => c.PaymentAmount ?? 0m),
                    AprilEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    AprilR09Payments = g.Where(p => PeriodPaymentsPredicate(p, 9)).Sum(c => c.PaymentAmount ?? 0m),
                    MayEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    MayR10Payments = g.Where(p => PeriodPaymentsPredicate(p, 10)).Sum(c => c.PaymentAmount ?? 0m),
                    JuneEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    JuneR11Payments = g.Where(p => PeriodPaymentsPredicate(p, 11)).Sum(c => c.PaymentAmount ?? 0m),
                    JulyEarnings = g.Where(p => PeriodEarningsPredicate(p, 1)).Sum(c => c.EarningAmount ?? 0m),
                    JulyR12Payments = g.Where(p => PeriodPaymentsPredicate(p, 12)).Sum(c => c.PaymentAmount ?? 0m),
                    R13Payments = g.Where(p => PeriodPaymentsPredicate(p, 13)).Sum(c => c.EarningAmount ?? 0m),
                    R14Payments = g.Where(p => PeriodPaymentsPredicate(p, 14)).Sum(c => c.PaymentAmount ?? 0m),

                    // Annual totals
                    TotalEarnings = g.Where(p => AnnualEarningsPredicate(p, 0)).Sum(c => c.EarningAmount ?? 0m),
                    TotalPaymentsYearToDate = g.Where(p => AnnualPaymentsPredicate(p, 0)).Sum(c => c.PaymentAmount ?? 0m),
                }).ToList();

            appsAdditionalPaymentsModels.AddRange(additionalPaymentGroups);

            return appsAdditionalPaymentsModels;
        }

        public List<AppsAdditionalPaymentExtendedPaymentModel> BuildAdditionalPaymentsExtendedPaymentsModel(
            AppsAdditionalPaymentILRInfo appsAdditionalPaymentIlrInfo,
            AppsAdditionalPaymentRulebaseInfo appsAdditionalPaymentRulebaseInfo,
            AppsAdditionalPaymentDasPaymentsInfo appsAdditionalPaymentDasPaymentsInfo)
        {
            List<AppsAdditionalPaymentExtendedPaymentModel> extendedPayments =
                new List<AppsAdditionalPaymentExtendedPaymentModel>();

            // Create a new payment model which includes the related data from the ILR
            foreach (var dasPaymentInfo in appsAdditionalPaymentDasPaymentsInfo.Payments)
            {
                // lookup the related reference data for this payment
                var learner = appsAdditionalPaymentIlrInfo?.Learners?
                    .SingleOrDefault(x =>
                        x.LearnRefNumber.CaseInsensitiveEquals(dasPaymentInfo?.LearnerReferenceNumber));

                var appsAdditionalPaymentLearningDeliveryInfo = learner?.LearningDeliveries?.SingleOrDefault(x =>
                        (x?.LearnRefNumber.CaseInsensitiveEquals(dasPaymentInfo?.LearnerReferenceNumber) ?? false) &&
                        x.LearnAimRef.CaseInsensitiveEquals(dasPaymentInfo?.LearningAimReference) &&
                        x?.LearnStartDate == dasPaymentInfo?.LearningStartDate &&
                        (x?.ProgType == null || x?.ProgType == dasPaymentInfo?.LearningAimProgrammeType) &&
                        (x?.StdCode == null || x?.StdCode == dasPaymentInfo?.LearningAimStandardCode) &&
                        (x?.FworkCode == null || x?.FworkCode == dasPaymentInfo?.LearningAimFrameworkCode) &&
                        (x?.PwayCode == null || x?.PwayCode == dasPaymentInfo?.LearningAimPathwayCode));

                var aecLearningDeliveryInfo = appsAdditionalPaymentRulebaseInfo?.AECLearningDeliveries?.SingleOrDefault(x =>
                        (x?.LearnRefNumber.CaseInsensitiveEquals(appsAdditionalPaymentLearningDeliveryInfo?.LearnRefNumber) ?? false) &&
                        x?.AimSeqNumber == appsAdditionalPaymentLearningDeliveryInfo?.AimSeqNumber);

                var aecApprenticeshipPriceEpisodePeriodisedValuesInfos = appsAdditionalPaymentRulebaseInfo
                        ?.AECApprenticeshipPriceEpisodePeriodisedValues.Where(x =>
                        (x?.LearnRefNumber.CaseInsensitiveEquals(appsAdditionalPaymentLearningDeliveryInfo?.LearnRefNumber) ?? false) &&
                        x?.AimSeqNumber == appsAdditionalPaymentLearningDeliveryInfo?.AimSeqNumber).ToList();

                // copy this payment's fields to the new extended payment model
                var extendedPayment = new AppsAdditionalPaymentExtendedPaymentModel();

                // copy the reporting grouping fields
                extendedPayment.PaymentLearnerReferenceNumber = dasPaymentInfo.LearnerReferenceNumber;
                extendedPayment.PaymentUniqueLearnerNumber = dasPaymentInfo.LearnerUln;
                extendedPayment.PaymentLearningStartDate = dasPaymentInfo.LearningStartDate;
                extendedPayment.PaymentLearningAimFundingLineType = dasPaymentInfo.LearningAimFundingLineType;
                extendedPayment.PaymentTypeOfAdditionalPayment = dasPaymentInfo.TypeOfAdditionalPayment;
                extendedPayment.AppsServiceEmployerName = dasPaymentInfo.EmployerName;
                extendedPayment.ilrEmployerIdentifier = GetEmployerIdentifier(appsAdditionalPaymentLearningDeliveryInfo, aecLearningDeliveryInfo, dasPaymentInfo);

                // copy the remaining payment fields
                extendedPayment.PaymentLearningAimReference = dasPaymentInfo.LearningAimReference;
                extendedPayment.PaymentTransactionType = dasPaymentInfo.TransactionType;
                extendedPayment.PaymentAcademicYear = dasPaymentInfo.AcademicYear;
                extendedPayment.PaymentCollectionPeriod = dasPaymentInfo.CollectionPeriod;
                extendedPayment.PaymentAmount = dasPaymentInfo.Amount;

                // copy the ilr fields
                extendedPayment.ProviderSpecifiedLearnerMonitoringA = GetProviderSpecMonitor(learner, Generics.ProviderSpecifiedLearnerMonitoringA);
                extendedPayment.ProviderSpecifiedLearnerMonitoringB = GetProviderSpecMonitor(learner, Generics.ProviderSpecifiedLearnerMonitoringB);
                extendedPayment.ilrEmployerIdentifier = GetEmployerIdentifier(appsAdditionalPaymentLearningDeliveryInfo, aecLearningDeliveryInfo, dasPaymentInfo);
                extendedPayment.EarningAmount = GetMonthlyEarnings(dasPaymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfos, dasPaymentInfo.CollectionPeriod);

                extendedPayments.Add(extendedPayment);
            }

            return extendedPayments;
        }

        private string GetProviderSpecMonitor(AppsAdditionalPaymentLearnerInfo learner, string providerSpecifiedLearnerMonitoring)
        {
            return learner?.ProviderSpecLearnerMonitorings
                       .SingleOrDefault(x => x.ProvSpecLearnMonOccur.CaseInsensitiveEquals(providerSpecifiedLearnerMonitoring))?.ProvSpecLearnMon ?? string.Empty;
        }

        private bool PeriodPaymentsPredicate(AppsAdditionalPaymentExtendedPaymentModel payment, int period)
        {
            bool result = payment.PaymentCollectionPeriod == period &&
                          AnnualPaymentsPredicate(payment, 0);

            return result;
        }

        private bool AnnualPaymentsPredicate(AppsAdditionalPaymentExtendedPaymentModel payment, int period)
        {
            bool result = payment.PaymentAcademicYear == 1920 &&
                          (payment.PaymentTypeOfAdditionalPayment.CaseInsensitiveEquals("Employer") ||
                           payment.PaymentTypeOfAdditionalPayment.CaseInsensitiveEquals("Provider") ||
                           payment.PaymentTypeOfAdditionalPayment.CaseInsensitiveEquals("Apprentice"));

            return result;
        }

        private bool PeriodEarningsPredicate(AppsAdditionalPaymentExtendedPaymentModel payment, int period)
        {
            bool result = payment.PaymentCollectionPeriod == period &&
                          AnnualEarningsPredicate(payment, period);

            return result;
        }

        private bool AnnualEarningsPredicate(AppsAdditionalPaymentExtendedPaymentModel payment, int period)
        {
            bool result = payment.PaymentAcademicYear == 1920;

            return result;
        }

        private decimal GetMonthlyEarnings(
            DASPaymentInfo dasPaymentInfo,
            List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aecApprenticeshipPriceEpisodePeriodisedValuesInfo,
            byte month)
        {
            decimal? result = 0;
            if (dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive || dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive)
            {
                result = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.Where(x => x.AttributeName.CaseInsensitiveEquals(Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName)).Sum(z => z.Periods[month]) ?? 0 +
                         aecApprenticeshipPriceEpisodePeriodisedValuesInfo.Where(x => x.AttributeName.CaseInsensitiveEquals(Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName)).Sum(z => z?.Periods[month]) ?? 0;
            }
            else if (dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive || dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive)
            {
                result = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.Where(x => x.AttributeName.CaseInsensitiveEquals(Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName)).Sum(z => z?.Periods[month]) ?? 0 +
                         aecApprenticeshipPriceEpisodePeriodisedValuesInfo.Where(x => x.AttributeName.CaseInsensitiveEquals(Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName)).Sum(z => z?.Periods[month]) ?? 0;
            }
            else if (dasPaymentInfo.TransactionType == Constants.DASPayments.TransactionType.Apprenticeship)
            {
                result = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.Where(x => x.AttributeName.CaseInsensitiveEquals(Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName))?.Sum(z => z.Periods[month]) ?? 0;
            }

            return result.GetValueOrDefault();
        }

        private string GetEmployerIdentifier(AppsAdditionalPaymentLearningDeliveryInfo appsAdditionalPaymentLearningDeliveryInfo, AECLearningDeliveryInfo aecLearningDeliveryInfo, DASPaymentInfo payment)
        {
            var identifier = 0;

            if (aecLearningDeliveryInfo != null)
            {
                if (payment.TransactionType == Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive) // 4
                {
                    identifier = aecLearningDeliveryInfo.LearnDelEmpIdFirstAdditionalPaymentThreshold.GetValueOrDefault();
                }
                else if (payment.TransactionType == Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive) //6
                {
                    identifier = aecLearningDeliveryInfo.LearnDelEmpIdSecondAdditionalPaymentThreshold.GetValueOrDefault();
                }
            }

            return identifier == 0 ? Generics.NotAvailable : identifier.ToString();
        }

        //foreach (var paymentInfo in appsAdditionalPaymentDasPaymentsInfo.Payments)
        //    {
        //        var model = new AppsAdditionalPaymentsModel()
        //        {
        //            LearnerReferenceNumber = paymentInfo.LearnerReferenceNumber,
        //            UniqueLearnerNumber = paymentInfo.LearnerUln,
        //            ProviderSpecifiedLearnerMonitoringA = GetProviderSpecMonitor(appsAdditionalPaymentIlrInfo, Generics.ProviderSpecifiedLearnerMonitoringA),
        //            ProviderSpecifiedLearnerMonitoringB = GetProviderSpecMonitor(appsAdditionalPaymentIlrInfo, Generics.ProviderSpecifiedLearnerMonitoringB),
        //            LearningStartDate = paymentInfo.LearningStartDate,
        //            FundingLineType = paymentInfo.LearningAimFundingLineType,
        //            EmployerNameFromApprenticeshipService = paymentInfo.EmployerName,
        //            TypeOfAdditionalPayment = paymentInfo.TypeOfAdditionalPayment,
        //            AugustR01Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.AugustR01),
        //            SeptemberR02Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.SeptemberR02),
        //            OctoberR03Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.OctoberR03),
        //            NovemberR04Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.NovemberR04),
        //            DecemberR05Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.DecemberR05),
        //            JanuaryR06Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.JanuaryR06),
        //            FebruaryR07Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.FebruaryR07),
        //            MarchR08Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.MarchR08),
        //            AprilR09Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.AprilR09),
        //            MayR10Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.MayR10),
        //            JuneR11Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.JuneR11),
        //            JulyR12Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.JulyR12),
        //            R13Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.R13),
        //            R14Payments = GetMonthlyPayments(paymentInfo, CollectionPeriods1920.R14)
        //        };

        //        foreach (var learner in appsAdditionalPaymentIlrInfo.Learners)
        //        {
        //            var appsAdditionalPaymentLearningDeliveryInfo = learner.LearningDeliveries.SingleOrDefault(x => x.UKPRN == paymentInfo.UkPrn &&
        //                                                                                                            x.LearnRefNumber.CaseInsensitiveEquals(paymentInfo.LearnerReferenceNumber) &&
        //                                                                                                            x.LearnAimRef.CaseInsensitiveEquals(paymentInfo.LearningAimReference) &&
        //                                                                                                            x.LearnStartDate == paymentInfo.LearningStartDate &&
        //                                                                                                            x.ProgType == paymentInfo.LearningAimProgrammeType &&
        //                                                                                                            x.StdCode == paymentInfo.LearningAimStandardCode &&
        //                                                                                                            x.FworkCode == paymentInfo.LearningAimFrameworkCode &&
        //                                                                                                            x.PwayCode == paymentInfo.LearningAimPathwayCode);
        //            var aecLearningDeliveryInfo = appsAdditionalPaymentLearningDeliveryInfo == null ? null
        //                : appsAdditionalPaymentRulebaseInfo.AECLearningDeliveries.SingleOrDefault(x =>
        //                x.UKPRN == appsAdditionalPaymentLearningDeliveryInfo.UKPRN &&
        //                x.LearnRefNumber.Equals(appsAdditionalPaymentLearningDeliveryInfo.LearnRefNumber, StringComparison.OrdinalIgnoreCase) &&
        //                x.AimSeqNumber == appsAdditionalPaymentLearningDeliveryInfo.AimSeqNumber);

        //            var aecApprenticeshipPriceEpisodePeriodisedValuesInfo = appsAdditionalPaymentLearningDeliveryInfo == null ? null
        //                : appsAdditionalPaymentRulebaseInfo.AECApprenticeshipPriceEpisodePeriodisedValues.Where(x =>
        //                x.UKPRN == appsAdditionalPaymentLearningDeliveryInfo.UKPRN &&
        //                x.LearnRefNumber.Equals(appsAdditionalPaymentLearningDeliveryInfo.LearnRefNumber, StringComparison.OrdinalIgnoreCase) &&
        //                x.AimSeqNumber == appsAdditionalPaymentLearningDeliveryInfo.AimSeqNumber).ToList();

        //            model.EmployerIdentifierFromILR = GetEmployerIdentifier(aecLearningDeliveryInfo, paymentInfo);
        //            model.AugustEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.August);
        //            model.SeptemberEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.September);
        //            model.OctoberEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.October);
        //            model.NovemberEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.November);
        //            model.DecemberEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.December);
        //            model.JanuaryEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.January);
        //            model.FebruaryEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.February);
        //            model.MarchEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.March);
        //            model.AprilEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.April);
        //            model.MayEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.May);
        //            model.JuneEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.June);
        //            model.JulyEarnings = aecApprenticeshipPriceEpisodePeriodisedValuesInfo == null ? 0 : GetMonthlyEarnings(paymentInfo, aecApprenticeshipPriceEpisodePeriodisedValuesInfo, PeriodMonths.July);
        //        }

        //        model.TotalEarnings = BuildTotalEarnings(model);
        //        model.TotalPaymentsYearToDate = BuildTotalPayments(model);
        //        appsAdditionalPaymentsModels.Add(model);
        //    }

        //    return BuildAppsAdditionalPaymentsResultModel(appsAdditionalPaymentsModels);
        //}

        //private List<AppsAdditionalPaymentsModel> BuildAppsAdditionalPaymentsResultModel(
        //    List<AppsAdditionalPaymentsModel> appsAdditionalPaymentsModels)
        //{
        //    return appsAdditionalPaymentsModels.GroupBy(
        //        x => new
        //    {
        //            LearnerReferenceNumber = (x.LearnerReferenceNumber ?? string.Empty).ToLowerInvariant(),
        //            x.UniqueLearnerNumber,
        //            x.LearningStartDate,
        //            FundingLineType = (x.FundingLineType ?? string.Empty).ToLowerInvariant(),
        //            TypeOfAdditionalPayment = (x.TypeOfAdditionalPayment ?? string.Empty).ToLowerInvariant(),
        //            EmployerNameFromApprenticeshipService = (x.EmployerNameFromApprenticeshipService ?? string.Empty).ToLowerInvariant(),
        //            EmployerIdentifierFromILR = (x.EmployerIdentifierFromILR ?? string.Empty).ToLowerInvariant()
        //    })
        //    .Select(x => new AppsAdditionalPaymentsModel()
        //    {
        //        LearnerReferenceNumber = x.Key.LearnerReferenceNumber,
        //        UniqueLearnerNumber = x.Key.UniqueLearnerNumber,
        //        LearningStartDate = x.Key.LearningStartDate,
        //        FundingLineType = x.Key.FundingLineType,
        //        TypeOfAdditionalPayment = x.Key.TypeOfAdditionalPayment,
        //        EmployerNameFromApprenticeshipService = x.Key.EmployerNameFromApprenticeshipService,
        //        EmployerIdentifierFromILR = x.Key.EmployerIdentifierFromILR,
        //        ProviderSpecifiedLearnerMonitoringA = x.First().ProviderSpecifiedLearnerMonitoringA,
        //        ProviderSpecifiedLearnerMonitoringB = x.First().ProviderSpecifiedLearnerMonitoringB,
        //        AugustEarnings = x.Sum(e => e.AugustEarnings),
        //        SeptemberEarnings = x.Sum(e => e.SeptemberEarnings),
        //        OctoberEarnings = x.Sum(e => e.OctoberEarnings),
        //        NovemberEarnings = x.Sum(e => e.NovemberEarnings),
        //        DecemberEarnings = x.Sum(e => e.DecemberEarnings),
        //        JanuaryEarnings = x.Sum(e => e.JanuaryEarnings),
        //        FebruaryEarnings = x.Sum(e => e.FebruaryEarnings),
        //        MarchEarnings = x.Sum(e => e.MarchEarnings),
        //        AprilEarnings = x.Sum(e => e.AprilEarnings),
        //        MayEarnings = x.Sum(e => e.MayEarnings),
        //        JuneEarnings = x.Sum(e => e.JuneEarnings),
        //        JulyEarnings = x.Sum(e => e.JulyEarnings),
        //        AugustR01Payments = x.Sum(p => p.AugustR01Payments),
        //        SeptemberR02Payments = x.Sum(p => p.SeptemberR02Payments),
        //        OctoberR03Payments = x.Sum(p => p.OctoberR03Payments),
        //        NovemberR04Payments = x.Sum(p => p.NovemberR04Payments),
        //        DecemberR05Payments = x.Sum(p => p.DecemberR05Payments),
        //        JanuaryR06Payments = x.Sum(p => p.JanuaryR06Payments),
        //        FebruaryR07Payments = x.Sum(p => p.FebruaryR07Payments),
        //        MarchR08Payments = x.Sum(p => p.MarchR08Payments),
        //        AprilR09Payments = x.Sum(p => p.AprilR09Payments),
        //        MayR10Payments = x.Sum(p => p.MayR10Payments),
        //        JuneR11Payments = x.Sum(p => p.JuneR11Payments),
        //        JulyR12Payments = x.Sum(p => p.JulyR12Payments),
        //        R13Payments = x.Sum(p => p.R13Payments),
        //        R14Payments = x.Sum(p => p.R14Payments),
        //        TotalEarnings = x.Sum(p => p.TotalEarnings),
        //        TotalPaymentsYearToDate = x.Sum(p => p.TotalPaymentsYearToDate)
        //    })
        //    .OrderBy(a => a.LearnerReferenceNumber)
        //    .ToList();
        //}

        //private decimal BuildAnnualTotalEarnings(AppsAdditionalPaymentsModel model)
        //{
        //    return model.AugustEarnings +
        //        model.SeptemberEarnings +
        //        model.OctoberEarnings +
        //        model.NovemberEarnings +
        //        model.DecemberEarnings +
        //        model.JanuaryEarnings +
        //        model.FebruaryEarnings +
        //        model.MarchEarnings +
        //        model.AprilEarnings +
        //        model.MayEarnings +
        //        model.JuneEarnings +
        //        model.JulyEarnings;
        //}

        //private decimal BuildTotalPayments(AppsAdditionalPaymentsModel model)
        //{
        //    return model.AugustR01Payments +
        //        model.SeptemberR02Payments +
        //        model.OctoberR03Payments +
        //        model.NovemberR04Payments +
        //        model.DecemberR05Payments +
        //        model.JanuaryR06Payments +
        //        model.FebruaryR07Payments +
        //        model.MarchR08Payments +
        //        model.AprilR09Payments +
        //        model.MayR10Payments +
        //        model.JuneR11Payments +
        //        model.JulyR12Payments +
        //        model.R13Payments +
        //        model.R14Payments;
        //}

        //private decimal GetMonthlyPayments(DASPaymentInfo paymentInfo, string collectionPeriodName)
        //{
        //    return paymentInfo.CollectionPeriod.ToCollectionPeriodName(paymentInfo.AcademicYear.ToString()).Equals(collectionPeriodName, StringComparison.OrdinalIgnoreCase) ? paymentInfo.Amount : 0;
        //}

        //private decimal GetMonthlyEarnings(
        //    AppsAdditionalPaymentExtendedPaymentModel payment,
        //    AECApprenticeshipPriceEpisodePeriodisedValuesInfo aecApprenticeshipPriceEpisodePeriodisedValuesInfo,
        //    byte period)
        //{
        //    decimal earning = 0;
        //    //if (payment.PaymentTransactionType == 4 || payment.PaymentTransactionType == 6)
        //    //{
        //    //}
        //    //else if (payment.PaymentTransactionType == 5 || payment.PaymentTransactionType == 7)
        //    //{
        //    //    earning = aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Periods[month] ?? 0 +
        //    //    //                 aecApprenticeshipPriceEpisodePeriodisedValuesInfo.SingleOrDefault(x => x.AttributeName.Equals(Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName, StringComparison.OrdinalIgnoreCase))?.Periods[month] ?? 0;
        //    //    //    }
        //    //}
        //    //else if ((payment.PaymentTransactionType == 16)
        //    //{

        //    //}

        //    return earning;
        //}

        ////public string GetAecPeriodisedValue(string attributeName, byte period)
        //{
        //    switch (period)
        //    {
        //        case 1:

        //            break;
        //    }
        //}
    }
}
