using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Constants;
using ESFA.DC.PeriodEnd.ReportService.Reports.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.Enums;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Data;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;
using IPeriodisedValuesLookup = ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.IPeriodisedValuesLookup;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.FundingSummary
{
    public class FundingSummaryModelBuilder : IFundingSummaryModelBuilder
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        private const string lastSubmittedIlrFileDateStringFormat = "dd/MM/yyyy HH:mm:ss";
        private const string ilrFileNameDateTimeParseFormat = "yyyyMMdd-HHmmss";

        private const string NA = "N/A";

        private const string AdultEducationBudgetNote =
            "Please note that devolved adult education funding for learners who are funded through the Mayoral Combined Authorities or Greater London Authority is not included here.\nPlease refer to the separate Devolved Adult Education Funding Summary Report.";

        public FundingSummaryModelBuilder(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public FundingSummaryReportModel Build(IReportServiceContext reportServiceContext, IFundingSummaryDataModel fundingSummaryDataModel)
        {
            var models = BuildFundingSummaryReportModel(reportServiceContext, fundingSummaryDataModel);

            return models;
        }

        public FundingSummaryReportModel BuildFundingSummaryReportModel(IReportServiceContext reportServiceContext, IFundingSummaryDataModel fundingSummaryDataModel)
        {
            var noContract = "No Contract";

            var fcsContractAllocationFspCodeLookup = fundingSummaryDataModel.FcsDictionary;
            var periodisedValues = fundingSummaryDataModel.PeriodisedValuesLookup;

            var carryInApprenticeshipBudget = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.APPS2021, noContract);
            var apprenticeshipEmployerOnApprenticeshipServiceLevy = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.LEVY1799, noContract);
            var apprenticeshipEmployersOnApprenticeshipServiceNonLevy = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.NONLEVY2019, noContract);
            var nonLevyContractedApprenticeships1618 = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.C1618NLAP2018, noContract);
            var nonLevyContractedApprenticeshipsAdult = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.ANLAP2018, noContract);
            var traineeships1618 = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.C1618TRN2021, noContract);
            var traineeships1924NonProcured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEBC19TRN2021, noContract);
            var traineeships1924Procured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEB19TRN2021, noContract);
            var traineeships19242020Procurement = string.Empty; // Not known at this time
            var adultEducationBudgetNonProcured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEBCASCL2021, noContract);
            var adultEducationBudgetProcured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEBAS2021, noContract);
            var aebCovid19SkillsOfferNonProcured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEBCCSO2021, noContract);
            var aebCovid19SkillsOfferProcured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEBCSO2021, noContract);

            var advancedLoansBursary =
                fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.ALLB2021)
                ?? fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.ALLBC2021)
                ?? noContract;

            byte reportCurrentPeriod = (byte)reportServiceContext.ReturnPeriod > 12 ? (byte)12 : (byte)reportServiceContext.ReturnPeriod;
            var headerData = BuildHeaderData(reportServiceContext, fundingSummaryDataModel);
            var footerData = BuildFooterData();

            var fundingSummaryReportModel = new FundingSummaryReportModel(
                headerData,
                new List<FundingCategory>()
                {
                    //----------------------------------------------------------------------------------------
                    // Carry-in Apprenticeships Budget (for starts before 1 May 2017 and non-procured delivery
                    //----------------------------------------------------------------------------------------
                    new FundingCategory(@"Carry-in Apprenticeships Budget (for starts before 1 May 2017 and non-procured delivery)", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory(@"16-18 Apprenticeship Frameworks for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("16-18", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.Apprenticeship1618}, periodisedValues))
                                .WithFundLineGroup(BuildEasFm35FundLineGroup("16-18", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.EasApprenticeship1618}, periodisedValues)),

                            new FundingSubCategory(@"16-18 Trailblazer Apprenticeships for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrTrailblazerApprenticeshipsFundLineGroup("16-18", reportCurrentPeriod, new[] {FundLineConstants.TrailblazerApprenticeship1618}, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("16-18", "Trailblazer Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.EasTrailblazerApprenticeship1618}, periodisedValues)),

                            new FundingSubCategory(@"16-18 Non-Levy Contracted Apprenticeships - Non-procured delivery", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrNonLevyApprenticeshipsFundLineGroup("16-18", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeship1618NonProcured }, periodisedValues))
                                .WithFundLineGroup(BuildEasNonLevyApprenticeshipsFundLineGroup("16-18", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeship1618NonProcured}, periodisedValues)),

                            new FundingSubCategory(@"19-23 Apprenticeship Frameworks for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("19-23", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.Apprenticeship1923}, periodisedValues))
                                .WithFundLineGroup(BuildEasFm35FundLineGroup("19-23", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.EasApprenticeship1923}, periodisedValues)),

                            new FundingSubCategory(@"19-23 Trailblazer Apprenticeships for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrTrailblazerApprenticeshipsFundLineGroup("19-23", reportCurrentPeriod, new[] {FundLineConstants.TrailblazerApprenticeship1923}, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("19-23", "Trailblazer Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.EasTrailblazerApprenticeship1923}, periodisedValues)),

                            new FundingSubCategory(@"24+ Apprenticeship Frameworks for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("24+", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.Apprenticeship24Plus}, periodisedValues))
                                .WithFundLineGroup(BuildEasFm35FundLineGroup("24+", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.EasApprenticeship24Plus}, periodisedValues)),

                            new FundingSubCategory(@"24+ Trailblazer Apprenticeships for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrTrailblazerApprenticeshipsFundLineGroup("24+", reportCurrentPeriod, new[] {FundLineConstants.TrailblazerApprenticeship24Plus}, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("24+", "Trailblazer Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.EasTrailblazerApprenticeship24Plus}, periodisedValues)),

                            new FundingSubCategory(@"Adult Non-Levy Contracted Apprenticeships - Non-procured delivery", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrNonLevyApprenticeshipsFundLineGroup("Adult", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeship19PlusNonProcured}, periodisedValues))
                                .WithFundLineGroup(BuildEasNonLevyApprenticeshipsFundLineGroup("Adult", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeship19PlusNonProcured }, periodisedValues))
                        }, carryInApprenticeshipBudget),

                    //-------------------------------------------------------------
                    // Apprenticeships – Employers on Apprenticeship Service - Levy
                    //-------------------------------------------------------------
                    new FundingCategory("Apprenticeships – Employers on Apprenticeship Service - Levy", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("16-18 Apprenticeship (Employer on App Service) Levy", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrApprenticeshipsFundLineGroup("16-18", "Apprenticeship (Employer on App Service) Levy", reportCurrentPeriod, new[] {FundLineConstants.LevyApprenticeship1618}, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("16-18", "Apprenticeship (Employer on App Service) Levy", reportCurrentPeriod, new[] { FundLineConstants.LevyApprenticeship1618 }, periodisedValues)),

                            new FundingSubCategory("Adult Apprenticeship (Employer on App Service) Levy", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrApprenticeshipsFundLineGroup("Adult", "Apprenticeship (Employer on App Service) Levy", reportCurrentPeriod, new[] { FundLineConstants.LevyApprenticeship19Plus }, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("Adult", "Apprenticeship (Employer on App Service) Levy", reportCurrentPeriod, new[] { FundLineConstants.LevyApprenticeship19Plus }, periodisedValues)),
                        }, apprenticeshipEmployerOnApprenticeshipServiceLevy),

                    //-----------------------------------------------------------------
                    // Apprenticeships – Employers on Apprenticeship Service - Non-Levy
                    //-----------------------------------------------------------------
                    new FundingCategory("Apprenticeships – Employers on Apprenticeship Service - Non-Levy", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("16-18 Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrApprenticeshipsFundLineGroup("16-18", "Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeshipEmployerOnAppService1618 }, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("16-18", "Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeshipEmployerOnAppService1618 }, periodisedValues)),

                            new FundingSubCategory("Adult Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrApprenticeshipsFundLineGroup("Adult", "Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeshipEmployerOnAppService19Plus }, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("Adult", "Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeshipEmployerOnAppService19Plus }, periodisedValues)),
                        }, apprenticeshipEmployersOnApprenticeshipServiceNonLevy),

                    //---------------------------------------------------------------------
                    // 16-18 Non-Levy Contracted Apprenticeships Budget - Procured delivery
                    //---------------------------------------------------------------------
                    new FundingCategory("16-18 Non-Levy Contracted Apprenticeships Budget - Procured delivery", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("16-18 Non-Levy Contracted Apprenticeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrNonLevyApprenticeshipsProcuredFundLineGroup("16-18", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeship1618Procured}, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("16-18", "Non-Levy Contracted Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeship1618Procured}, periodisedValues)),
                        }, nonLevyContractedApprenticeships1618),

                    //---------------------------------------------------------------------
                    // Adult Non-Levy Contracted Apprenticeships Budget - Procured delivery
                    //---------------------------------------------------------------------
                    new FundingCategory("Adult Non-Levy Contracted Apprenticeships Budget - Procured delivery", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("Adult Non-Levy Contracted Apprenticeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrNonLevyApprenticeshipsProcuredFundLineGroup("Adult", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeship19PlusProcured}, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("Adult", "Non-Levy Contracted Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeship19PlusProcured}, periodisedValues)),
                        }, nonLevyContractedApprenticeshipsAdult),

                    //---------------------------------------------------------------------
                    // 16-18 Traineeships Budget
                    //---------------------------------------------------------------------
                    new FundingCategory("16-18 Traineeships Budget", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("16-18 Traineeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm25FundLineGroup(reportCurrentPeriod, periodisedValues))
                                .WithFundLineGroup(BuildEasFm25FundLineGroup(reportCurrentPeriod, periodisedValues))
                        }, traineeships1618),

                    //---------------------------------------------------------------------
                    // 19-24 Traineeships - Non-procured delivery
                    //---------------------------------------------------------------------
                    new FundingCategory("19-24 Traineeships - Non-procured delivery", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("19-24 Traineeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new[] { FundLineConstants.Traineeship1924, FundLineConstants.Traineeship1924NonProcured }, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new[] {FundLineConstants.EasTraineeships1924NonProcured}, periodisedValues)),
                        }, traineeships1924NonProcured),

                    //---------------------------------------------------------------------
                    // 19-24 Traineeships - Procured delivery from 1 Nov 2017
                    //---------------------------------------------------------------------
                    new FundingCategory("19-24 Traineeships - Procured delivery from 1 Nov 2017", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("19-24 Traineeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new[] {FundLineConstants.Traineeship1924ProcuredFromNov2017}, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new[] { FundLineConstants.EasTraineeships1924ProcuredFromNov2017 }, periodisedValues)),
                        }, traineeships1924Procured),

                    //---------------------------------------------------------------------
                    // 19-24 Traineeships - 2020 Procurement
                    //---------------------------------------------------------------------
                    new FundingCategory("19-24 Traineeships - 2020 Procurement", reportCurrentPeriod,
                        new List<FundingSubCategory>
                        {
                            new FundingSubCategory("19-24 Traineeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new [] {FundLineConstants.Traineeship19242020Procurement}, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new [] { FundLineConstants.EasTraineeship19242020Procurement}, periodisedValues))
                        }, traineeships19242020Procurement),

                    //---------------------------------------------------------------------
                    // ESFA Adult Education Budget – Non-procured delivery
                    //---------------------------------------------------------------------
                    new FundingCategory("ESFA Adult Education Budget – Non-procured delivery", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("ESFA AEB – Adult Skills (non-procured)", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("ESFA", "AEB - Adult Skills (non-procured)", reportCurrentPeriod, new[] { FundLineConstants.AebOtherLearningNonProcured }, periodisedValues))
                                .WithFundLineGroup(BuildEasAebFundLineGroup("ESFA", "AEB - Adult Skills (non-procured)", reportCurrentPeriod, new[] { FundLineConstants.EasAebAdultSkillsNonProcured }, periodisedValues))
                        }, adultEducationBudgetNonProcured, AdultEducationBudgetNote),

                    //---------------------------------------------------------------------
                    // ESFA Adult Education Budget – Procured delivery from 1 Nov 2017
                    //---------------------------------------------------------------------
                    new FundingCategory("ESFA Adult Education Budget – Procured delivery from 1 Nov 2017 ", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("ESFA AEB – Adult Skills (procured from Nov 2017)", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("ESFA", "AEB - Adult Skills (procured from Nov 2017)", reportCurrentPeriod, new[] { FundLineConstants.AebOtherLearningProcuredFromNov2017 }, periodisedValues))
                                .WithFundLineGroup(BuildEasAebFundLineGroup("ESFA", "AEB - Adult Skills (procured from Nov 2017)", reportCurrentPeriod, new[] { FundLineConstants.EasAebAdultSkillsProcuredFromNov2017 }, periodisedValues))
                        }, adultEducationBudgetProcured, AdultEducationBudgetNote),

                    //---------------------------------------------------------------------
                    // ESFA AEB COVID-19 Skills Offer - Non Procured
                    //---------------------------------------------------------------------
                    new FundingCategory("ESFA AEB – COVID-19 Skills Offer – Non-procured delivery", reportCurrentPeriod,
                        new List<FundingSubCategory>
                        {
                            new FundingSubCategory("ESFA AEB – COVID-19 Skills Offer (non-procured)", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("ESFA", "AEB – COVID-19 Skills Offer (non-procured)", reportCurrentPeriod, new [] { FundLineConstants.AebCovidSkillsOfferNonProcured }, periodisedValues))
                                .WithFundLineGroup(BuildEasAebCovidFundLineGroup("ESFA", "AEB – COVID-19 Skills Offer (non-procured)", reportCurrentPeriod, new [] { FundLineConstants.EasAebCovidSkillsOfferNonProcured }, periodisedValues))
                        },aebCovid19SkillsOfferNonProcured),

                    //---------------------------------------------------------------------
                    // ESFA AEB COVID-19 Skills Offer - Procured
                    //---------------------------------------------------------------------
                    new FundingCategory("ESFA AEB – COVID-19 Skills Offer – Procured delivery", reportCurrentPeriod,
                        new List<FundingSubCategory>
                        {
                            new FundingSubCategory("ESFA AEB – COVID-19 Skills Offer (procured)", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("ESFA", "AEB – COVID-19 Skills Offer (procured)", reportCurrentPeriod, new [] { FundLineConstants.AebCovidSkillsOfferProcured }, periodisedValues))
                                .WithFundLineGroup(BuildEasAebCovidFundLineGroup("ESFA", "AEB – COVID-19 Skills Offer (procured)", reportCurrentPeriod, new [] { FundLineConstants.EasAebCovidSkillsOfferProcured }, periodisedValues))
                        },aebCovid19SkillsOfferProcured),

                    //---------------------------------------------------------------------
                    // Advanced Loans Bursary Budget
                    //---------------------------------------------------------------------
                    new FundingCategory("Advanced Loans Bursary Budget", reportCurrentPeriod,
                        new List<FundingSubCategory>()
                        {
                            new FundingSubCategory("Advanced Loans Bursary", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm99FundLineGroup(reportCurrentPeriod, periodisedValues))
                                .WithFundLineGroup(BuildEasFm99FundLineGroup(reportCurrentPeriod, periodisedValues))
                        }, advancedLoansBursary)
                },
                footerData);

            return fundingSummaryReportModel;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Apprenticeship Frameworks FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildIlrFm35FundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"ILR Total {ageRange} {description} (£)", currentPeriod, FundingDataSource.FM35, fundLines, periodisedValues)
                .WithFundLine($"ILR {ageRange} {description} Programme Funding (£)", new[] { AttributeConstants.Fm35OnProgPayment, AttributeConstants.Fm35AchievePayment, AttributeConstants.Fm35BalancePayment, AttributeConstants.Fm35EmpOutcomePay })
                .WithFundLine($"ILR {ageRange} {description} Learning Support (£)", new[] { AttributeConstants.Fm35LearnSuppFundCash });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS Apprenticeship Frameworks FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildEasFm35FundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, fundLines, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaims })
                .WithFundLine($"EAS {ageRange} {description} Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport })
                .WithFundLine($"EAS {ageRange} {description} Learner Support (£)", new[] { AttributeConstants.EasLearnerSupport });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Trailblazer Apprenticeships FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildIlrTrailblazerApprenticeshipsFundLineGroup(string ageRange, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Trailblazer Apprenticeships";

            var fundLineGroup = new FundLineGroup($"ILR Total {ageRange} {description} (£)", currentPeriod, FundingDataSource.FM81, fundLines, periodisedValues)
                .WithFundLine($"ILR {ageRange} {description} Programme Funding (Core Government Contribution, Maths and English) (£)", new[] { AttributeConstants.Fm81CoreGovContPayment, AttributeConstants.Fm81MathEngBalPayment, AttributeConstants.Fm81MathEngOnProgPayment })
                .WithFundLine($"ILR {ageRange} {description} Employer Incentive Payments (Achievement, Small Employer, 16-18) (£)", new[] { AttributeConstants.Fm81AchPayment, AttributeConstants.Fm81SmallBusPayment, AttributeConstants.Fm81YoungAppPayment })
                .WithFundLine($"ILR {ageRange} {description} Learning Support (£)", new[] { AttributeConstants.Fm81LearnSuppFundCash });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS Trailblazer Apprenticeships FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, fundLines, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaims })
                .WithFundLine($"EAS {ageRange} {description} Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Employers On Apprenticeship Service - FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildIlrApprenticeshipsFundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"ILR Total {ageRange} {description} (£)", currentPeriod, FundingDataSource.DAS, fundLines, periodisedValues)
                .WithFundLine($"ILR {ageRange} {description} Programme Aim Programme Funding - Levy Funding (£)", new[] { Constants.DASPayments.FundingSource.Levy, Constants.DASPayments.FundingSource.LevyTransfer }, new[] { Constants.DASPayments.TransactionType.Learning_On_Programme, Constants.DASPayments.TransactionType.Completion, Constants.DASPayments.TransactionType.Balancing })
                .WithFundLine($"ILR {ageRange} {description} Programme Aim Programme Funding - Government Co-Investment (£)", new[] { Constants.DASPayments.FundingSource.Co_Invested_SFA }, new[] { Constants.DASPayments.TransactionType.Learning_On_Programme, Constants.DASPayments.TransactionType.Completion, Constants.DASPayments.TransactionType.Balancing })
                .WithFundLine($"ILR {ageRange} {description} Maths & English Programme Funding (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.On_Programme_Maths_and_English, Constants.DASPayments.TransactionType.BalancingMathAndEnglish })
                .WithFundLine($"ILR {ageRange} {description} Framework Uplift (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.On_Programme_16To18_Framework_Uplift, Constants.DASPayments.TransactionType.Completion_16To18_Framework_Uplift, Constants.DASPayments.TransactionType.Balancing_16To18_Framework_Uplift })
                .WithFundLine($"ILR {ageRange} {description} Disadvantage Payments (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_Disadvantage_Payment, Constants.DASPayments.TransactionType.Second_Disadvantage_Payment })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Providers (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive, Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Employers (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive, Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Apprentices (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.Apprenticeship })
                .WithFundLine($"ILR {ageRange} {description} Learning Support (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.Learning_Support });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS Employers On Apprenticeship Service - Levy FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public virtual FundLineGroup BuildEasLevyApprenticeshipsFundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EASDAS, fundLines, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} - Training exc. Maths/English Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsTraining })
                .WithFundLine($"EAS {ageRange} {description} - Maths and English Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsMathsAndEnglish })
                .WithFundLine($"EAS {ageRange} {description} - Additional Payments for Providers Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsProvider })
                .WithFundLine($"EAS {ageRange} {description} - Additional Payments for Employers Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsEmployer })
                .WithFundLine($"EAS {ageRange} {description} - Additional Payments for Apprentices Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsApprentice })
                .WithFundLine($"EAS {ageRange} {description} - Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Employers On Apprenticeship Service - Unresolved Data Locks FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildIlrApprenticeshipsUnresolvedDataLocksFundLineGroup(byte currentPeriod, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Apprenticeship (Employer on App Service) Unresolved Data Locks";

            var fundLineGroup = new FundLineGroup($"Total {description} (£)", currentPeriod, FundingDataSource.DAS, null, periodisedValues)
                .WithFundLine($"ILR 16-18 {description} (£)", new[] { FundLineConstants.ApprenticeshipEmployerOnAppServiceUnresolvedDataLock1618 }, new[] { Constants.DASPayments.FundingSource.Levy, Constants.DASPayments.FundingSource.LevyTransfer }, Constants.DASPayments.TransactionType.All)
                .WithFundLine($"ILR Adult {description} (£)", new[] { FundLineConstants.ApprenticeshipEmployerOnAppServiceUnresolvedDataLock19Plus }, new[] { Constants.DASPayments.FundingSource.Levy, Constants.DASPayments.FundingSource.LevyTransfer }, Constants.DASPayments.TransactionType.All);

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Non-Levy Contracted Apprenticeships FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildIlrNonLevyApprenticeshipsFundLineGroup(string ageRange, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Non-Levy Contracted Apprenticeships";

            var fundLineGroup = new FundLineGroup($"ILR Total {ageRange} {description} (£)", currentPeriod, FundingDataSource.DAS, fundLines, periodisedValues)
                .WithFundLine($"ILR {ageRange} {description} Programme Aim Funding - Government Co-investment (£)", new[] { Constants.DASPayments.FundingSource.Co_Invested_SFA }, new[] { Constants.DASPayments.TransactionType.Learning_On_Programme, Constants.DASPayments.TransactionType.Completion, Constants.DASPayments.TransactionType.Balancing })
                .WithFundLine($"ILR {ageRange} {description} Maths & English Programme Funding (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.On_Programme_Maths_and_English, Constants.DASPayments.TransactionType.BalancingMathAndEnglish })
                .WithFundLine($"ILR {ageRange} {description} Framework Uplift (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.On_Programme_16To18_Framework_Uplift, Constants.DASPayments.TransactionType.Completion_16To18_Framework_Uplift, Constants.DASPayments.TransactionType.Balancing_16To18_Framework_Uplift })
                .WithFundLine($"ILR {ageRange} {description} Disadvantage Payments (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_Disadvantage_Payment, Constants.DASPayments.TransactionType.Second_Disadvantage_Payment })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Providers (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive, Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Employers (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive, Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive })
                .WithFundLine($"ILR {ageRange} {description} Learning Support (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.Learning_Support });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS Non-Levy Contracted Apprenticeships FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildEasNonLevyApprenticeshipsFundLineGroup(string ageRange, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Non-Levy Contracted Apprenticeships";

            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EASDAS, fundLines, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} Training exc. Maths/English Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsTraining })
                .WithFundLine($"EAS {ageRange} {description} Maths and English Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsMathsAndEnglish })
                .WithFundLine($"EAS {ageRange} {description} Additional Payments for Providers Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsProvider })
                .WithFundLine($"EAS {ageRange} {description} Additional Payments for Employers Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsEmployer })
                .WithFundLine($"EAS {ageRange} {description} Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Non-Levy Contracted Apprenticeships FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildIlrNonLevyApprenticeshipsProcuredFundLineGroup(string ageRange, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Non-Levy Contracted Apprenticeships";

            var fundLineGroup = new FundLineGroup($"ILR Total {ageRange} {description} (£)", currentPeriod, FundingDataSource.DAS, fundLines, periodisedValues)
                .WithFundLine($"ILR {ageRange} {description} Programme Aim Funding - Government Co-investment (£)", new[] { Constants.DASPayments.FundingSource.Co_Invested_SFA }, new[] { Constants.DASPayments.TransactionType.Learning_On_Programme, Constants.DASPayments.TransactionType.Completion, Constants.DASPayments.TransactionType.Balancing })
                .WithFundLine($"ILR {ageRange} {description} Maths & English Programme Funding (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.On_Programme_Maths_and_English, Constants.DASPayments.TransactionType.BalancingMathAndEnglish })
                .WithFundLine($"ILR {ageRange} {description} Framework Uplift (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.On_Programme_16To18_Framework_Uplift, Constants.DASPayments.TransactionType.Completion_16To18_Framework_Uplift, Constants.DASPayments.TransactionType.Balancing_16To18_Framework_Uplift })
                .WithFundLine($"ILR {ageRange} {description} Disadvantage Payments (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_Disadvantage_Payment, Constants.DASPayments.TransactionType.Second_Disadvantage_Payment })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Providers (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive, Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Employers (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive, Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Apprentices (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.Apprenticeship })
                .WithFundLine($"ILR {ageRange} {description} Learning Support (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.Learning_Support });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Traineeships FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildIlrFm25FundLineGroup(byte currentPeriod, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"ILR Total 16-18 Traineeships (£)", currentPeriod, FundingDataSource.FM25, null, periodisedValues)
                .WithFundLine($"ILR 16-18 Traineeships Programme Funding (£)", new[] { FundLineConstants.TraineeshipsAdultFunded1618 }, new[] { AttributeConstants.Fm25LrnOnProgPay })
                .WithFundLine($"ILR 19-24 Traineeships (16-19 Model) Programme Funding (£)", new[] { FundLineConstants.TraineeshipsAdultFunded19Plus }, new[] { AttributeConstants.Fm25LrnOnProgPay });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS Traineeships Budget FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildEasFm25FundLineGroup(byte currentPeriod, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Traineeships";

            var fundLineGroup = new FundLineGroup($"EAS Total 16-18 {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, new[] { FundLineConstants.Traineeships1618 }, periodisedValues)
                .WithFundLine($"EAS 16-18 {description} Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaims })
                .WithFundLine($"EAS 16-18 {description} Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport })
                .WithFundLine($"EAS 16-19 {description} Vulnerable Bursary (£)", new[] { AttributeConstants.EasVulnerableBursary })
                .WithFundLine($"EAS 16-19 {description} Free Meals (£)", new[] { AttributeConstants.EasFreeMeals })
                .WithFundLine($"EAS 16-19 {description} Discretionary Bursary (£)", new[] { AttributeConstants.EasDiscretionaryBursary });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS ESFA Adult Education Budget - Non-Procured Delivery From 1 Nov 2017 FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildEasAebFundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundModels, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, fundModels, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaims })
                .WithFundLine($"EAS {ageRange} {description} Prince's Trust (£)", new[] { AttributeConstants.EasPrincesTrust })
                .WithFundLine($"EAS {ageRange} {description} Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS ESFA Adult Education Budget Covid Skills - Non-Procured Delivery From 1 Nov 2017 FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildEasAebCovidFundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundModels, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, fundModels, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaims })
                .WithFundLine($"EAS {ageRange} {description} Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Advanced Loans Bursary Budget FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildIlrFm99FundLineGroup(byte currentPeriod, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Advanced Loans Bursary";

            var fundLineGroup = new FundLineGroup($"ILR Total {description} (£)", currentPeriod, FundingDataSource.FM99, new[] { FundLineConstants.AdvancedLearnerLoansBursary }, periodisedValues)
                .WithFundLine($"ILR {description} Funding (£)", new[] { AttributeConstants.Fm99AlbSupportPayment })
                .WithFundLine($"ILR {description} Area Costs (£)", new[] { AttributeConstants.Fm99AreaUpliftBalPayment, AttributeConstants.Fm99AreaUpliftOnProgPayment });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS Advanced Loans Bursary Budget FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public FundLineGroup BuildEasFm99FundLineGroup(byte currentPeriod,
            IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Advanced Loans Bursary";

            var fundLineGroup = new FundLineGroup($"EAS Total {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, new[] { FundLineConstants.AdvancedLearnerLoansBursary }, periodisedValues)
                .WithFundLine($"EAS {description} Excess Support (£)", new[] { AttributeConstants.EasAllbExcessSupport })
                .WithFundLine($"EAS {description} Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaims });

            return fundLineGroup;
        }

        private IDictionary<string, string> BuildHeaderData(IReportServiceContext reportServiceContext, IFundingSummaryDataModel fundingSummaryDataModel)
        {
            var organisationName = fundingSummaryDataModel.OrganisationName;
            var easFileName = ExtractFileName(fundingSummaryDataModel.EasFileName);
            var ilrFileName = ExtractFileName(fundingSummaryDataModel.IlrFileName);
            var easLastUpdate = fundingSummaryDataModel.LastEasUpdate;
            var ilrSubmittedDateTime = _dateTimeProvider.ConvertUtcToUk(fundingSummaryDataModel.IlrSubmittedDateTime);

            string easLastUpdateUk = NA;

            if (easLastUpdate != null)
            {
                easLastUpdateUk = _dateTimeProvider.ConvertUtcToUk(easLastUpdate.Value).LongDateStringFormat();
            }

            return new Dictionary<string, string>()
            {
                {SummaryPageConstants.ProviderName, organisationName},
                {SummaryPageConstants.UKPRN, reportServiceContext.Ukprn.ToString()},
                {SummaryPageConstants.ILRFile, ilrFileName},
                {SummaryPageConstants.LastILRFileUpdate, ilrSubmittedDateTime.ToString(lastSubmittedIlrFileDateStringFormat)},
                {SummaryPageConstants.EASFile, easFileName },
                {SummaryPageConstants.LastEASUpdate, easLastUpdateUk},
                {SummaryPageConstants.SecurityClassification, SummaryPageConstants.OfficialSensitive}
            };
        }

        private IDictionary<string, string> BuildFooterData()
        {
            DateTime dateTimeNowUtc = _dateTimeProvider.GetNowUtc();
            DateTime dateTimeNowUk = _dateTimeProvider.ConvertUtcToUk(dateTimeNowUtc);

            var reportGeneratedAt = dateTimeNowUk.TimeOfDayOnDateStringFormat();

            return new Dictionary<string, string>()
            {
                { SummaryPageConstants.ReportGeneratedAt, reportGeneratedAt }
            };
        }

        private string ExtractFileName(string fileName)
        {
            var parts = fileName?.Split('/') ?? new string[]{};
            if (parts.Any())
            {
                var returnFileName = parts[parts.Length - 1];

                return returnFileName;
            }

            return NA;
        }
    }
}
