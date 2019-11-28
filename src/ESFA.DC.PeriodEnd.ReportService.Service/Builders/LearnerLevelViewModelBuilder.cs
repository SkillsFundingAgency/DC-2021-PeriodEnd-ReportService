﻿using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment.Comparer;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.Common;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.LearnerLevelView;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class LearnerLevelViewModelBuilder : ILearnerLevelViewModelBuilder
    {
        private readonly HashSet<byte?> _fundingSourceLevyPayments = new HashSet<byte?>() { 1, 5 };
        private readonly HashSet<byte?> _fundingSourceCoInvestmentPayments = new HashSet<byte?>() { 2 };
        private readonly HashSet<byte?> _fundingSourceCoInvestmentDueFromEmployer = new HashSet<byte?>() { 3 };

        private readonly HashSet<byte?> _transactionTypesLevyPayments = new HashSet<byte?>() { 1, 2, 3 };
        private readonly HashSet<byte?> _transactionTypesCoInvestmentPayments = new HashSet<byte?>() { 1, 2, 3 };
        private readonly HashSet<byte?> _transactionTypesCoInvestmentDueFromEmployer = new HashSet<byte?>() { 1, 2, 3 };
        private readonly HashSet<byte?> _transactionTypesEmployerAdditionalPayments = new HashSet<byte?>() { 4, 6 };
        private readonly HashSet<byte?> _transactionTypesProviderAdditionalPayments = new HashSet<byte?>() { 5, 7 };
        private readonly HashSet<byte?> _transactionTypesApprenticeshipAdditionalPayments = new HashSet<byte?>() { 16 };
        private readonly HashSet<byte?> _transactionTypesEnglishAndMathsPayments = new HashSet<byte?>() { 13, 14 };
        private readonly HashSet<byte?> _transactionTypesLearningSupportPayments = new HashSet<byte?>() { 8, 9, 10, 11, 12, 15 };

        private readonly ILogger _logger;
        private AppsMonthlyPaymentILRInfo _appsMonthlyPaymentIlrInfo;
        private AppsMonthlyPaymentDASInfo _appsMonthlyPaymentDasInfo;
        private AppsMonthlyPaymentDasEarningsInfo _appsMonthlyPaymentDasEarningsInfo;
        private LearnerLevelViewFM36Info _learnerLevelViewFM36Info;
        private LearnerLevelViewDASDataLockInfo _learnerLevelViewDASDataLockInfo;
        private int _appsReturnPeriod;

        private IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> _appsMonthlyPaymentLarsLearningDeliveryInfoList;

        public LearnerLevelViewModelBuilder(ILogger logger)
        {
            _logger = logger;
        }

        public IReadOnlyList<LearnerLevelViewModel> BuildLearnerLevelViewModelList(
            int Ukprn,
            AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo,
            AppsMonthlyPaymentDASInfo appsMonthlyPaymentDasInfo,
            AppsMonthlyPaymentDasEarningsInfo appsMonthlyPaymentDasEarningsInfo,
            AppsCoInvestmentILRInfo appsCoInvestmentIlrInfo,
            IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> appsMonthlyPaymentLarsLearningDeliveryInfoList,
            LearnerLevelViewFM36Info learnerLevelViewFM36Info,
            LearnerLevelViewDASDataLockInfo learnerLevelViewDASDataLockInfo,
            int returnPeriod)
        {
            // cache the passed in data for use in the private 'Get' methods
            _appsMonthlyPaymentIlrInfo = appsMonthlyPaymentIlrInfo;
            _appsMonthlyPaymentDasInfo = appsMonthlyPaymentDasInfo;
            _appsMonthlyPaymentDasEarningsInfo = appsMonthlyPaymentDasEarningsInfo;
            _appsMonthlyPaymentLarsLearningDeliveryInfoList = appsMonthlyPaymentLarsLearningDeliveryInfoList;
            _learnerLevelViewFM36Info = learnerLevelViewFM36Info;
            _learnerLevelViewDASDataLockInfo = learnerLevelViewDASDataLockInfo;
            _appsReturnPeriod = returnPeriod;

            // this variable is the final report and is the return value of this method.
            List<LearnerLevelViewModel> learnerLevelViewModelList = null;

            try
            {
                // Create keys for the records which need to come from payments and earnings
                var paymentsDictionary = BuildPaymentInfoDictionary(appsMonthlyPaymentDasInfo);
                var aECPriceEpisodeDictionary = BuildAECPriceEpisodeDictionary(_learnerLevelViewFM36Info?.AECApprenticeshipPriceEpisodePeriodisedValues);
                var aECLearningDeliveryDictionary = BuildAECLearningDeliveryDictionary(_learnerLevelViewFM36Info?.AECLearningDeliveryPeriodisedValuesInfo);
                var unionedKeys = UnionKeys(paymentsDictionary.Keys, aECPriceEpisodeDictionary.Keys, aECLearningDeliveryDictionary.Keys);

                // Populate the learner list using the ILR query first
                learnerLevelViewModelList = unionedKeys
                .OrderBy(o => o.LearnerReferenceNumber)
                    .ThenBy(o => o.PaymentFundingLineType)
                .Select(record =>
                {
                    var reportRecord = new LearnerLevelViewModel()
                    {
                        Ukprn = Ukprn,
                        PaymentLearnerReferenceNumber = record.LearnerReferenceNumber,
                        PaymentFundingLineType = record.PaymentFundingLineType
                    };

                    List<AppsMonthlyPaymentDasPaymentModel> paymentValues;
                    if (paymentsDictionary.TryGetValue(new LearnerLevelViewPaymentsKey(reportRecord.PaymentLearnerReferenceNumber, reportRecord.PaymentFundingLineType), out paymentValues))
                    {
                        // Assign the amounts
                        reportRecord.PlannedPaymentsToYouToDate = paymentValues.Where(p => PeriodLevyPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                            paymentValues.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                            paymentValues.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                            paymentValues.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                            paymentValues.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                            paymentValues.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                            paymentValues.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                            paymentValues.Where(p => PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                        reportRecord.ESFAPlannedPaymentsThisPeriod = paymentValues.Where(p => PeriodLevyPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                paymentValues.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                paymentValues.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                paymentValues.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                paymentValues.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                paymentValues.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                paymentValues.Where(p => PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                        reportRecord.CoInvestmentPaymentsToCollectThisPeriod = paymentValues.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                        // NOTE: Additional earigns calc required for this field
                        reportRecord.CoInvestmentOutstandingFromEmplToDate = paymentValues.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);
                    }

                    // Extract ILR info
                    var ilrRecord = appsMonthlyPaymentIlrInfo.Learners.FirstOrDefault(p => p.Ukprn == Ukprn && p.LearnRefNumber == reportRecord.PaymentLearnerReferenceNumber);
                    if (ilrRecord != null)
                    {
                        reportRecord.FamilyName = ilrRecord.FamilyName;
                        reportRecord.GivenNames = ilrRecord.GivenNames;
                        reportRecord.PaymentUniqueLearnerNumber = ilrRecord.UniqueLearnerNumber;
                        if ((ilrRecord.LearnerEmploymentStatus != null) && (ilrRecord.LearnerEmploymentStatus.Count > 0))
                        {
                            reportRecord.LearnerEmploymentStatusEmployerId = ilrRecord.LearnerEmploymentStatus.Where(les => les?.Ukprn == Ukprn &&
                                                                                                                        les.LearnRefNumber.CaseInsensitiveEquals(reportRecord.PaymentLearnerReferenceNumber) &&
                                                                                                                        les?.EmpStat == 10)
                                                                                                                .OrderByDescending(les => les?.DateEmpStatApp)
                                                                                                                .FirstOrDefault().EmpdId;
                        }
                    }

                    if (appsCoInvestmentIlrInfo != null)
                    {
                        var ilrInfo = appsCoInvestmentIlrInfo.Learners?
                                        .FirstOrDefault(l => l.LearnRefNumber.CaseInsensitiveEquals(reportRecord.PaymentLearnerReferenceNumber));

                        if (ilrInfo != null)
                        {
                            var learningDeliveries = ilrInfo.LearningDeliveries.Where(p => p.LearnRefNumber == reportRecord.PaymentLearnerReferenceNumber && p.UKPRN == Ukprn);
                            if ((learningDeliveries != null) && (learningDeliveries.Count() > 0))
                            {
                                foreach (var learningDelivery in learningDeliveries)
                                {
                                    IReadOnlyCollection<AppFinRecordInfo> currentYearData = learningDelivery.AppFinRecords.Where(x => x.AFinDate >= Generics.BeginningOfYear && x.AFinDate <= Generics.EndOfYear && string.Equals(x.AFinType, "PMR", StringComparison.OrdinalIgnoreCase)).ToList();
                                    reportRecord.TotalCoInvestmentCollectedToDate =
                                        currentYearData.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                                        currentYearData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);
                                }
                            }
                        }
                    }

                    // Work out total earnings to date
                    reportRecord.TotalEarningsToDate =
                        CalculatePriceEpisodeEarningsToPeriod(aECLearningDeliveryDictionary, aECPriceEpisodeDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeCompletionPaymentAttributeName) +
                        CalculatePriceEpisodeEarningsToPeriod(aECLearningDeliveryDictionary, aECPriceEpisodeDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeOnProgPaymentAttributeName) +
                        CalculatePriceEpisodeEarningsToPeriod(aECLearningDeliveryDictionary, aECPriceEpisodeDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm3PriceEpisodeBalancePaymentAttributeName) +
                        CalculatePriceEpisodeEarningsToPeriod(aECLearningDeliveryDictionary, aECPriceEpisodeDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeLSFCashAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36MathEngOnProgPaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36LearnSuppFundCashAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36MathEngBalPayment) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, true, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName);

                    // Work out earnings for this period
                    reportRecord.TotalEarningsForPeriod =
                        CalculatePriceEpisodeEarningsToPeriod(aECLearningDeliveryDictionary, aECPriceEpisodeDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeCompletionPaymentAttributeName) +
                        CalculatePriceEpisodeEarningsToPeriod(aECLearningDeliveryDictionary, aECPriceEpisodeDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeOnProgPaymentAttributeName) +
                        CalculatePriceEpisodeEarningsToPeriod(aECLearningDeliveryDictionary, aECPriceEpisodeDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm3PriceEpisodeBalancePaymentAttributeName) +
                        CalculatePriceEpisodeEarningsToPeriod(aECLearningDeliveryDictionary, aECPriceEpisodeDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeLSFCashAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36MathEngOnProgPaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36LearnSuppFundCashAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36MathEngBalPayment) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName) +
                        CalculateLearningDeliveryEarningsToPeriod(aECLearningDeliveryDictionary, false, _appsReturnPeriod, reportRecord, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName);

                    // Work out calculated fields
                    // Issues amount - how much the gap is between what the provider earnt and the payments the ESFA/Employer were planning to give them
                    reportRecord.IssuesAmount = (reportRecord.TotalEarningsForPeriod
                                                - reportRecord.ESFAPlannedPaymentsThisPeriod
                                                - reportRecord.CoInvestmentPaymentsToCollectThisPeriod) * -1;

                    // Work out what is remaining from employer by subtracting what they a have paid so far from their calculated payments.
                    reportRecord.CoInvestmentOutstandingFromEmplToDate = reportRecord.CoInvestmentOutstandingFromEmplToDate - reportRecord.TotalCoInvestmentCollectedToDate;

                    // Issues for non-payment - worked out in order of priority.
                    // Work out issues (Other) (NOTE: Do this first as lowest priority)
                    if (reportRecord.TotalEarningsForPeriod > reportRecord.CoInvestmentPaymentsToCollectThisPeriod +
                                                              reportRecord.ESFAPlannedPaymentsThisPeriod)
                    {
                        reportRecord.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_Other;
                    }

                    // TODO: Work out issues (HBCP)

                    // Work out reason for issues (Clawback)
                    if (reportRecord.ESFAPlannedPaymentsThisPeriod < 0)
                    {
                        reportRecord.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_Clawback;
                    }

                    if ((reportRecord.TotalEarningsForPeriod > reportRecord.ESFAPlannedPaymentsThisPeriod +
                                                               reportRecord.CoInvestmentPaymentsToCollectThisPeriod)
                        && (reportRecord.TotalEarningsToDate == reportRecord.PlannedPaymentsToYouToDate +
                                                                reportRecord.TotalCoInvestmentCollectedToDate +
                                                                reportRecord.CoInvestmentOutstandingFromEmplToDate))
                    {
                        reportRecord.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_Clawback;
                    }

                    // If the reason for issue is datalock then we need to set the rule description
                    if ((_learnerLevelViewDASDataLockInfo != null) && (_learnerLevelViewDASDataLockInfo.DASDataLocks != null))
                    {
                        var datalock = _learnerLevelViewDASDataLockInfo.DASDataLocks
                                                .FirstOrDefault(x => x.UkPrn == reportRecord.Ukprn &&
                                                        x.LearnerReferenceNumber == reportRecord.PaymentLearnerReferenceNumber &&
                                                        x.CollectionPeriod == _appsReturnPeriod);

                        // Check to see if any records returned
                        if (datalock != null)
                        {
                            // Extract data lock info
                            int datalock_rule_id = datalock.DataLockFailureId;

                            // calculate the rule description
                            string datalockValue = Generics.DLockErrorRuleNamePrefix + datalock_rule_id.ToString("00");
                            reportRecord.ReasonForIssues = datalockValue;
                            reportRecord.RuleDescription = DataLockValidationMessages.Validations.FirstOrDefault(x => x.RuleId.CaseInsensitiveEquals(datalockValue))?.ErrorMessage;
                        }
                    }

                    return reportRecord;
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Build Learner Level View", ex);
                throw;
            }

            return learnerLevelViewModelList;
        }

        public IDictionary<LearnerLevelViewPaymentsKey, List<AppsMonthlyPaymentDasPaymentModel>> BuildPaymentInfoDictionary(AppsMonthlyPaymentDASInfo paymentsInfo)
        {
            return paymentsInfo
                .Payments
                .GroupBy(
                p => new LearnerLevelViewPaymentsKey(p.LearnerReferenceNumber, p.ReportingAimFundingLineType), new LLVPaymentRecordKeyEqualityComparer())
                .ToDictionary(k => k.Key, v => v.ToList());
        }

        public IDictionary<string, List<AppsMonthlyPaymentDasEarningEventModel>> BuildEarningsInfoDictionary(AppsMonthlyPaymentDasEarningsInfo earningsInfo)
        {
            return earningsInfo
                .Earnings
                .GroupBy(p => p.LearnerReferenceNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key, v => v.ToList());
        }

        public IDictionary<string, List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>> BuildAECPriceEpisodeDictionary(List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> aECPriceEpisodeInfo)
        {
            return aECPriceEpisodeInfo
                .GroupBy(p => p.LearnRefNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key, v => v.ToList());
        }

        public IDictionary<string, List<AECLearningDeliveryPeriodisedValuesInfo>> BuildAECLearningDeliveryDictionary(List<AECLearningDeliveryPeriodisedValuesInfo> aECLearningDeliveryInfo)
        {
            return aECLearningDeliveryInfo
                .GroupBy(p => p.LearnRefNumber, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(k => k.Key, v => v.ToList());
        }

        public IEnumerable<LearnerLevelViewPaymentsKey> UnionKeys(IEnumerable<LearnerLevelViewPaymentsKey> paymentRecords, IEnumerable<string> priceEpisodeRecords, IEnumerable<string> learningDeliveryRecords)
        {
            var filteredPaymentRecordsHashSet = new HashSet<LearnerLevelViewPaymentsKey>(paymentRecords.Select(r => new LearnerLevelViewPaymentsKey(r.LearnerReferenceNumber, r.PaymentFundingLineType)), new LLVPaymentRecordLRefOnlyKeyEqualityComparer());
            var filteredPriceEpisodeRecordHashset = new HashSet<LearnerLevelViewPaymentsKey>(priceEpisodeRecords.Select(r => new LearnerLevelViewPaymentsKey(r.ToString(), string.Empty)), new LLVPaymentRecordLRefOnlyKeyEqualityComparer());
            var filteredLearningDeliveryRecordHashset = new HashSet<LearnerLevelViewPaymentsKey>(learningDeliveryRecords.Select(r => new LearnerLevelViewPaymentsKey(r.ToString(), string.Empty)), new LLVPaymentRecordLRefOnlyKeyEqualityComparer());

            filteredPaymentRecordsHashSet.UnionWith(filteredPriceEpisodeRecordHashset);
            filteredPaymentRecordsHashSet.UnionWith(filteredLearningDeliveryRecordHashset);

            return filteredPaymentRecordsHashSet.ToList();
        }

        //------------------------------------------------------------------------------------------------------
        // Levy Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is a Levy payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Levy payment, otherwise false.</returns>
        private bool PeriodLevyPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalLevyPaymentsTypePredicate(payment);

            return result;
        }

        /// <summary>
        /// Returns true if the payment is a Levy payment for a range of periods.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="toPeriod">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Levy payment, otherwise false.</returns>
        private bool PeriodLevyPaymentsTypePredicateToPeriod(AppsMonthlyPaymentDasPaymentModel payment, int toPeriod)
        {
            bool result = payment.CollectionPeriod <= toPeriod &&
                          TotalLevyPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalLevyPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                   payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                   _fundingSourceLevyPayments.Contains(payment.FundingSource) &&
                   _transactionTypesLevyPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // CoInvestment Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is a CoInvestment payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a CoInvestment payment, otherwise false.</returns>
        private bool PeriodCoInvestmentPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalCoInvestmentPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalCoInvestmentPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                          payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _fundingSourceCoInvestmentPayments.Contains(payment.FundingSource) &&
                          _transactionTypesCoInvestmentPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // CoInvestment Payments Due From Employer Payment Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payments is a CoInvestment payment due from the Employer.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a CoInvestment due from the employer payment, otherwise false.</returns>
        private bool PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(payment);

            return result;
        }

        /// <summary>
        /// Returns true if the payments is a CoInvestment payment due from the Employer for a range of periods.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="toPeriod">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a CoInvestment due from the employer payment, otherwise false.</returns>
        private bool PeriodCoInvestmentDueFromEmployerPaymentsTypePredicateToPeriod(AppsMonthlyPaymentDasPaymentModel payment, int toPeriod)
        {
            bool result = payment.CollectionPeriod <= toPeriod &&
                          TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(payment);

            return result;
        }

        private bool TotalCoInvestmentPaymentsDueFromEmployerTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                          payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _fundingSourceCoInvestmentDueFromEmployer.Contains(payment.FundingSource) &&
                         _transactionTypesCoInvestmentDueFromEmployer.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // Employer Additional Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is an Employer additional payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if an Employer additional payment, otherwise false.</returns>
        private bool PeriodEmployerAdditionalPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalEmployerAdditionalPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalEmployerAdditionalPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                          payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _transactionTypesEmployerAdditionalPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // Provider Additional Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is Provider additional payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if a Provider additional payment, otherwise false.</returns>
        private bool PeriodProviderAdditionalPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalProviderAdditionalPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalProviderAdditionalPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                          payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _transactionTypesProviderAdditionalPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // Apprenticeship Additional Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is an Apprenticeship additional payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if an Apprenticeship additional payment, otherwise false.</returns>
        private bool PeriodApprenticeAdditionalPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalProviderApprenticeshipAdditionalPaymentsTypePredicate(payment);

            return result;
        }

        /// <summary>
        /// Returns true if the payment is an Apprenticeship additional payment for a range of periods.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="toPeriod">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if an Apprenticeship additional payment, otherwise false.</returns>
        private bool PeriodApprenticeAdditionalPaymentsTypePredicateToPeriod(AppsMonthlyPaymentDasPaymentModel payment, int toPeriod)
        {
            bool result = payment.CollectionPeriod <= toPeriod &&
                          TotalProviderApprenticeshipAdditionalPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalProviderApprenticeshipAdditionalPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                   payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                   _transactionTypesApprenticeshipAdditionalPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // English and Maths Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is an English and Maths payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if an English and Maths payment.</returns>
        private bool PeriodEnglishAndMathsPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                          TotalEnglishAndMathsPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalEnglishAndMathsPaymentsTypePredicate(AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                          !payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                          _transactionTypesEnglishAndMathsPayments.Contains(payment.TransactionType);

            return result;
        }

        //------------------------------------------------------------------------------------------------------
        // Learning Support, Disadvantage and Framework Uplift Payments Type Predicates
        //------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Returns true if the payment is a Learning Support, Disadvantage and Framework Uplift payment.
        /// </summary>
        /// <param name="payment">Instance of type AppsMonthlyPaymentReportModel.</param>
        /// <param name="period">Return period e.g. 1, 2, 3 etc.</param>
        /// <returns>true if aLearning Support, Disadvantage and Framework Uplift payments.</returns>
        private bool PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment, int period)
        {
            bool result = payment.CollectionPeriod == period &&
                   TotalLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(payment);

            return result;
        }

        private bool TotalLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(
            AppsMonthlyPaymentDasPaymentModel payment)
        {
            bool result = payment.AcademicYear == Generics.AcademicYear &&
                           ((payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) &&
                           _transactionTypesLearningSupportPayments.Contains(payment.TransactionType)) ||
                           (!payment.LearningAimReference.CaseInsensitiveEquals(Generics.ZPROG001) && payment.TransactionType == 15));

            return result;
        }

        private decimal? CalculatePriceEpisodeEarningsToPeriod(
                                IDictionary<string, List<AECLearningDeliveryPeriodisedValuesInfo>> ldpvdict,
                                IDictionary<string, List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo>> pepvdict,
                                bool yearToDate,
                                int period,
                                LearnerLevelViewModel learnerLevelViewModel,
                                string attributeType)
        {
            // NOTE: Don't return any data if attribute is PriceEpisodeLSFCash and LearnDelMathEng flag is set
            if (attributeType == Generics.Fm36PriceEpisodeLSFCashAttributeName)
            {
                List<AECLearningDeliveryPeriodisedValuesInfo> ldLearner;
                if (ldpvdict.TryGetValue(learnerLevelViewModel.PaymentLearnerReferenceNumber, out ldLearner))
                {
                    if (ldLearner.Where(p => p.LearnDelMathEng == true).Count() > 0)
                    {
                        return 0;
                    }
                }
            }

            // Filter the correct records based on learner info and type of payment/earning
            List<AECApprenticeshipPriceEpisodePeriodisedValuesInfo> learnerRecord;
            decimal? sum = 0;
            if (pepvdict.TryGetValue(learnerLevelViewModel.PaymentLearnerReferenceNumber, out learnerRecord))
            {
                foreach (var lr in learnerRecord.Where( p => p.AttributeName == attributeType && p.Periods != null))
                {
                    // NOTE: "i" is an index value created in the linq query
                    if (yearToDate)
                    {
                        sum = sum + lr.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i <= period).Sum(o => o.pv);
                    }
                    else
                    {
                        sum = sum + lr.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i == period).Sum(o => o.pv);
                    }
                }
            }

            return sum;
        }

        private decimal? CalculateLearningDeliveryEarningsToPeriod(
                                IDictionary<string, List<AECLearningDeliveryPeriodisedValuesInfo>> pvdict,
                                bool yearToDate,
                                int period,
                                LearnerLevelViewModel learnerLevelViewModel,
                                string attributeType)
        {
            // Filter the correct records based on learner info and type of payment/earning
            List<AECLearningDeliveryPeriodisedValuesInfo> ldLearner = null;
            if (attributeType == Generics.Fm36LearnSuppFundCashAttributeName)
            {
                if (pvdict.TryGetValue(learnerLevelViewModel.PaymentLearnerReferenceNumber, out ldLearner))
                {
                    if (ldLearner.Where(p => p.LearnDelMathEng == true).Count() == 0)
                    {
                        return 0;
                    }
                }
            }

            // Now extract the total value - loop though learners and pull out period data less than or equal to current period
            decimal? sum = 0;
            if ((ldLearner != null) || pvdict.TryGetValue(learnerLevelViewModel.PaymentLearnerReferenceNumber, out ldLearner))
            {
                foreach (var lr in ldLearner.Where(p => p.AttributeName == attributeType && p.Periods != null))
                {
                    // NOTE: "i" is an index value created in the linq query
                    if (yearToDate)
                    {
                        sum = sum + lr.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i <= period).Sum(o => o.pv);
                    }
                    else
                    {
                        sum = sum + lr.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i == period).Sum(o => o.pv);
                    }
                }
            }

            return sum;
        }
    }
}
