using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsCoInvestment;
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
                // Populate the learner list using the ILR query first
                learnerLevelViewModelList = appsMonthlyPaymentIlrInfo.Learners
                .GroupBy(r => new
                {
                    r.Ukprn,
                    r.LearnRefNumber,
                    r.UniqueLearnerNumber
                })
                .OrderBy(o => o.Key.Ukprn)
                    .ThenBy(o => o.Key.LearnRefNumber)
                    .ThenBy(o => o.Key.UniqueLearnerNumber)
                .Select(g => new LearnerLevelViewModel
                {
                    Ukprn = g.Key.Ukprn,
                    PaymentLearnerReferenceNumber = g.Key.LearnRefNumber,
                    PaymentUniqueLearnerNumber = g.Key.UniqueLearnerNumber,
                    GivenNames = g.First().GivenNames,
                    FamilyName = g.First().FamilyName,
                    LearnerEmploymentStatusEmployerId = g.First().LearnerEmploymentStatus.Where(les => les?.Ukprn == g.Key.Ukprn &&
                                                            les.LearnRefNumber.CaseInsensitiveEquals(g.Key.LearnRefNumber) &&
                                                            les?.EmpStat == 10)
                                                        .OrderByDescending(les => les?.DateEmpStatApp)
                                                        .FirstOrDefault().EmpdId,
                    LearningAimReference = g.First().LearningDeliveries.First().LearnAimRef
                }).ToList();

                // Further population of the appsMonthlyPaymentModel payment related fields
                if (learnerLevelViewModelList != null)
                {
                    foreach (var learnerLevelViewModel in learnerLevelViewModelList)
                    {
                        // Add the payment information from the the appmonthly payments data source
                        if (appsMonthlyPaymentDasInfo != null && appsMonthlyPaymentDasInfo.Payments != null)
                        {
                            var learnerPayment = appsMonthlyPaymentDasInfo.Payments?
                                    .Where(p => p.AcademicYear == Generics.AcademicYear &&
                                            p.LearnerReferenceNumber == learnerLevelViewModel.PaymentLearnerReferenceNumber);

                            if ( learnerPayment != null && learnerPayment.Count() > 0)
                            {
                                // Assign the payment information for the learner - learning aim details are same in all records in the payment data set
                                learnerLevelViewModel.LearningAimReference = learnerPayment.FirstOrDefault().LearningAimReference;
                                learnerLevelViewModel.LearningStartDate = learnerPayment.FirstOrDefault().LearningStartDate;
                                learnerLevelViewModel.LearningAimProgrammeType = learnerPayment.FirstOrDefault().LearningAimProgrammeType;
                                learnerLevelViewModel.LearningAimStandardCode = learnerPayment.FirstOrDefault().LearningAimStandardCode;
                                learnerLevelViewModel.LearningAimFrameworkCode = learnerPayment.FirstOrDefault().LearningAimFrameworkCode;
                                learnerLevelViewModel.LearningAimPathwayCode = learnerPayment.FirstOrDefault().LearningAimPathwayCode;
                                learnerLevelViewModel.PaymentFundingLineType = learnerPayment.FirstOrDefault().ReportingAimFundingLineType;

                                // Assign the amounts
                                learnerLevelViewModel.PlannedPaymentsToYouToDate = learnerPayment.Where(p => PeriodLevyPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                                    learnerPayment.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                                    learnerPayment.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                                    learnerPayment.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                                    learnerPayment.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                                    learnerPayment.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                                    learnerPayment.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                                                    learnerPayment.Where(p => PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                                learnerLevelViewModel.ESFAPlannedPaymentsThisPeriod = learnerPayment.Where(p => PeriodLevyPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                        learnerPayment.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                        learnerPayment.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                        learnerPayment.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                        learnerPayment.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                        learnerPayment.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                                                        learnerPayment.Where(p => PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                                learnerLevelViewModel.CoInvestmentPaymentsToCollectThisPeriod = learnerPayment.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);

                                // NOTE: Additional earigns calc required for this field
                                learnerLevelViewModel.CoInvestmentOutstandingFromEmplToDate = learnerPayment.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m);
                            }
                        }

                // Populate learner level view list
                // learnerLevelViewModelList = appsMonthlyPaymentDasInfo.Payments?
                //    .Where(p => p.AcademicYear == Generics.AcademicYear)
                //    .GroupBy(r => new
                //    {
                //        r.Ukprn,
                //        r.LearnerReferenceNumber,
                //        r.LearnerUln,
                //        r.ReportingAimFundingLineType
                //    })
                //    .OrderBy(o => o.Key.Ukprn)
                //        .ThenBy(o => o.Key.LearnerReferenceNumber)
                //        .ThenBy(o => o.Key.LearnerUln)
                //        .ThenBy(o => o.Key.ReportingAimFundingLineType)
                //    .Select(g => new LearnerLevelViewModel
                //    {
                //        Ukprn = g.Key.Ukprn,
                //        PaymentLearnerReferenceNumber = g?.Key.LearnerReferenceNumber,
                //        PaymentUniqueLearnerNumber = g?.Key.LearnerUln,
                //        LearningAimReference = g.First().LearningAimReference,
                //        LearningStartDate = g.First().LearningStartDate,
                //        LearningAimProgrammeType = g.First().LearningAimProgrammeType,
                //        LearningAimStandardCode = g.First().LearningAimStandardCode,
                //        LearningAimFrameworkCode = g.First().LearningAimFrameworkCode,
                //        LearningAimPathwayCode = g.First().LearningAimPathwayCode,
                //        LearnerEmploymentStatusEmployerId = null, // Set in the "Further..." section below
                //        TotalEarningsToDate = 0, // Set in the "Further..." section below
                //        PlannedPaymentsToYouToDate = g.Where(p => PeriodLevyPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodCoInvestmentPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m),
                //        TotalCoInvestmentCollectedToDate = 0, // Set in the "Further..." section below
                //        CoInvestmentOutstandingFromEmplToDate = g.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicateToPeriod(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m), // Additional calc needed in the "further..." section below
                //        TotalEarningsForPeriod = 0, // Set in the "Further..." section below
                //        ESFAPlannedPaymentsThisPeriod = g.Where(p => PeriodLevyPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodApprenticeAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodEnglishAndMathsPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodEmployerAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodProviderAdditionalPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m) +
                //                g.Where(p => PeriodLearningSupportDisadvantageAndFrameworkUpliftPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m),
                //        CoInvestmentPaymentsToCollectThisPeriod = g.Where(p => PeriodCoInvestmentDueFromEmployerPaymentsTypePredicate(p, _appsReturnPeriod)).Sum(c => c.Amount ?? 0m),
                //        IssuesAmount = 0, // Set in the "Further..." section below
                //        ReasonForIssues = null, // TODO
                //        PaymentFundingLineType = g?.Key.ReportingAimFundingLineType
                //    }).ToList();

                        // Extract learner delivery details
                        AppsCoInvestmentRecordKey record = new AppsCoInvestmentRecordKey(
                            learnerLevelViewModel.PaymentLearnerReferenceNumber,
                            learnerLevelViewModel.LearningStartDate,
                            learnerLevelViewModel.LearningAimProgrammeType.GetValueOrDefault(),
                            learnerLevelViewModel.LearningAimStandardCode.GetValueOrDefault(),
                            learnerLevelViewModel.LearningAimFrameworkCode.GetValueOrDefault(),
                            learnerLevelViewModel.LearningAimPathwayCode.GetValueOrDefault());
                        var learner = GetLearnerForRecord(appsCoInvestmentIlrInfo, record);
                        if (learner != null)
                        {
                            var learningDelivery = GetLearningDeliveryForRecord(learner, record);

                            // Use LearnerDelivery details from the Emp Investment record to calculate employ contributions
                            if ((learningDelivery != null) && (learningDelivery.AppFinRecords != null))
                            {
                                IReadOnlyCollection<AppFinRecordInfo> currentYearData = learningDelivery.AppFinRecords
                                    .Where(x => x.AFinDate >= Generics.BeginningOfYear && x.AFinDate <= Generics.EndOfYear &&
                                                string.Equals(x.AFinType, "PMR", StringComparison.OrdinalIgnoreCase)).ToList();

                                learnerLevelViewModel.TotalCoInvestmentCollectedToDate =
                                    currentYearData.Where(x => x.AFinCode == 1 || x.AFinCode == 2).Sum(x => x.AFinAmount) -
                                    currentYearData.Where(x => x.AFinCode == 3).Sum(x => x.AFinAmount);
                            }
                        }

                        if (_appsMonthlyPaymentDasEarningsInfo?.Earnings != null)
                        {
                            // Extract learning aim seq number
                            learnerLevelViewModel.learningAimSeqNumbers = _appsMonthlyPaymentDasEarningsInfo?.Earnings?
                                .Where(dei => dei.Ukprn == learnerLevelViewModel.Ukprn &&
                                       dei.LearnerReferenceNumber == learnerLevelViewModel.PaymentLearnerReferenceNumber &&
                                       dei.LearnerUln == learnerLevelViewModel.PaymentUniqueLearnerNumber)
                                .Select(p => p.LearningAimSequenceNumber)
                                .ToArray();
                        }

                        if ((_learnerLevelViewFM36Info?.AECApprenticeshipPriceEpisodePeriodisedValues != null) &&
                             (_learnerLevelViewFM36Info?.AECLearningDeliveryPeriodisedValuesInfo != null))
                        {
                            // Work out total earnings to date
                            learnerLevelViewModel.TotalEarningsToDate =
                                CalculatePriceEpisodeEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeCompletionPaymentAttributeName) +
                                CalculatePriceEpisodeEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeOnProgPaymentAttributeName) +
                                CalculatePriceEpisodeEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm3PriceEpisodeBalancePaymentAttributeName) +
                                CalculatePriceEpisodeEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeLSFCashAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36MathEngOnProgPaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36LearnSuppFundCashAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36MathEngBalPayment) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(true, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName);

                            // Work out earnings for this period
                            learnerLevelViewModel.TotalEarningsForPeriod =
                                CalculatePriceEpisodeEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeCompletionPaymentAttributeName) +
                                CalculatePriceEpisodeEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeOnProgPaymentAttributeName) +
                                CalculatePriceEpisodeEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm3PriceEpisodeBalancePaymentAttributeName) +
                                CalculatePriceEpisodeEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeLSFCashAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36MathEngOnProgPaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36LearnSuppFundCashAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36MathEngBalPayment) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeFirstEmp1618PayAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeSecondEmp1618PayAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeFirstProv1618PayAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeSecondProv1618PayAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName) +
                                CalculateLearningDeliveryEarningsToPeriod(false, _appsReturnPeriod, learnerLevelViewModel, Generics.Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName);
                        }

                        // Work out calculated fields
                        // Issues amount - how much the gap is between what the provider earnt and the payments the ESFA/Employer were planning to give them
                        learnerLevelViewModel.IssuesAmount = (learnerLevelViewModel.TotalEarningsForPeriod
                                                            - learnerLevelViewModel.ESFAPlannedPaymentsThisPeriod
                                                            - learnerLevelViewModel.CoInvestmentPaymentsToCollectThisPeriod) * -1;

                        // Work out what is remaining from employer by subtracting what they a have paid so far from their calculated payments.
                        learnerLevelViewModel.CoInvestmentOutstandingFromEmplToDate = learnerLevelViewModel.CoInvestmentOutstandingFromEmplToDate - learnerLevelViewModel.TotalCoInvestmentCollectedToDate;

                        // Issues for non-payment - worked out in order of priority.
                        // Work out issues (Other) (NOTE: Do this first as lowest priority)
                        if (learnerLevelViewModel.TotalEarningsForPeriod > learnerLevelViewModel.CoInvestmentPaymentsToCollectThisPeriod +
                                                                           learnerLevelViewModel.ESFAPlannedPaymentsThisPeriod)
                        {
                            learnerLevelViewModel.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_Other;
                        }

                        // TODO: Work out issues (HBCP)

                        // Work out reason for issues (Clawback)
                        if (learnerLevelViewModel.ESFAPlannedPaymentsThisPeriod < 0)
                        {
                            learnerLevelViewModel.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_Clawback;
                        }

                        if ((learnerLevelViewModel.TotalEarningsForPeriod > learnerLevelViewModel.ESFAPlannedPaymentsThisPeriod +
                                                                            learnerLevelViewModel.CoInvestmentPaymentsToCollectThisPeriod)
                            && (learnerLevelViewModel.TotalEarningsToDate == learnerLevelViewModel.PlannedPaymentsToYouToDate +
                                                                             learnerLevelViewModel.TotalCoInvestmentCollectedToDate +
                                                                             learnerLevelViewModel.CoInvestmentOutstandingFromEmplToDate))
                        {
                            learnerLevelViewModel.ReasonForIssues = Reports.LearnerLevelViewReport.ReasonForIssues_Clawback;
                        }

                        // If the reason for issue is datalock then we need to set the rule description
                        if ((_learnerLevelViewDASDataLockInfo != null) && (_learnerLevelViewDASDataLockInfo.DASDataLocks != null))
                        {
                            var datalock = _learnerLevelViewDASDataLockInfo.DASDataLocks
                                                    .FirstOrDefault(x => x.UkPrn == learnerLevelViewModel.Ukprn &&
                                                            x.LearnerReferenceNumber == learnerLevelViewModel.PaymentLearnerReferenceNumber &&
                                                            x.LearnerUln == learnerLevelViewModel.PaymentUniqueLearnerNumber &&
                                                            x.LearningAimReference == learnerLevelViewModel.LearningAimReference &&
                                                            x.CollectionPeriod == _appsReturnPeriod);

                            // Check to see if any records returned
                            if (datalock != null)
                            {
                                // Extract data lock info
                                int datalock_rule_id = datalock.DataLockFailureId;

                                // calculate the rule description
                                string datalockValue = Generics.DLockErrorRuleNamePrefix + datalock_rule_id.ToString("00");
                                learnerLevelViewModel.ReasonForIssues = datalockValue;
                                learnerLevelViewModel.RuleDescription = DataLockValidationMessages.Validations.FirstOrDefault(x => x.RuleId.CaseInsensitiveEquals(datalockValue))?.ErrorMessage;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to Build Learner Level View", ex);
                throw;
            }

            return learnerLevelViewModelList;
        }

        public LearningDeliveryInfo GetLearningDeliveryForRecord(LearnerInfo learner, AppsCoInvestmentRecordKey record)
        {
            if (learner != null)
            {
                return learner?
                    .LearningDeliveries
                    .FirstOrDefault(ld => IlrLearningDeliveryRecordMatch(ld, record));
            }

            return null;
        }

        public LearnerInfo GetLearnerForRecord(AppsCoInvestmentILRInfo ilrInfo, AppsCoInvestmentRecordKey record)
        {
            if (ilrInfo != null)
            {
                return ilrInfo
                    .Learners?
                    .FirstOrDefault(l => l.LearnRefNumber.CaseInsensitiveEquals(record.LearnerReferenceNumber)
                        && (l.LearningDeliveries?.Any(ld => IlrLearningDeliveryRecordMatch(ld, record)) ?? false));
            }

            return null;
        }

        public bool IlrLearningDeliveryRecordMatch(LearningDeliveryInfo learningDelivery, AppsCoInvestmentRecordKey record)
        {
            return learningDelivery.ProgType == record.LearningAimProgrammeType
                    && learningDelivery.StdCode == record.LearningAimStandardCode
                    && learningDelivery.FworkCode == record.LearningAimFrameworkCode
                    && learningDelivery.PwayCode == record.LearningAimPathwayCode
                    && learningDelivery.LearnStartDate == record.LearningStartDate
                    && learningDelivery.LearnAimRef.CaseInsensitiveEquals(record.LearningAimReference);
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

        private decimal? CalculatePriceEpisodeEarningsToPeriod(bool yearToDate, int period, LearnerLevelViewModel learnerLevelViewModel, string attributeType)
        {
            // NOTE: Don't return any data if attribute is PriceEpisodeLSFCash and LearnDelMathEng flag is set
            if (attributeType == Generics.Fm36PriceEpisodeLSFCashAttributeName)
            {
                if (_learnerLevelViewFM36Info?
                        .AECLearningDeliveryPeriodisedValuesInfo?
                        .Where(p =>
                            p.UKPRN == learnerLevelViewModel.Ukprn
                            && p.LearnRefNumber == learnerLevelViewModel.PaymentLearnerReferenceNumber
                            && learnerLevelViewModel.learningAimSeqNumbers.Contains((byte?)p.AimSeqNumber)
                            && p.Periods != null
                            && p.LearnDelMathEng == true
                            && p.AttributeName == attributeType).Count() > 0)
                {
                    return 0;
                }
            }

            // Filter the correct records based on learner info and type of payment/earning
            var learnerRecords = _learnerLevelViewFM36Info?
                    .AECApprenticeshipPriceEpisodePeriodisedValues?
                    .Where(p =>
                        p.UKPRN == learnerLevelViewModel.Ukprn
                        && p.LearnRefNumber == learnerLevelViewModel.PaymentLearnerReferenceNumber
                        && learnerLevelViewModel.learningAimSeqNumbers.Contains((byte?)p.AimSeqNumber)
                        && p.Periods != null
                        && p.AttributeName == attributeType);

            // Now extract the total value - loop though learners and pull out period data less than or equal to current period
            decimal? sum = 0;
            foreach (var learnerRecord in learnerRecords)
            {
                // NOTE: "i" is an index value created in the linq query
                if (yearToDate)
                {
                    sum = sum + learnerRecord.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i <= period).Sum(o => o.pv);
                }
                else
                {
                    sum = sum + learnerRecord.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i == period).Sum(o => o.pv);
                }
            }

            return sum;
        }

        private decimal? CalculateLearningDeliveryEarningsToPeriod(bool yearToDate, int period, LearnerLevelViewModel learnerLevelViewModel, string attributeType)
        {
            // Filter the correct records based on learner info and type of payment/earning
            IEnumerable<AECLearningDeliveryPeriodisedValuesInfo> learnerRecords;
            if (attributeType == Generics.Fm36LearnSuppFundCash)
            {
                // Check attribute - only add LearnSuppFundCash if LearnDelMathEng is set
                learnerRecords = _learnerLevelViewFM36Info?
                        .AECLearningDeliveryPeriodisedValuesInfo?
                        .Where(p =>
                            p.UKPRN == learnerLevelViewModel.Ukprn
                            && p.LearnRefNumber == learnerLevelViewModel.PaymentLearnerReferenceNumber
                            && learnerLevelViewModel.learningAimSeqNumbers.Contains((byte?)p.AimSeqNumber)
                            && p.Periods != null
                            && p.LearnDelMathEng == true
                            && p.AttributeName == attributeType);
            }
            else
            {
                learnerRecords = _learnerLevelViewFM36Info?
                    .AECLearningDeliveryPeriodisedValuesInfo?
                    .Where(p =>
                        p.UKPRN == learnerLevelViewModel.Ukprn
                        && p.LearnRefNumber == learnerLevelViewModel.PaymentLearnerReferenceNumber
                        && learnerLevelViewModel.learningAimSeqNumbers.Contains((byte?)p.AimSeqNumber)
                        && p.Periods != null
                        && p.AttributeName == attributeType);
            }

            // Now extract the total value - loop though learners and pull out period data less than or equal to current period
            decimal? sum = 0;
            foreach (var learnerRecord in learnerRecords)
            {
                // NOTE: "i" is an index value created in the linq query
                if (yearToDate)
                {
                    sum = sum + learnerRecord.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i <= period).Sum(o => o.pv);
                }
                else
                {
                    sum = sum + learnerRecord.Periods.Select((pv, i) => new { i, pv }).Where(a => a.i == period).Sum(o => o.pv);
                }
            }

            return sum;
        }
    }
}
