using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;
using ESFA.DC.PeriodEnd.ReportService.Service.Interface;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport
{
    public class FundingSummaryReportModelBuilder : IFundingSummaryReportModelBuilder
    {
        private string AdultEducationBudgetNote =
            "Please note that devolved adult education funding for learners who are funded through the Mayoral Combined Authorities or Greater London Authority is not included here.\nPlease refer to the separate Devolved Adult Education Funding Summary Report.";

        public FundingSummaryReportModel BuildFundingSummaryReportModel(IReportServiceContext reportServiceContext, IPeriodisedValuesLookup periodisedValues, IDictionary<string, string> fcsContractAllocationFspCodeLookup)
        {
            var carryInApprenticeshipBudget = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.APPS1920);
            var apprenticeshipEmployerOnApprenticeshipServiceLevy = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.LEVY1799);
            var apprenticeshipEmployersOnApprenticeshipServiceNonLevy = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.NONLEVY2019);
            var nonFunded = "Non-funded";
            var nonLevyContractedApprenticeships1618 = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.C1618NLAP2018);
            var nonLevyContractedApprenticeshipsAdult = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.ANLAP2018);
            var traineeships1618 = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.C1618TRN1920);
            var traineeships1924NonProcured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEBC19TRN1920);
            var traineeships1924Procured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEB19TRN1920);
            var adultEducationBudgetNonProcured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEBCASCL1920);
            var adultEducationBudgetProcured = fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.AEBAS1920);

            var advancedLoansBursary =
                fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.ALLB1920)
                ?? fcsContractAllocationFspCodeLookup.GetValueOrDefault(FundingStreamPeriodCodeConstants.ALLBC1920);

            byte reportCurrentPeriod = (byte)reportServiceContext.ReturnPeriod > 12 ? (byte)12 : (byte)reportServiceContext.ReturnPeriod;

            var fundingSummaryReportModel = new FundingSummaryReportModel(
                new List<IFundingCategory>()
                {
                    //----------------------------------------------------------------------------------------
                    // Carry-in Apprenticeships Budget (for starts before 1 May 2017 and non-procured delivery
                    //----------------------------------------------------------------------------------------
                    new FundingCategory(@"Carry-in Apprenticeships Budget (for starts before 1 May 2017 and non-procured delivery)", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory(@"16-18 Apprenticeship Frameworks for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("16-18", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.Apprenticeship1618}, periodisedValues))
                                .WithFundLineGroup(BuildEasFm35FundLineGroup("16-18", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.Apprenticeship1618}, periodisedValues)),

                            new FundingSubCategory(@"16-18 Trailblazer Apprenticeships for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrTrailblazerApprenticeshipsFundLineGroup("16-18", reportCurrentPeriod, new[] {FundLineConstants.TrailblazerApprenticeship1618}, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("16-18", "Trailblazer Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.TrailblazerApprenticeship1618}, periodisedValues)),

                            new FundingSubCategory(@"16-18 Non-Levy Contracted Apprenticeships - Non-procured delivery", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrNonLevyApprenticeshipsFundLineGroup("16-18", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeship1618, FundLineConstants.NonLevyApprenticeship1618NonProcured }, periodisedValues))
                                .WithFundLineGroup(BuildEasNonLevyApprenticeshipsFundLineGroup("16-18", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeship1618NonProcured}, periodisedValues)),

                            new FundingSubCategory(@"19-23 Apprenticeship Frameworks for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("19-23", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.Apprenticeship1923}, periodisedValues))
                                .WithFundLineGroup(BuildEasFm35FundLineGroup("19-23", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.Apprenticeship1923}, periodisedValues)),

                            new FundingSubCategory(@"19-23 Trailblazer Apprenticeships for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrTrailblazerApprenticeshipsFundLineGroup("19-23", reportCurrentPeriod, new[] {FundLineConstants.TrailblazerApprenticeship1923}, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("19-23", "Trailblazer Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.TrailblazerApprenticeship1923}, periodisedValues)),

                            new FundingSubCategory(@"24+ Apprenticeship Frameworks for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("24+", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.Apprenticeship24Plus}, periodisedValues))
                                .WithFundLineGroup(BuildEasFm35FundLineGroup("24+", "Apprenticeship Frameworks", reportCurrentPeriod, new[] {FundLineConstants.Apprenticeship24Plus}, periodisedValues)),

                            new FundingSubCategory(@"24+ Trailblazer Apprenticeships for starts before 1 May 2017", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrTrailblazerApprenticeshipsFundLineGroup("24+", reportCurrentPeriod, new[] {FundLineConstants.TrailblazerApprenticeship24Plus}, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("24+", "Trailblazer Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.TrailblazerApprenticeship24Plus}, periodisedValues)),

                            new FundingSubCategory(@"Adult Non-Levy Contracted Apprenticeships - Non-procured delivery", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrNonLevyApprenticeshipsFundLineGroup("Adult", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeship19Plus, FundLineConstants.NonLevyApprenticeship19PlusNonProcured}, periodisedValues))
                                .WithFundLineGroup(BuildEasNonLevyApprenticeshipsFundLineGroup("Adult", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeship19PlusNonProcured }, periodisedValues))
                        }, carryInApprenticeshipBudget),

                    //-------------------------------------------------------------
                    // Apprenticeships – Employers on Apprenticeship Service - Levy
                    //-------------------------------------------------------------
                    new FundingCategory("Apprenticeships – Employers on Apprenticeship Service - Levy", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("16-18 Apprenticeship (Employer on App Service) Levy", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrApprenticeshipsFundLineGroup("16-18", "Apprenticeship (Employer on App Service) Levy", reportCurrentPeriod, new[] {FundLineConstants.LevyApprenticeship1618}, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("16-18", "Apprenticeship (Employer on App Service) Levy", reportCurrentPeriod, new[] { FundLineConstants.LevyApprenticeship1618 }, periodisedValues)),

                            new FundingSubCategory("Adult Apprenticeship (Employer on App Service)", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrApprenticeshipsFundLineGroup("Adult", "Apprenticeship (Employer on App Service)", reportCurrentPeriod, new[] { FundLineConstants.LevyApprenticeship19Plus }, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("Adult", "Apprenticeship (Employer on App Service)", reportCurrentPeriod, new[] { FundLineConstants.LevyApprenticeship19Plus }, periodisedValues)),
                        }, apprenticeshipEmployerOnApprenticeshipServiceLevy),

                    //-----------------------------------------------------------------
                    // Apprenticeships – Employers on Apprenticeship Service - Non-Levy
                    //-----------------------------------------------------------------
                    new FundingCategory("Apprenticeships – Employers on Apprenticeship Service - Non-Levy", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("16-18 Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrApprenticeshipsFundLineGroup("16-18", "Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeshipEmployerOnAppService1618 }, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("16-18", "Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeshipEmployerOnAppService1618 }, periodisedValues)),

                            new FundingSubCategory("Adult Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrApprenticeshipsFundLineGroup("Adult", "Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeshipEmployerOnAppService19Plus }, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("Adult", "Apprenticeship (Employer on App Service) Non-Levy", reportCurrentPeriod, new[] { FundLineConstants.NonLevyApprenticeshipEmployerOnAppService19Plus }, periodisedValues)),
                        }, apprenticeshipEmployersOnApprenticeshipServiceNonLevy),

                    //------------------------------------------------------------------------------
                    // Apprenticeships – Employers on Apprenticeship Service - Unresolved Data Locks
                    //------------------------------------------------------------------------------
                    new FundingCategory("Apprenticeships – Employers on Apprenticeship Service - Unresolved Data Locks", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("Apprenticeship (Employer on App Service) Unresolved Data Locks", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrApprenticeshipsUnresolvedDataLocksFundLineGroup(reportCurrentPeriod, periodisedValues)),
                        },
                        nonFunded,
                        "Please note that funding with unresolved data locks will NOT be paid.  These figures represent total funding; co-investment is not taken into account."),

                    //---------------------------------------------------------------------
                    // 16-18 Non-Levy Contracted Apprenticeships Budget - Procured delivery
                    //---------------------------------------------------------------------
                    new FundingCategory("16-18 Non-Levy Contracted Apprenticeships Budget - Procured delivery", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("16-18 Non-Levy Contracted Apprenticeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrNonLevyApprenticeshipsProcuredFundLineGroup("16-18", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeship1618Procured}, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("16-18", "Non-Levy Contracted Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeship1618Procured}, periodisedValues)),
                        }, nonLevyContractedApprenticeships1618),

                    //---------------------------------------------------------------------
                    // Adult Non-Levy Contracted Apprenticeships Budget - Procured delivery
                    //---------------------------------------------------------------------
                    new FundingCategory("Adult Non-Levy Contracted Apprenticeships Budget - Procured delivery", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("Adult Non-Levy Contracted Apprenticeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrNonLevyApprenticeshipsProcuredFundLineGroup("Adult", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeship19PlusProcured}, periodisedValues))
                                .WithFundLineGroup(BuildEasLevyApprenticeshipsFundLineGroup("Adult", "Non-Levy Contracted Apprenticeships", reportCurrentPeriod, new[] {FundLineConstants.NonLevyApprenticeship19PlusProcured}, periodisedValues)),
                        }, nonLevyContractedApprenticeshipsAdult),

                    //---------------------------------------------------------------------
                    // 16-18 Traineeships Budget
                    //---------------------------------------------------------------------
                    new FundingCategory("16-18 Traineeships Budget", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("16-18 Traineeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm25FundLineGroup(reportCurrentPeriod, periodisedValues))
                                .WithFundLineGroup(BuildEasFm25FundLineGroup(reportCurrentPeriod, periodisedValues))
                        }, traineeships1618),

                    //---------------------------------------------------------------------
                    // 19-24 Traineeships - Non-procured delivery
                    //---------------------------------------------------------------------
                    new FundingCategory("19-24 Traineeships - Non-procured delivery", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("19-24 Traineeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new[] { FundLineConstants.Traineeship1924, FundLineConstants.Traineeship1924NonProcured }, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new[] {FundLineConstants.EasTraineeships1924NonProcured}, periodisedValues)),
                        }, traineeships1924NonProcured),

                    //---------------------------------------------------------------------
                    // 19-24 Traineeships - Procured delivery from 1 Nov 2017
                    //---------------------------------------------------------------------
                    new FundingCategory("19-24 Traineeships - Procured delivery from 1 Nov 2017", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("19-24 Traineeships", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new[] {FundLineConstants.Traineeship1924ProcuredFromNov2017}, periodisedValues))
                                .WithFundLineGroup(BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup("19-24", "Traineeships", reportCurrentPeriod, new[] {FundLineConstants.EasTraineeships1924NonProcured}, periodisedValues)),
                        }, traineeships1924Procured),

                    //---------------------------------------------------------------------
                    // ESFA Adult Education Budget – Non-procured delivery
                    //---------------------------------------------------------------------
                    new FundingCategory("ESFA Adult Education Budget – Non-procured delivery", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("ESFA AEB – Adult Skills (non-procured)", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("ESFA", "AEB - Adult Skills (non-procured)", reportCurrentPeriod, new[] { FundLineConstants.AebOtherLearning, FundLineConstants.AebOtherLearningNonProcured }, periodisedValues))
                                .WithFundLineGroup(BuildEasAebFundLineGroup("ESFA", "AEB - Adult Skills (non-procured)", reportCurrentPeriod, new[] {FundLineConstants.EasAebAdultSkillsNonProcured}, periodisedValues))
                        }, adultEducationBudgetNonProcured, AdultEducationBudgetNote),

                    //---------------------------------------------------------------------
                    // ESFA Adult Education Budget – Procured delivery from 1 Nov 2017
                    //---------------------------------------------------------------------
                    new FundingCategory("ESFA Adult Education Budget – Procured delivery from 1 Nov 2017 ", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("ESFA AEB – Adult Skills (procured from Nov 2017)", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm35FundLineGroup("ESFA", "AEB - Adult Skills (procured from Nov 2017)", reportCurrentPeriod, new[] {FundLineConstants.AebOtherLearningProcuredFromNov2017}, periodisedValues))
                                .WithFundLineGroup(BuildEasAebFundLineGroup("ESFA", "AEB - Adult Skills (procured from Nov 2017)", reportCurrentPeriod, new[] {FundLineConstants.EasAebAdultSkillsProcuredFromNov2017}, periodisedValues))
                        }, adultEducationBudgetProcured, AdultEducationBudgetNote),

                    //---------------------------------------------------------------------
                    // Advanced Loans Bursary Budget
                    //---------------------------------------------------------------------
                    new FundingCategory("Advanced Loans Bursary Budget", reportCurrentPeriod,
                        new List<IFundingSubCategory>()
                        {
                            new FundingSubCategory("Advanced Loans Bursary", reportCurrentPeriod)
                                .WithFundLineGroup(BuildIlrFm99FundLineGroup(reportCurrentPeriod, periodisedValues))
                                .WithFundLineGroup(BuildEasFm99FundLineGroup(reportCurrentPeriod, periodisedValues))
                        }, advancedLoansBursary)
                });

            return fundingSummaryReportModel;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Apprenticeship Frameworks FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public IFundLineGroup BuildIlrFm35FundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"ILR Total {ageRange} {description} (£)", currentPeriod, FundingDataSource.FM35, fundLines, periodisedValues)
                .WithFundLine($"ILR {ageRange} {description} Programme Funding (£)", new[] { AttributeConstants.Fm35OnProgPayment, AttributeConstants.Fm35AchievePayment, AttributeConstants.Fm35BalancePayment, AttributeConstants.Fm35EmpOutcomePay })
                .WithFundLine($"ILR {ageRange} {description} Learning Support (£)", new[] { AttributeConstants.Fm35LearnSuppFundCash });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS Apprenticeship Frameworks FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public IFundLineGroup BuildEasFm35FundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, fundLines, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} Authorised Claims (£)", new[] {AttributeConstants.EasAuthorisedClaims})
                .WithFundLine($"EAS {ageRange} {description} Excess Learning Support (£)", new[] {AttributeConstants.EasExcessLearningSupport})
                .WithFundLine($"EAS {ageRange} {description} Learner Support (£)", new[] {AttributeConstants.EasLearnerSupport});

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Trailblazer Apprenticeships FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public IFundLineGroup BuildIlrTrailblazerApprenticeshipsFundLineGroup(string ageRange, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
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
        public IFundLineGroup BuildEasAuthorisedClaimsExcessLearningSupportFundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, fundLines, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} Authorised Claims (£)", new[] {AttributeConstants.EasAuthorisedClaims})
                .WithFundLine($"EAS {ageRange} {description} Excess Learning Support (£)", new[] {AttributeConstants.EasExcessLearningSupport});

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Employers On Apprenticeship Service - FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public IFundLineGroup BuildIlrApprenticeshipsFundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
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
        public virtual IFundLineGroup BuildEasLevyApprenticeshipsFundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, fundLines, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} - Training Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsTraining })
                .WithFundLine($"EAS {ageRange} {description} - Additional Payments for Providers Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsProvider })
                .WithFundLine($"EAS {ageRange} {description} - Additional Payments for Employers Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsEmployer })
                .WithFundLine($"EAS {ageRange} {description} - Additional Payments for Apprentices Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsApprentice })
                .WithFundLine($"EAS {ageRange} {description} - Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Employers On Apprenticeship Service - Unresolved Data Locks FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public IFundLineGroup BuildIlrApprenticeshipsUnresolvedDataLocksFundLineGroup(byte currentPeriod, IPeriodisedValuesLookup periodisedValues)
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
        public IFundLineGroup BuildIlrNonLevyApprenticeshipsFundLineGroup(string ageRange, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Non-Levy Contracted Apprenticeships";

            var fundLineGroup = new FundLineGroup($"ILR Total {ageRange} {description} (£)", currentPeriod, FundingDataSource.DAS, fundLines, periodisedValues)
                .WithFundLine($"ILR {ageRange} {description} Programme Aim Funding - Government Co-investment (£)", new [] { Constants.DASPayments.FundingSource.Co_Invested_SFA }, new [] { Constants.DASPayments.TransactionType.Learning_On_Programme, Constants.DASPayments.TransactionType.Completion, Constants.DASPayments.TransactionType.Balancing })
                .WithFundLine($"ILR {ageRange} {description} Maths & English Programme Funding (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new [] { Constants.DASPayments.TransactionType.On_Programme_Maths_and_English, Constants.DASPayments.TransactionType.BalancingMathAndEnglish })
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
        public IFundLineGroup BuildEasNonLevyApprenticeshipsFundLineGroup(string ageRange, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Non-Levy Contracted Apprenticeships";

            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, fundLines, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} Training Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsTraining })
                .WithFundLine($"EAS {ageRange} {description} Additional Payments for Providers Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsProvider })
                .WithFundLine($"EAS {ageRange} {description} Additional Payments for Employers Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaimsEmployer })
                .WithFundLine($"EAS {ageRange} {description} Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Non-Levy Contracted Apprenticeships FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public IFundLineGroup BuildIlrNonLevyApprenticeshipsProcuredFundLineGroup(string ageRange, byte currentPeriod, IEnumerable<string> fundLines, IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Non-Levy Contracted Apprenticeships";

            var fundLineGroup = new FundLineGroup($"ILR Total {ageRange} {description} (£)", currentPeriod, FundingDataSource.DAS, fundLines, periodisedValues)
                .WithFundLine($"ILR {ageRange} {description} Programme Aim Funding - Government Co-investment (£)", new[] { Constants.DASPayments.FundingSource.Co_Invested_SFA }, new[] { Constants.DASPayments.TransactionType.Learning_On_Programme, Constants.DASPayments.TransactionType.Completion, Constants.DASPayments.TransactionType.Balancing })
                .WithFundLine($"ILR {ageRange} {description} Maths & English Programme Funding (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.On_Programme_Maths_and_English, Constants.DASPayments.TransactionType.BalancingMathAndEnglish })
                .WithFundLine($"ILR {ageRange} {description} Framework Uplift (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.On_Programme_16To18_Framework_Uplift, Constants.DASPayments.TransactionType.Completion_16To18_Framework_Uplift, Constants.DASPayments.TransactionType.Balancing_16To18_Framework_Uplift })
                .WithFundLine($"ILR {ageRange} {description} Disadvantage Payments (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_Disadvantage_Payment, Constants.DASPayments.TransactionType.Second_Disadvantage_Payment })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Providers (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_16To18_Provider_Incentive, Constants.DASPayments.TransactionType.Second_16To18_Provider_Incentive })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Employers (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.First_16To18_Employer_Incentive, Constants.DASPayments.TransactionType.Second_16To18_Employer_Incentive })
                .WithFundLine($"ILR {ageRange} {description} Additional Payments for Apprenticeships (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.Apprenticeship })
                .WithFundLine($"ILR {ageRange} {description} Learning Support (£)", new[] { Constants.DASPayments.FundingSource.Fully_Funded_SFA }, new[] { Constants.DASPayments.TransactionType.Learning_Support });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Traineeships FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public IFundLineGroup BuildIlrFm25FundLineGroup(byte currentPeriod, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"ILR Total 16-18 Traineeships (£)", currentPeriod, FundingDataSource.FM25, null, periodisedValues)
                .WithFundLine($"ILR 16-18 Traineeships Programme Funding (£)", new[] { FundLineConstants.TraineeshipsAdultFunded1618 }, new[] { AttributeConstants.Fm25LrnOnProgPay })
                .WithFundLine($"ILR 19-24 Traineeships (16-19 Model) Programme Funding (£)", new[] { FundLineConstants.TraineeshipsAdultFunded19Plus }, new[] { AttributeConstants.Fm25LrnOnProgPay });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build EAS Traineeships Budget FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public IFundLineGroup BuildEasFm25FundLineGroup(byte currentPeriod, IPeriodisedValuesLookup periodisedValues)
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
        public IFundLineGroup BuildEasAebFundLineGroup(string ageRange, string description, byte currentPeriod, IEnumerable<string> fundModels, IPeriodisedValuesLookup periodisedValues)
        {
            var fundLineGroup = new FundLineGroup($"EAS Total {ageRange} {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, fundModels, periodisedValues)
                .WithFundLine($"EAS {ageRange} {description} Authorised Claims (£)", new[] { AttributeConstants.EasAuthorisedClaims })
                .WithFundLine($"EAS {ageRange} {description} Prince's Trust (£)", new[] { AttributeConstants.EasPrincesTrust })
                .WithFundLine($"EAS {ageRange} {description} Excess Learning Support (£)", new[] { AttributeConstants.EasExcessLearningSupport });

            return fundLineGroup;
        }

        // -------------------------------------------------------------------------------------------------------------------------------------
        // Build ILR Advanced Loans Bursary Budget FundLineGroup
        // -------------------------------------------------------------------------------------------------------------------------------------
        public IFundLineGroup BuildIlrFm99FundLineGroup(byte currentPeriod, IPeriodisedValuesLookup periodisedValues)
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
        public IFundLineGroup BuildEasFm99FundLineGroup(byte currentPeriod,
            IPeriodisedValuesLookup periodisedValues)
        {
            var description = "Advanced Loans Bursary";

            var fundLineGroup = new FundLineGroup($"EAS Total {description} Earnings Adjustment (£)", currentPeriod, FundingDataSource.EAS, new[] {FundLineConstants.AdvancedLearnerLoansBursary}, periodisedValues)
                .WithFundLine($"EAS {description} Excess Support (£)", new[] {AttributeConstants.EasAllbExcessSupport})
                .WithFundLine($"EAS {description} Authorised Claims (£)", new[] {AttributeConstants.EasAuthorisedClaims});

            return fundLineGroup;
        }
    }
}