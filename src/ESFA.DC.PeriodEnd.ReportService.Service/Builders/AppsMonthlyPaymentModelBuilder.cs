using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Interface.Utils;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsMonthlyPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

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

        public IReadOnlyList<AppsMonthlyPaymentModel> BuildModel(
            AppsMonthlyPaymentILRInfo appsMonthlyPaymentIlrInfo,
            AppsMonthlyPaymentRulebaseInfo appsMonthlyPaymentRulebaseInfo,
            AppsMonthlyPaymentDASInfo appsMonthlyPaymentDasInfo,
            AppsMonthlyPaymentFcsInfo appsMonthlyPaymentFcsInfo,
            IReadOnlyList<AppsMonthlyPaymentLarsLearningDeliveryInfo> appsMonthlyPaymentLarsLearningDeliveryInfoList)
        {
            List<AppsMonthlyPaymentModel> appsMonthlyPaymentModels = null;

            try
            {
                appsMonthlyPaymentModels = new List<AppsMonthlyPaymentModel>();

                /*
                --------------------------------------------------
                From Apps Monthly Payment Report spec version 3.1
                --------------------------------------------------

                BR1 – Applicable Records

                This report shows rows of calculated payment data related to funding model 36, and associated ILR data.

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
                • LearningAimFundingLineType
                • PriceEpisodeIdentifier(note that only programme aims(LearningAimReference = ZPROG001) have PriceEpisodeIdentifiers; maths and English aims do not)

                ----------------------------------------------------------------------------------------------------------------------------------------
                *** Where these fields are identical, multiple payments should be displayed in the appropriate monthly field and summed as necessary ***
                ----------------------------------------------------------------------------------------------------------------------------------------

                There may be multiple price episodes for an aim.  Only price episodes with a start date in the current funding year should be included on this report.
                Note that English and maths aims do not have price episodes, so there should be just one row per aim.
                */

                //var paymentsGroupedByBR1 = appsMonthlyPaymentDasInfo.Payments
                //    .Where(f => f.AcademicYear = 1920)
                //    .GroupBy(r => new
                //    {
                //        r.UkPrn,
                //        r.LearnerReferenceNumber,
                //        r.LearnerUln,
                //        r.LearningAimReference,
                //        r.LearningStartDate,
                //        r.LearningAimProgrammeType,
                //        r.LearningAimStandardCode,
                //        r.LearningAimFrameworkCode,
                //        r.LearningAimPathwayCode,
                //        r.LearningAimFundingLineType,
                //        r.PriceEpisodeIdentifier
                //    })
                //    .OrderBy(g => new
                //    {
                //        g.Key.UkPrn,
                //        g.Key.LearnerReferenceNumber,
                //        g.Key.LearnerUln,
                //        g.Key.LearningAimReference,
                //        g.Key.LearningStartDate,
                //        g.Key.LearningAimProgrammeType,
                //        g.Key.LearningAimStandardCode,
                //        g.Key.LearningAimFrameworkCode,
                //        g.Key.LearningAimPathwayCode,
                //        g.Key.LearningAimFundingLineType,
                //        g.Key.PriceEpisodeIdentifier
                //    })

                    //.Select(g => new {      });
                //g.UkPrn,
                        //g.LearnerReferenceNumber,
                        //g.LearnerUln,
                        //g.LearningAimReference,
                        //g.LearningStartDate,
                        //g.LearningAimProgrammeType,
                        //g.LearningAimStandardCode,
                        //g.LearningAimFrameworkCode,
                        //g.LearningAimPathwayCode,
                        //g.LearningAimFundingLineType,
                        //g.PriceEpisodeIdentifier

                //var authorCategoryRecipes =
                //    db.Recipes
                //        .GroupBy(r => new { r.Author, r.Category })
                //        .OrderBy(g => g.Key.Author)
                //        .Select(g => new { Author = g.Key, RecipeCount = g.Count() });

                foreach (var learner in appsMonthlyPaymentIlrInfo.Learners)
                {
                    var paymentGroups = appsMonthlyPaymentDasInfo.Payments.Where(x => x.LearnerReferenceNumber.CaseInsensitiveEquals(learner.LearnRefNumber))
                        .GroupBy(x => new
                        {
                            x.UkPrn,
                            x.LearnerReferenceNumber,
                            x.LearnerUln,
                            x.LearningAimReference,
                            x.LearningStartDate,
                            x.LearningAimProgrammeType,
                            x.LearningAimStandardCode,
                            x.LearningAimFrameworkCode,
                            x.LearningAimPathwayCode,
                            x.LearningAimFundingLineType,
                            x.PriceEpisodeIdentifier
                        });

                    foreach (var paymentGroup in paymentGroups)
                    {
                        var learningDeliveryInfo = learner.LearningDeliveries.SingleOrDefault(x =>
                            x.UKPRN == paymentGroup.First().UkPrn &&
                            x.LearnRefNumber.CaseInsensitiveEquals(paymentGroup.Key.LearnerReferenceNumber) &&
                            x.LearnAimRef.CaseInsensitiveEquals(paymentGroup.Key.LearningAimReference) &&
                            x.LearnStartDate == paymentGroup.Key.LearningStartDate &&
                            x.ProgType == paymentGroup.Key.LearningAimProgrammeType &&
                            x.StdCode == paymentGroup.Key.LearningAimStandardCode &&
                            x.FworkCode == paymentGroup.Key.LearningAimFrameworkCode &&
                            x.PwayCode == paymentGroup.Key.LearningAimPathwayCode);

                        var aecApprenticeshipPriceEpisode =
                            appsMonthlyPaymentRulebaseInfo.AECApprenticeshipPriceEpisodes.SingleOrDefault(x =>
                                x.UkPrn == learningDeliveryInfo?.UKPRN &&
                                x.LearnRefNumber == learningDeliveryInfo.LearnRefNumber &&
                                x.AimSequenceNumber == learningDeliveryInfo.AimSeqNumber);

                        //var reportLearningAimFundingLineType =

                        string fundingStreamPeriod = Utils.GetFundingStreamPeriodForFundingLineType(paymentGroup.Key.LearningAimFundingLineType);

                       //var contractAllocationNumber = appsMonthlyPaymentFcsInfo.Contracts.SingleOrDefault(x => x.ContractNumber == )
                            //.FirstOrDefault()
                            //.ContractAllocations
                            //.FirstOrDefault(x => x.FundingStreamPeriodCode == fundingStreamPeriod)
                            //.ContractAllocationNumber.ToString();

                        var model = new AppsMonthlyPaymentModel()
                        {
                            LearnerReferenceNumber = paymentGroup.Key.LearnerReferenceNumber,
                            UniqueLearnerNumber = paymentGroup.Key.LearnerUln,
                            CampusIdentifier = learner.CampId,
                            ProviderSpecifiedLearnerMonitoringA = learner.ProviderSpecLearnerMonitorings
                                ?.SingleOrDefault(x =>
                                    string.Equals(x.ProvSpecLearnMonOccur, "A", StringComparison.OrdinalIgnoreCase))
                                ?.ProvSpecLearnMon,
                            ProviderSpecifiedLearnerMonitoringB = learner.ProviderSpecLearnerMonitorings
                                ?.SingleOrDefault(x =>
                                    string.Equals(x.ProvSpecLearnMonOccur, "B", StringComparison.OrdinalIgnoreCase))
                                ?.ProvSpecLearnMon,
                            AimSeqNumber = learningDeliveryInfo.AimSeqNumber,
                            LearningAimReference = paymentGroup.Key.LearningAimReference,
                            LearningAimTitle = appsMonthlyPaymentLarsLearningDeliveryInfoList?.FirstOrDefault(x => x.LearnAimRef.CaseInsensitiveEquals(learningDeliveryInfo.LearnAimRef))?.LearningAimTitle,
                            LearningStartDate = paymentGroup.Key.LearningStartDate?.ToString("dd/MM/yyyy"),
                            LearningAimProgrammeType = paymentGroup.Key.LearningAimProgrammeType,
                            LearningAimStandardCode = paymentGroup.Key.LearningAimStandardCode,
                            LearningAimFrameworkCode = paymentGroup.Key.LearningAimFrameworkCode,
                            LearningAimPathwayCode = paymentGroup.Key.LearningAimPathwayCode,
                            AimType = learningDeliveryInfo.AimType,
                            SoftwareSupplierAimIdentifier = learningDeliveryInfo.SWSupAimId,
                            ProviderSpecifiedDeliveryMonitoringA = learningDeliveryInfo.ProviderSpecDeliveryMonitorings
                                ?.SingleOrDefault(x =>
                                    string.Equals(x.ProvSpecDelMonOccur, "A", StringComparison.OrdinalIgnoreCase))
                                ?.ProvSpecDelMon,
                            ProviderSpecifiedDeliveryMonitoringB = learningDeliveryInfo.ProviderSpecDeliveryMonitorings
                                ?.SingleOrDefault(x =>
                                    string.Equals(x.ProvSpecDelMonOccur, "B", StringComparison.OrdinalIgnoreCase))
                                ?.ProvSpecDelMon,
                            ProviderSpecifiedDeliveryMonitoringC = learningDeliveryInfo.ProviderSpecDeliveryMonitorings
                                ?.SingleOrDefault(x =>
                                    string.Equals(x.ProvSpecDelMonOccur, "C", StringComparison.OrdinalIgnoreCase))
                                ?.ProvSpecDelMon,
                            ProviderSpecifiedDeliveryMonitoringD = learningDeliveryInfo.ProviderSpecDeliveryMonitorings
                                ?.SingleOrDefault(x =>
                                    string.Equals(x.ProvSpecDelMonOccur, "D", StringComparison.OrdinalIgnoreCase))
                                ?.ProvSpecDelMon,
                            EndPointAssessmentOrganisation = learningDeliveryInfo.EPAOrganisation,
                            SubContractedOrPartnershipUKPRN = learningDeliveryInfo.PartnerUkPrn,
                            PriceEpisodeStartDate = paymentGroup.Key.LearningAimReference.CaseInsensitiveEquals(ZPROG001) && paymentGroup.Key.PriceEpisodeIdentifier.Length > 10
                                ? paymentGroup.Key.PriceEpisodeIdentifier.Substring(paymentGroup.Key.PriceEpisodeIdentifier.Length - 10)
                                : string.Empty,
                            PriceEpisodeActualEndDate = aecApprenticeshipPriceEpisode?.PriceEpisodeActualEndDate
                                .GetValueOrDefault().ToString("dd/MM/yyyy"),
                            //ContractNo = contractAllocationNumber,
                            FundingLineType = paymentGroup.Key.LearningAimFundingLineType,
                            LearningDeliveryFAMTypeApprenticeshipContractType = paymentGroup.First().ContractType,
                            AgreementIdentifier = aecApprenticeshipPriceEpisode?.PriceEpisodeAgreeId,
                        };

                        List<AppsMonthlyPaymentDASPaymentInfo> appsMonthlyPaymentDasPaymentInfos = paymentGroup.ToList();

                        PopulatePayments(model, appsMonthlyPaymentDasPaymentInfos);
                        PopulateMonthlyTotalPayments(model);
                        model.TotalLevyPayments = model.LevyPayments.Sum();
                        model.TotalCoInvestmentPayments = model.CoInvestmentPayments.Sum();
                        model.TotalCoInvestmentDueFromEmployerPayments = model.CoInvestmentDueFromEmployerPayments.Sum();
                        model.TotalEmployerAdditionalPayments = model.EmployerAdditionalPayments.Sum();
                        model.TotalProviderAdditionalPayments = model.ProviderAdditionalPayments.Sum();
                        model.TotalApprenticeAdditionalPayments = model.ApprenticeAdditionalPayments.Sum();
                        model.TotalEnglishAndMathsPayments = model.EnglishAndMathsPayments.Sum();
                        model.TotalPaymentsForLearningSupport = model.PaymentsForLearningSupport.Sum();
                        PopulateTotalPayments(model);
                        appsMonthlyPaymentModels.Add(model);
                    }
                }
            }
            catch (Exception ex)
            {
                var y = ex;
                //_logger.LogError("Failed to get Rulebase data", ex);
            }

            return appsMonthlyPaymentModels;
        }

        private void PopulatePayments(AppsMonthlyPaymentModel model, List<AppsMonthlyPaymentDASPaymentInfo> paymentInfos)
        {
            model.LevyPayments = new decimal[14];
            model.CoInvestmentPayments = new decimal[14];
            model.CoInvestmentDueFromEmployerPayments = new decimal[14];
            model.EmployerAdditionalPayments = new decimal[14];
            model.ProviderAdditionalPayments = new decimal[14];
            model.ApprenticeAdditionalPayments = new decimal[14];
            model.EnglishAndMathsPayments = new decimal[14];
            model.PaymentsForLearningSupport = new decimal[14];
            for (int i = 0; i <= 13; i++)
            {
                model.LevyPayments[i] = GetPayments(paymentInfos, _collectionPeriods[i], _fundingSourceLevyPayments, _transactionTypesLevyPayments);
                model.CoInvestmentPayments[i] = GetPayments(paymentInfos, _collectionPeriods[i], _fundingSourceCoInvestmentPayments, _transactionTypesCoInvestmentPayments);
                model.CoInvestmentDueFromEmployerPayments[i] = GetPayments(paymentInfos, _collectionPeriods[i], _fundingSourceCoInvestmentDueFromEmployer, _transactionTypesCoInvestmentFromEmployer);
                model.EmployerAdditionalPayments[i] = GetPayments(paymentInfos, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesEmployerAdditionalPayments);
                model.ProviderAdditionalPayments[i] = GetPayments(paymentInfos, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesProviderAdditionalPayments);
                model.ApprenticeAdditionalPayments[i] = GetPayments(paymentInfos, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesApprenticeshipAdditionalPayments);
                model.EnglishAndMathsPayments[i] = GetPayments(paymentInfos, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesEnglishAndMathsPayments);
                model.PaymentsForLearningSupport[i] = GetPayments(paymentInfos, _collectionPeriods[i], _fundingSourceEmpty, _transactionTypesLearningSupportPayments);
            }
        }

        private void PopulateMonthlyTotalPayments(AppsMonthlyPaymentModel model)
        {
            model.TotalMonthlyPayments = new decimal[14];
            for (int i = 0; i <= 13; i++)
            {
                model.TotalMonthlyPayments[i] = model.LevyPayments[i] + model.CoInvestmentPayments[i] + model.CoInvestmentDueFromEmployerPayments[i] +
                                                model.EmployerAdditionalPayments[i] + model.ProviderAdditionalPayments[i] + model.ApprenticeAdditionalPayments[i] +
                                                model.EnglishAndMathsPayments[i] + model.PaymentsForLearningSupport[i];
            }
        }

        private void PopulateTotalPayments(AppsMonthlyPaymentModel model)
        {
            model.TotalPayments = model.TotalLevyPayments +
                                  model.TotalCoInvestmentPayments +
                                  model.TotalCoInvestmentDueFromEmployerPayments +
                                  model.TotalEmployerAdditionalPayments +
                                  model.TotalProviderAdditionalPayments +
                                  model.TotalApprenticeAdditionalPayments +
                                  model.TotalEnglishAndMathsPayments +
                                  model.TotalPaymentsForLearningSupport;
        }

        private decimal GetPayments(List<AppsMonthlyPaymentDASPaymentInfo> appsMonthlyPaymentDasPaymentInfos, string collectionPeriodName, int[] fundingSource, int[] transactionTypes)
        {
            decimal payment = 0;
            foreach (var paymentInfo in appsMonthlyPaymentDasPaymentInfos)
            {
                if (fundingSource.Length > 0)
                {
                    if (paymentInfo.CollectionPeriod.ToCollectionPeriodName(paymentInfo.AcademicYear.ToString()).CaseInsensitiveEquals(collectionPeriodName) &&
                        transactionTypes.Contains(paymentInfo.TransactionType) &&
                        fundingSource.Contains(paymentInfo.FundingSource))
                    {
                        payment += paymentInfo.Amount;
                    }
                }
                else if (paymentInfo.CollectionPeriod.ToCollectionPeriodName(paymentInfo.AcademicYear.ToString()).CaseInsensitiveEquals(collectionPeriodName) &&
                         transactionTypes.Contains(paymentInfo.TransactionType))
                {
                    payment += paymentInfo.Amount;
                }
            }

            return payment;
        }
    }
}
