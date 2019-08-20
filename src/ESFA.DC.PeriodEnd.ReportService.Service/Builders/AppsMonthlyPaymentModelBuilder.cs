using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Interface.Utils;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;
using ESFA.DC.ReferenceData.FCS.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class AppsMonthlyPaymentModelBuilder : IAppsMonthlyPaymentModelBuilder
    {
        private const string ZPROG001 = "ZPROG001";

        private readonly string[] _collectionPeriods =
        {
            "1920-R01",
            "1920-R02",
            "1920-R03",
            "1920-R04",
            "1920-R05",
            "1920-R06",
            "1920-R07",
            "1920-R08",
            "1920-R09",
            "1920-R10",
            "1920-R11",
            "1920-R12",
            "1920-R13",
            "1920-R14"
        };

        private readonly int[] _fundingSourceLevyPayments = { 1, 5 };
        private readonly int[] _fundingSourceCoInvestmentPayments = { 2 };
        private readonly int[] _fundingSourceCoInvestmentDueFromEmployer = { 3 };
        private readonly int[] _transactionTypesLevyPayments = { 1, 2, 3 };
        private readonly int[] _transactionTypesCoInvestmentPayments = { 1, 2, 3 };
        private readonly int[] _transactionTypesCoInvestmentFromEmployer = { 1, 2, 3 };
        private readonly int[] _transactionTypesEmployerAdditionalPayments = { 4, 6 };
        private readonly int[] _transactionTypesProviderAdditionalPayments = { 5, 7 };
        private readonly int[] _transactionTypesApprenticeshipAdditionalPayments = { 16 };
        private readonly int[] _transactionTypesEnglishAndMathsPayments = { 13, 14 };
        private readonly int[] _transactionTypesLearningSupportPayments = { 8, 9, 10, 11, 12, 15 };

        private int[] _fundingSourceEmpty => new int[] { };

        public IReadOnlyList<AppsMonthlyPaymentModel> BuildAppsMonthlyPaymentModelList(
            AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo,
            AppsMonthlyPaymentRulebaseInfo appsMonthlyPaymentRulebaseInfo,
            AppsMonthlyPaymentDASInfo appsMonthlyPaymentDasInfo,
            AppsMonthlyPaymentDasEarningsInfo appsMonthlyPaymentDasEarningsInfo,
            AppsMonthlyPaymentFcsInfo appsMonthlyPaymentFcsInfo,
            IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> appsMonthlyPaymentLarsLearningDeliveryInfoList)
        {
            List<AppsMonthlyPaymentModel> appsMonthlyPaymentModelList = null;

            try
            {
                appsMonthlyPaymentModelList = new List<AppsMonthlyPaymentModel>();

                /*
                --------------------------------------------------
                From Apps Monthly Payment Report spec version 3.1
                --------------------------------------------------

                BR1 – Applicable Records

                This report shows rows of calculated payment data related to funding appsMonthlyPaymentModel 36, and associated ILR data.

                ------------------------------------------------------------------------------------------------------------------------------------------
                *** There should be a new row on the report where the data is different for any of the following fields in the Payments2.Payment table:***
                ------------------------------------------------------------------------------------------------------------------------------------------
                • LearnerReferenceNumber
                • LearnerUln
                • LearningAimReference
                • LearningStartDate
                • LearningAimProgrammeType
                • LearningAimStandardCode
                • LearningAimFrameworkCode
                • LearningAimPathwayCode
                • ReportingAimFundingLineType
                • PriceEpisodeIdentifier(note that only programme aims(LearningAimReference = ZPROG001) have PriceEpisodeIdentifiers; maths and English aims do not)

                ----------------------------------------------------------------------------------------------------------------------------------------
                *** Where these fields are identical, multiple payments should be displayed in the appropriate monthly field and summed as necessary ***
                ----------------------------------------------------------------------------------------------------------------------------------------

                There may be multiple price episodes for an aim.  Only price episodes with a start date in the current funding year should be included on this report.
                Note that English and maths aims do not have price episodes, so there should be just one row per aim.
                */

                List<AppsMonthlyPaymentModel> paymentsGroupedByBr1List = appsMonthlyPaymentDasInfo.Payments
                    .Where(p => p.AcademicYear.Equals("1920"))
                    .GroupBy(r => new
                    {
                        r.Ukprn,
                        r.LearnerReferenceNumber,
                        r.LearnerUln,
                        r.LearningAimReference,
                        r.LearningStartDate,
                        r.LearningAimProgrammeType,
                        r.LearningAimStandardCode,
                        r.LearningAimFrameworkCode,
                        r.LearningAimPathwayCode,
                        r.ReportingAimFundingLineType,
                        r.PriceEpisodeIdentifier
                    })
                    .Select(g => new AppsMonthlyPaymentModel
                    {
                        Ukprn = g.Key.Ukprn,

                        // Br3 key columns
                        PaymentLearnerReferenceNumber = g.Key.LearnerReferenceNumber,
                        PaymentUniqueLearnerNumber = g.Key.LearnerUln,
                        PaymentLearningAimReference = g.Key.LearningAimReference,
                        PaymentLearningStartDate = g.Key.LearningStartDate,
                        PaymentProgrammeType = g.Key.LearningAimProgrammeType,
                        PaymentStandardCode = g.Key.LearningAimStandardCode,
                        PaymentFrameworkCode = g.Key.LearningAimFrameworkCode,
                        PaymentPathwayCode = g.Key.LearningAimPathwayCode,
                        PaymentFundingLineType = g.Key.ReportingAimFundingLineType,
                        PaymentPriceEpisodeIdentifier = g.Key.PriceEpisodeIdentifier,
                    }).ToList();

                foreach (var paymentGroup in paymentsGroupedByBr1List)
                {
                    // get the matching payment for this payment group in order to populate
                    // the monthly payment report model
                    //var paymentInfo = appsMonthlyPaymentDasInfo.Payments.FirstOrDefault(p =>
                    //    p.LearnerReferenceNumber.CaseInsensitiveEquals(paymentGroup.PaymentLearnerReferenceNumber) &&
                    //    p.LearnerUln == paymentGroup.PaymentUniqueLearnerNumber &&
                    //    p.LearningAimReference.CaseInsensitiveEquals(paymentGroup.PaymentLearningAimReference) &&
                    //    p.LearningStartDate == paymentGroup.PaymentLearningStartDate &&
                    //    p.LearningAimProgrammeType == paymentGroup.PaymentProgrammeType &&
                    //    p.LearningAimStandardCode == paymentGroup.PaymentStandardCode &&
                    //    p.LearningAimFrameworkCode == paymentGroup.PaymentFrameworkCode &&
                    //    p.LearningAimPathwayCode == paymentGroup.PaymentPathwayCode &&
                    //    p.PriceEpisodeIdentifier.CaseInsensitiveEquals(paymentGroup.PaymentPriceEpisodeIdentifier));

                    // get the matching DAS EarningEvents for this payment
                    //var earningsEvents = appsMonthlyPaymentDasEarningsInfo.Earnings
                    //    .Where(x => x.Id = paymentGroup.learningst)

                    // get the matching contract allocation number for this payment group in order to populate
                    // the Contractract No field in the apps monthly payment report model
                    string fundingStreamPeriod = string.Empty;
                    string contractAllocationNumber = string.Empty;
                    try
                    {
                        fundingStreamPeriod = Utils.GetFundingStreamPeriodForFundingLineType(paymentGroup.PaymentFundingLineType);
                        contractAllocationNumber = appsMonthlyPaymentFcsInfo.Contracts
                            .SelectMany(x => x.ContractAllocations)
                            .Where(y => y.FundingStreamPeriodCode == fundingStreamPeriod)
                            .Select(x => x.ContractAllocationNumber)
                            .DefaultIfEmpty("Contract Not Found!")
                            .FirstOrDefault();
                    }
                    catch (Exception e)
                    {
                        // TODO: log the exception
                    }

                    // get the matching ILR data for this payment group in order to populate
                    // the ILR fields in the apps monthly payment report model
                    AppsMonthlyPaymentLearnerInfo ilrInfo = null;
                    try
                    {
                        ilrInfo = appsMonthlyPaymentIlrInfo.Learners
                            .FirstOrDefault(i => i.Ukprn == paymentGroup.Ukprn &&
                                                 i.LearnRefNumber.CaseInsensitiveEquals(paymentGroup.PaymentLearnerReferenceNumber) &&
                                                 i.UniqueLearnerNumber.CaseInsensitiveEquals(paymentGroup.PaymentUniqueLearnerNumber));
                    }
                    catch (Exception e)
                    {
                        // TODO: log the exception
                    }

                    string providerSpecifiedLearnerMonitoringA = string.Empty;
                    try
                    {
                        providerSpecifiedLearnerMonitoringA = ilrInfo?.ProviderSpecLearnerMonitorings
                            ?.FirstOrDefault(x =>
                                string.Equals(x.ProvSpecLearnMonOccur, "A", StringComparison.OrdinalIgnoreCase))
                            ?.ProvSpecLearnMon;
                    }
                    catch (Exception e)
                    {
                        // TODO: log the exception
                    }

                    string providerSpecifiedLearnerMonitoringB = string.Empty;
                    try
                    {
                        providerSpecifiedLearnerMonitoringB = ilrInfo?.ProviderSpecLearnerMonitorings
                            ?.FirstOrDefault(x =>
                                string.Equals(x.ProvSpecLearnMonOccur, "B", StringComparison.OrdinalIgnoreCase))
                            ?.ProvSpecLearnMon;
                    }
                    catch (Exception e)
                    {
                        // TODO: log the exception
                    }

                    // get the LARS Learning Aim name from the learning aim reference for this payment group in order to populate
                    // the aim title field in the apps monthly payment report model
                    string larsLearningAimTitle = string.Empty;
                    try
                    {
                        larsLearningAimTitle = appsMonthlyPaymentLarsLearningDeliveryInfoList.FirstOrDefault(x =>
                            x.LearnAimRef == paymentGroup.PaymentLearningAimReference)?.LearningAimTitle;
                    }
                    catch (Exception e)
                    {
                        // TODO: log the exception
                    }

                    // Create and populate an apps monthly payment report model for each row of the report
                    var appsMonthlyPaymentModel = new AppsMonthlyPaymentModel()
                    {
                        Ukprn = paymentGroup.Ukprn,
                        PaymentLearnerReferenceNumber = paymentGroup.PaymentLearnerReferenceNumber,
                        PaymentUniqueLearnerNumber = paymentGroup.PaymentUniqueLearnerNumber,
                        LearnerCampusIdentifier = paymentGroup.LearnerCampusIdentifier,
                        ProviderSpecifiedLearnerMonitoringA = providerSpecifiedLearnerMonitoringA,
                        ProviderSpecifiedLearnerMonitoringB = providerSpecifiedLearnerMonitoringB,
                        PaymentEarningEventAimSeqNumber = 1, // TODO: Resolve EarningsEventId definition not matching table definition
                        PaymentLearningAimReference = paymentGroup.PaymentLearningAimReference,
                        LarsLearningDeliveryLearningAimTitle = larsLearningAimTitle,
                        LearningDeliveryOriginalLearningStartDate = paymentGroup.LearningDeliveryOriginalLearningStartDate,
                        PaymentLearningStartDate = paymentGroup.PaymentLearningStartDate,
                        LearningDeliveryLearningPlannedEndData = paymentGroup.LearningDeliveryLearningPlannedEndData,
                        LearningDeliveryCompletionStatus = paymentGroup.LearningDeliveryCompletionStatus,
                        LearningDeliveryLearningActualEndDate = paymentGroup.LearningDeliveryLearningActualEndDate,
                        LearningDeliveryAchievementDate = paymentGroup.LearningDeliveryAchievementDate,
                        LearningDeliveryOutcome = paymentGroup.LearningDeliveryOutcome,
                        PaymentProgrammeType = paymentGroup.PaymentProgrammeType,
                        PaymentStandardCode = paymentGroup.PaymentStandardCode,
                        PaymentFrameworkCode = paymentGroup.PaymentFrameworkCode,
                        PaymentPathwayCode = paymentGroup.PaymentPathwayCode,
                        LearningDeliveryAimType = paymentGroup.LearningDeliveryAimType,
                        LearningDeliverySoftwareSupplierAimIdentifier = paymentGroup.LearningDeliverySoftwareSupplierAimIdentifier,
                        LearningDeliveryFamTypeLearningDeliveryMonitoringA = paymentGroup.LearningDeliveryFamTypeLearningDeliveryMonitoringA,
                        LearningDeliveryFamTypeLearningDeliveryMonitoringB = paymentGroup.LearningDeliveryFamTypeLearningDeliveryMonitoringB,
                        LearningDeliveryFamTypeLearningDeliveryMonitoringC = paymentGroup.LearningDeliveryFamTypeLearningDeliveryMonitoringC,
                        LearningDeliveryFamTypeLearningDeliveryMonitoringD = paymentGroup.LearningDeliveryFamTypeLearningDeliveryMonitoringD,
                        LearningDeliveryFamTypeLearningDeliveryMonitoringE = paymentGroup.LearningDeliveryFamTypeLearningDeliveryMonitoringE,
                        LearningDeliveryFamTypeLearningDeliveryMonitoringF = paymentGroup.LearningDeliveryFamTypeLearningDeliveryMonitoringF,
                        ProviderSpecifiedDeliveryMonitoringA = paymentGroup.ProviderSpecifiedDeliveryMonitoringA,
                        ProviderSpecifiedDeliveryMonitoringB = paymentGroup.ProviderSpecifiedDeliveryMonitoringB,
                        ProviderSpecifiedDeliveryMonitoringC = paymentGroup.ProviderSpecifiedDeliveryMonitoringC,
                        ProviderSpecifiedDeliveryMonitoringD = paymentGroup.ProviderSpecifiedDeliveryMonitoringD,
                        LearningDeliveryEndPointAssessmentOrganisation = paymentGroup.LearningDeliveryEndPointAssessmentOrganisation,
                        RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim = paymentGroup.RulebaseAecLearningDeliveryPlannedNumberOfOnProgrammeInstalmentsForAim,
                        LearningDeliverySubContractedOrPartnershipUkprn = paymentGroup.LearningDeliverySubContractedOrPartnershipUkprn,
                        PaymentPriceEpisodeIdentifier = paymentGroup.PaymentPriceEpisodeIdentifier,
                        PaymentPriceEpisodeStartDate = paymentGroup.PaymentPriceEpisodeStartDate,
                        RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate = paymentGroup.RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate,
                        FcsContractContractAllocationContractAllocationNumber = paymentGroup.FcsContractContractAllocationContractAllocationNumber,
                        PaymentFundingLineType = paymentGroup.PaymentFundingLineType,
                        PaymentApprenticeshipContractType = paymentGroup.PaymentApprenticeshipContractType,
                        LearnerEmploymentStatusEmployerId = paymentGroup.LearnerEmploymentStatusEmployerId,
                        RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier = paymentGroup.RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier,
                        LearnerEmploymentStatus = paymentGroup.LearnerEmploymentStatus,
                        LearnerEmploymentStatusDate = paymentGroup.LearnerEmploymentStatusDate,

                        //TotalPayments = paymentGroup.LevyPayments.Sum(),
                        //TotalCoInvestmentPayments = paymentGroup.CoInvestmentPayments.Sum(),
                        //TotalCoInvestmentDueFromEmployerPayments = paymentGroup.CoInvestmentDueFromEmployerPayments.Sum(),
                        //TotalEmployerAdditionalPayments = paymentGroup.EmployerAdditionalPayments.Sum(),
                        //TotalProviderAdditionalPayments = paymentGroup.ProviderAdditionalPayments.Sum(),
                        //TotalEnglishAndMathsPayments = paymentGroup.EnglishAndMathsPayments.Sum(),
                        //TotalLearningSupportDisadvantageAndFrameworkUpliftPayments = paymentGroup.LearningSupportDisadvantageAndFrameworkUpliftPayments.Sum()

                        //            List<AppsMonthlyPaymentDASPaymentInfo> appsMonthlyPaymentDasPaymentInfos = paymentGroup.ToList();

                        //            PopulatePayments(appsMonthlyPaymentModel, appsMonthlyPaymentDasPaymentInfos);
                        //            PopulateMonthlyTotalPayments(appsMonthlyPaymentModel);
                        //            appsMonthlyPaymentModel.TotalLevyPayments = appsMonthlyPaymentModel.LevyPayments.Sum();
                        //            appsMonthlyPaymentModel.TotalCoInvestmentPayments = appsMonthlyPaymentModel.CoInvestmentPayments.Sum();
                        //            appsMonthlyPaymentModel.TotalCoInvestmentDueFromEmployerPayments = appsMonthlyPaymentModel.CoInvestmentDueFromEmployerPayments.Sum();
                        //            appsMonthlyPaymentModel.TotalEmployerAdditionalPayments = appsMonthlyPaymentModel.EmployerAdditionalPayments.Sum();
                        //            appsMonthlyPaymentModel.TotalProviderAdditionalPayments = appsMonthlyPaymentModel.ProviderAdditionalPayments.Sum();
                        //            appsMonthlyPaymentModel.TotalApprenticeAdditionalPayments = appsMonthlyPaymentModel.ApprenticeAdditionalPayments.Sum();
                        //            appsMonthlyPaymentModel.TotalEnglishAndMathsPayments = appsMonthlyPaymentModel.EnglishAndMathsPayments.Sum();
                        //            appsMonthlyPaymentModel.TotalPaymentsForLearningSupportDisadvantageAndFrameworkUplifts = appsMonthlyPaymentModel.LearningSupportDisadvantageAndFrameworkUpliftPayments.Sum();
                        //            PopulateTotalPayments(appsMonthlyPaymentModel);
                    };

                    appsMonthlyPaymentModelList.Add(appsMonthlyPaymentModel);
                }
            }
            catch (Exception ex)
            {
                var y = ex;
                //_logger.LogError("Failed to get Rulebase data", ex);
            }

            return appsMonthlyPaymentModelList;
        }

        //List<ResultLine> result = Lines
        //    .GroupBy(l => l.ProductCode)
        //    .Select(cl => new ResultLine
        //    {
        //        ProductName = cl.First().Name,
        //        Quantity = cl.Count().ToString(),
        //        Price = cl.Sum(c => c.Price).ToString(),
        //    }).ToList();

        //foreach (var payment in paymentsGroupedByBR1)
        //{
        //    var appsMonthlyPaymentModel2 = new AppsMonthlyPaymentModel()
        //    {
        //        PaymentLearnerReferenceNumber = payment.Key.LearnerReferenceNumber
        //    };
        //    appsMonthlyPaymentModelList.Add(appsMonthlyPaymentModel2);
        //}

        //    foreach (var learner in appsMonthlyPaymentIlrInfo.Learners)
        //    {
        //        var paymentGroups = appsMonthlyPaymentDasInfo.Payments.Where(x => x.LearnerReferenceNumber.CaseInsensitiveEquals(learner.LearnRefNumber))
        //            .GroupBy(x => new
        //            {
        //                x.UkPrn,
        //                x.LearnerReferenceNumber,
        //                x.LearnerUln,
        //                x.LearningAimReference,
        //                x.LearningStartDate,
        //                x.LearningAimProgrammeType,
        //                x.LearningAimStandardCode,
        //                x.LearningAimFrameworkCode,
        //                x.LearningAimPathwayCode,
        //                x.ReportingAimFundingLineType,
        //                x.PriceEpisodeIdentifier
        //            });

        //        foreach (var paymentGroup in paymentGroups)
        //        {
        //            var learningDeliveryInfo = learner.LearningDeliveries.SingleOrDefault(x =>
        //                x.UKPRN == paymentGroup.First().UkPrn &&
        //                x.LearnRefNumber.CaseInsensitiveEquals(paymentGroup.Key.LearnerReferenceNumber) &&
        //                x.LearnAimRef.CaseInsensitiveEquals(paymentGroup.Key.LearningAimReference) &&
        //                x.LearnStartDate == paymentGroup.Key.LearningStartDate &&
        //                x.ProgType == paymentGroup.Key.LearningAimProgrammeType &&
        //                x.StdCode == paymentGroup.Key.LearningAimStandardCode &&
        //                x.FworkCode == paymentGroup.Key.LearningAimFrameworkCode &&
        //                x.PwayCode == paymentGroup.Key.LearningAimPathwayCode);

        //            var aecApprenticeshipPriceEpisode =
        //                appsMonthlyPaymentRulebaseInfo.AECApprenticeshipPriceEpisodes.SingleOrDefault(x =>
        //                    x.UkPrn == learningDeliveryInfo?.UKPRN &&
        //                    x.LearnRefNumber == learningDeliveryInfo.LearnRefNumber &&
        //                    x.AimSequenceNumber == learningDeliveryInfo.AimSeqNumber);

        //            string fundingStreamPeriod = Utils.GetFundingStreamPeriodForFundingLineType(paymentGroup.Key.ReportingAimFundingLineType);

        //            var contractAllocationNumber = appsMonthlyPaymentFcsInfo.Contracts
        //                .SelectMany(x => x.ContractAllocations)
        //                .Where(y => y.FundingStreamPeriodCode == fundingStreamPeriod)
        //                .Select(x => x.ContractAllocationNumber)
        //                .DefaultIfEmpty("Contract Not Found!")
        //                .FirstOrDefault();

        //            var appsMonthlyPaymentModel = new AppsMonthlyPaymentModel()
        //            {
        //                PaymentLearnerReferenceNumber = paymentGroup.Key.LearnerReferenceNumber,
        //                PaymentUniqueLearnerNumber = paymentGroup.Key.LearnerUln,

        //                LearnerCampusIdentifier = learner.CampId,

        //                ProviderSpecifiedLearnerMonitoringA = learner.ProviderSpecLearnerMonitorings
        //                    ?.SingleOrDefault(x =>
        //                        string.Equals(x.ProvSpecLearnMonOccur, "A", StringComparison.OrdinalIgnoreCase))
        //                    ?.ProvSpecLearnMon,
        //                ProviderSpecifiedLearnerMonitoringB = learner.ProviderSpecLearnerMonitorings
        //                    ?.SingleOrDefault(x =>
        //                        string.Equals(x.ProvSpecLearnMonOccur, "B", StringComparison.OrdinalIgnoreCase))
        //                    ?.ProvSpecLearnMon,

        //                // ---------------------------------------------------------------------
        //                // TODO: Get AimSeqNumber from the Payments2.EarningEvent table
        //                // ---------------------------------------------------------------------
        //                // PaymentsEarningEventAimSeqNumber = learningDeliveryInfo.AimSeqNumber,
        //                // ---------------------------------------------------------------------

        //                PaymentLearningAimReference = paymentGroup.Key.LearningAimReference,

        //                LarsLearningDeliveryLearningAimTitle = appsMonthlyPaymentLarsLearningDeliveryInfoList?.FirstOrDefault(x => x.LearnAimRef.CaseInsensitiveEquals(learningDeliveryInfo.LearnAimRef))?.LearningAimTitle,

        //                PaymentLearningStartDate = paymentGroup.Key.LearningStartDate?.ToString("dd/MM/yyyy"),
        //                PaymentProgrammeType = paymentGroup.Key.LearningAimProgrammeType,
        //                PaymentStandardCode = paymentGroup.Key.LearningAimStandardCode,
        //                PaymentFrameworkCode = paymentGroup.Key.LearningAimFrameworkCode,
        //                PaymentPathwayCode = paymentGroup.Key.LearningAimPathwayCode,

        //                LearningDeliveryAimType = learningDeliveryInfo.AimType,
        //                LearningDeliverySoftwareSupplierAimIdentifier = learningDeliveryInfo.SWSupAimId,

        //                ProviderSpecifiedDeliveryMonitoringA = learningDeliveryInfo.ProviderSpecDeliveryMonitorings
        //                    ?.SingleOrDefault(x =>
        //                        string.Equals(x.ProvSpecDelMonOccur, "A", StringComparison.OrdinalIgnoreCase))
        //                    ?.ProvSpecDelMon,
        //                ProviderSpecifiedDeliveryMonitoringB = learningDeliveryInfo.ProviderSpecDeliveryMonitorings
        //                    ?.SingleOrDefault(x =>
        //                        string.Equals(x.ProvSpecDelMonOccur, "B", StringComparison.OrdinalIgnoreCase))
        //                    ?.ProvSpecDelMon,
        //                ProviderSpecifiedDeliveryMonitoringC = learningDeliveryInfo.ProviderSpecDeliveryMonitorings
        //                    ?.SingleOrDefault(x =>
        //                        string.Equals(x.ProvSpecDelMonOccur, "C", StringComparison.OrdinalIgnoreCase))
        //                    ?.ProvSpecDelMon,
        //                ProviderSpecifiedDeliveryMonitoringD = learningDeliveryInfo.ProviderSpecDeliveryMonitorings
        //                    ?.SingleOrDefault(x =>
        //                        string.Equals(x.ProvSpecDelMonOccur, "D", StringComparison.OrdinalIgnoreCase))
        //                    ?.ProvSpecDelMon,

        //                LearningDeliveryEndPointAssessmentOrganisation = learningDeliveryInfo.EPAOrganisation,
        //                LearningDeliverySubContractedOrPartnershipUkprn = learningDeliveryInfo.PartnerUkPrn.ToString(),

        //                PaymentPriceEpisodeStartDate = paymentGroup.Key.LearningAimReference.CaseInsensitiveEquals(ZPROG001) && paymentGroup.Key.PriceEpisodeIdentifier.Length > 10
        //                    ? paymentGroup.Key.PriceEpisodeIdentifier.Substring(paymentGroup.Key.PriceEpisodeIdentifier.Length - 10)
        //                    : string.Empty,

        //                RulebaseAecApprenticeshipPriceEpisodePriceEpisodeActualEndDate = aecApprenticeshipPriceEpisode?.PriceEpisodeActualEndDate
        //                    .GetValueOrDefault().ToString("dd/MM/yyyy"),

        //                FcsContractContractAllocationContractAllocationNumber = contractAllocationNumber,

        //                PaymentFundingLineType = paymentGroup.Key.ReportingAimFundingLineType,

        //                PaymentApprenticeshipContractType = paymentGroup.First().ContractType.ToString(),

        //                RulebaseAecApprenticeshipPriceEpisodeAgreementIdentifier = aecApprenticeshipPriceEpisode?.PriceEpisodeAgreeId,
        //            };

        //            List<AppsMonthlyPaymentDASPaymentInfo> appsMonthlyPaymentDasPaymentInfos = paymentGroup.ToList();

        //            PopulatePayments(appsMonthlyPaymentModel, appsMonthlyPaymentDasPaymentInfos);
        //            PopulateMonthlyTotalPayments(appsMonthlyPaymentModel);
        //            appsMonthlyPaymentModel.TotalLevyPayments = appsMonthlyPaymentModel.LevyPayments.Sum();
        //            appsMonthlyPaymentModel.TotalCoInvestmentPayments = appsMonthlyPaymentModel.CoInvestmentPayments.Sum();
        //            appsMonthlyPaymentModel.TotalCoInvestmentDueFromEmployerPayments = appsMonthlyPaymentModel.CoInvestmentDueFromEmployerPayments.Sum();
        //            appsMonthlyPaymentModel.TotalEmployerAdditionalPayments = appsMonthlyPaymentModel.EmployerAdditionalPayments.Sum();
        //            appsMonthlyPaymentModel.TotalProviderAdditionalPayments = appsMonthlyPaymentModel.ProviderAdditionalPayments.Sum();
        //            appsMonthlyPaymentModel.TotalApprenticeAdditionalPayments = appsMonthlyPaymentModel.ApprenticeAdditionalPayments.Sum();
        //            appsMonthlyPaymentModel.TotalEnglishAndMathsPayments = appsMonthlyPaymentModel.EnglishAndMathsPayments.Sum();
        //            appsMonthlyPaymentModel.TotalPaymentsForLearningSupportDisadvantageAndFrameworkUplifts = appsMonthlyPaymentModel.LearningSupportDisadvantageAndFrameworkUpliftPayments.Sum();
        //            PopulateTotalPayments(appsMonthlyPaymentModel);
        //            appsMonthlyPaymentModelList.Add(appsMonthlyPaymentModel);
        //        }
        //    }

        private void PopulatePayments(AppsMonthlyPaymentModel appsMonthlyPaymentModel, List<AppsMonthlyPaymentDASPaymentInfo> appsMonthlyPaymentDasPaymentInfo)
        {
            appsMonthlyPaymentModel.LevyPayments = new decimal[14];
            appsMonthlyPaymentModel.CoInvestmentPayments = new decimal[14];
            appsMonthlyPaymentModel.CoInvestmentDueFromEmployerPayments = new decimal[14];
            appsMonthlyPaymentModel.EmployerAdditionalPayments = new decimal[14];
            appsMonthlyPaymentModel.ProviderAdditionalPayments = new decimal[14];
            appsMonthlyPaymentModel.ApprenticeAdditionalPayments = new decimal[14];
            appsMonthlyPaymentModel.EnglishAndMathsPayments = new decimal[14];
            appsMonthlyPaymentModel.LearningSupportDisadvantageAndFrameworkUpliftPayments = new decimal[14];
            for (int i = 0; i <= 13; i++)
            {
                appsMonthlyPaymentModel.LevyPayments[i] = GetPayments(appsMonthlyPaymentDasPaymentInfo, _collectionPeriods[i], _fundingSourceLevyPayments, _transactionTypesLevyPayments);
                appsMonthlyPaymentModel.CoInvestmentPayments[i] = GetPayments(appsMonthlyPaymentDasPaymentInfo, _collectionPeriods[i], _fundingSourceCoInvestmentPayments, _transactionTypesCoInvestmentPayments);
                appsMonthlyPaymentModel.CoInvestmentDueFromEmployerPayments[i] = GetPayments(appsMonthlyPaymentDasPaymentInfo, _collectionPeriods[i], _fundingSourceCoInvestmentDueFromEmployer, _transactionTypesCoInvestmentFromEmployer);
                appsMonthlyPaymentModel.EmployerAdditionalPayments[i] = GetPayments(appsMonthlyPaymentDasPaymentInfo, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesEmployerAdditionalPayments);
                appsMonthlyPaymentModel.ProviderAdditionalPayments[i] = GetPayments(appsMonthlyPaymentDasPaymentInfo, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesProviderAdditionalPayments);
                appsMonthlyPaymentModel.ApprenticeAdditionalPayments[i] = GetPayments(appsMonthlyPaymentDasPaymentInfo, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesApprenticeshipAdditionalPayments);
                appsMonthlyPaymentModel.EnglishAndMathsPayments[i] = GetPayments(appsMonthlyPaymentDasPaymentInfo, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesEnglishAndMathsPayments);
                appsMonthlyPaymentModel.LearningSupportDisadvantageAndFrameworkUpliftPayments[i] = GetPayments(appsMonthlyPaymentDasPaymentInfo, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesLearningSupportPayments);
            }
        }

        private void PopulateMonthlyTotalPayments(AppsMonthlyPaymentModel appsMonthlyPaymentModel)
        {
            appsMonthlyPaymentModel.TotalMonthlyPayments = new decimal[14];
            for (int i = 0; i <= 13; i++)
            {
                appsMonthlyPaymentModel.TotalMonthlyPayments[i] = appsMonthlyPaymentModel.LevyPayments[i] + appsMonthlyPaymentModel.CoInvestmentPayments[i] + appsMonthlyPaymentModel.CoInvestmentDueFromEmployerPayments[i] +
                                                appsMonthlyPaymentModel.EmployerAdditionalPayments[i] + appsMonthlyPaymentModel.ProviderAdditionalPayments[i] + appsMonthlyPaymentModel.ApprenticeAdditionalPayments[i] +
                                                appsMonthlyPaymentModel.EnglishAndMathsPayments[i] + appsMonthlyPaymentModel.LearningSupportDisadvantageAndFrameworkUpliftPayments[i];
            }
        }

        private void PopulateTotalPayments(AppsMonthlyPaymentModel appsMonthlyPaymentModel)
        {
            appsMonthlyPaymentModel.TotalPayments = appsMonthlyPaymentModel.TotalLevyPayments +
                                  appsMonthlyPaymentModel.TotalCoInvestmentPayments +
                                  appsMonthlyPaymentModel.TotalCoInvestmentDueFromEmployerPayments +
                                  appsMonthlyPaymentModel.TotalEmployerAdditionalPayments +
                                  appsMonthlyPaymentModel.TotalProviderAdditionalPayments +
                                  appsMonthlyPaymentModel.TotalApprenticeAdditionalPayments +
                                  appsMonthlyPaymentModel.TotalEnglishAndMathsPayments +
                                  appsMonthlyPaymentModel.TotalLearningSupportDisadvantageAndFrameworkUpliftPayments;
        }

        private decimal GetPayments(
            List<AppsMonthlyPaymentDASPaymentInfo> appsMonthlyPaymentDasPaymentInfos,
            string collectionPeriodName,
            int[] fundingSource,
            int[] transactionTypes)
        {
            decimal payment = 0;
            foreach (var paymentInfo in appsMonthlyPaymentDasPaymentInfos)
            {
                if (fundingSource.Length > 0)
                {
                    //if (paymentInfo.CollectionPeriod.ToCollectionPeriodName(paymentInfo.AcademicYear.ToString()).CaseInsensitiveEquals(collectionPeriodName) &&
                    //    transactionTypes.Contains(paymentInfo.TransactionType) &&
                    //    fundingSource.Contains(paymentInfo.FundingSource))
                    {
                        payment += paymentInfo.Amount;
                    }
                }

                //else if (paymentInfo.CollectionPeriod.ToCollectionPeriodName(paymentInfo.AcademicYear.ToString()).CaseInsensitiveEquals(collectionPeriodName) &&
                ////         transactionTypes.Contains(paymentInfo.TransactionType))
                //{
                //    payment += paymentInfo.Amount;
                //}
            }

            return payment;
        }
    }
}
