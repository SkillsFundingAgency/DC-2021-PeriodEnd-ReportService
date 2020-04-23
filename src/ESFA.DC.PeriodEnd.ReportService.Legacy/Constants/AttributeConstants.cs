namespace ESFA.DC.PeriodEnd.ReportService.Legacy.Constants
{
    public static class AttributeConstants
    {
        // FM25 Attributes
        public const string Fm25LrnOnProgPay = "LnrOnProgPay";

        // FM35 Attributes
        public const string Fm35OnProgPayment = "OnProgPayment";
        public const string Fm35BalancePayment = "BalancePayment";
        public const string Fm35EmpOutcomePay = "EmpOutcomePay";
        public const string Fm35AchievePayment = "AchievePayment";
        public const string Fm35LearnSuppFundCash = "LearnSuppFundCash";
        public const string Fm35AchievePayPct = "AchievePayPct";

        // FM36 Attributes
        public const string Fm36PriceEpisodeOnProgPaymentAttributeName = "PriceEpisodeOnProgPayment";
        public const string Fm3PriceEpisodeBalancePaymentAttributeName = "PriceEpisodeBalancePayment";
        public const string Fm36PriceEpisodeCompletionPaymentAttributeName = "PriceEpisodeCompletionPayment";
        public const string Fm36PriceEpisodeLSFCashAttributeName = "PriceEpisodeLSFCash";
        public const string Fm36PriceEpisodeFirstDisadvantagePaymentAttributeName = "PriceEpisodeFirstDisadvantagePayment";
        public const string Fm36PriceEpisodeSecondDisadvantagePaymentAttributeName = "PriceEpisodeSecondDisadvantagePayment";
        public const string Fm36PriceEpisodeFirstEmp1618PayAttributeName = "PriceEpisodeFirstEmp1618Pay";
        public const string Fm36PriceEpisodeSecondEmp1618PayAttributeName = "PriceEpisodeSecondEmp1618Pay";
        public const string Fm36PriceEpisodeFirstProv1618PayAttributeName = "PriceEpisodeFirstProv1618Pay";
        public const string Fm36PriceEpisodeSecondProv1618PayAttributeName = "PriceEpisodeSecondProv1618Pay";
        public const string Fm36PriceEpisodeLearnerAdditionalPaymentAttributeName = "PriceEpisodeLearnerAdditionalPayment";
        public const string Fm36PriceEpisodeApplic1618FrameworkUpliftOnProgPaymentAttributeName = "PriceEpisodeApplic1618FrameworkUpliftOnProgPayment";
        public const string Fm36PriceEpisodeApplic1618FrameworkUpliftBalancingAttributeName = "PriceEpisodeApplic1618FrameworkUpliftBalancing";
        public const string Fm36PriceEpisodeApplic1618FrameworkUpliftCompletionPaymentAttributeName = "PriceEpisodeApplic1618FrameworkUpliftCompletionPayment";
        public const string Fm36ProgrammeAimOnProgPayment = "ProgrammeAimOnProgPayment";
        public const string Fm36ProgrammeAimBalPayment = "ProgrammeAimBalPayment";
        public const string Fm36ProgrammeAimCompletionPayment = "ProgrammeAimCompletionPayment";
        public const string Fm36ProgrammeAimProgFundIndMinCoInvest = "ProgrammeAimProgFundIndMinCoInvest";
        public const string Fm36MathEngOnProgPayment = "MathEngOnProgPayment";
        public const string Fm36MathEngBalPayment = "MathEngBalPayment";
        public const string Fm36LDApplic1618FrameworkUpliftBalancingPayment = "LDApplic1618FrameworkUpliftBalancingPayment";
        public const string Fm36LDApplic1618FrameworkUpliftCompletionPayment = "LDApplic1618FrameworkUpliftCompletionPayment";
        public const string Fm36LDApplic1618FrameworkUpliftOnProgPayment = "LDApplic1618FrameworkUpliftOnProgPayment";
        public const string Fm36DisadvFirstPayment = "DisadvFirstPayment";
        public const string Fm36DisadvSecondPayment = "DisadvSecondPayment";
        public const string Fm36LearnDelFirstProv1618Pay = "LearnDelFirstProv1618Pay";
        public const string Fm36LearnDelSecondProv1618Pay = "LearnDelSecondProv1618Pay";
        public const string Fm36LearnDelFirstEmp1618Pay = "LearnDelFirstEmp1618Pay";
        public const string Fm36LearnDelSecondEmp1618Pay = "LearnDelSecondEmp1618Pay";
        public const string Fm36LearnSuppFundCash = "LearnSuppFundCash";
        public const string Fm36LearnDelLearnAddPayment = "LearnDelLearnAddPayment";
        public const string Fm36PriceEpisodeContractTypeLevyContract = "Levy Contract";
        public const string Fm36LearnDelFAMTypeAct = "ACT";
        public const string Fm36LearnDelFAMCodeOne = "1";
        public const string Fm36FundLineType = "FundLineType";

        // FM81
        public const string Fm81CoreGovContPayment = "CoreGovContPayment";
        public const string Fm81MathEngBalPayment = "MathEngBalPayment";
        public const string Fm81MathEngOnProgPayment = "MathEngOnProgPayment";
        public const string Fm81AchPayment = "AchPayment";
        public const string Fm81SmallBusPayment = "SmallBusPayment";
        public const string Fm81YoungAppPayment = "YoungAppPayment";
        public const string Fm81LearnSuppFundCash = "LearnSuppFundCash";
        public const string Fm81YoungAppFirstPayment = "YoungAppFirstPayment";
        public const string Fm81YoungAppSecondPayment = "YoungAppSecondPayment";

        // FM99
        public const string Fm99ALBCode = "ALBCode";
        public const string Fm99AlbSupportPayment = "ALBSupportPayment";
        public const string Fm99AreaUpliftBalPayment = "AreaUpliftBalPayment";
        public const string Fm99AreaUpliftOnProgPayment = "AreaUpliftOnProgPayment";

        // EAS
        public const string EasAllbExcessSupport = "ALLB Excess Support";
        public const string EasAuthorisedClaims = "Authorised Claims";
        public const string EasAuthorisedClaimsApprentice = "Authorised Claims - Additional payments for apprentice";
        public const string EasAuthorisedClaimsEmployer = "Authorised Claims - Additional payments for employer";
        public const string EasAuthorisedClaimsProvider = "Authorised Claims - Additional payments for provider";
        public const string EasAuthorisedClaimsTraining = "Authorised Claims - Training costs exc Maths/Eng";
        public const string EasDiscretionaryBursary = "Discretionary Bursary";
        public const string EasExcessLearningSupport = "Excess Learning Support";
        public const string EasFreeMeals = "Free Meals";
        public const string EasLearnerSupport = "Learner Support";
        public const string EasPrincesTrust = "Princes Trust";
        public const string EasVulnerableBursary = "Vulnerable Bursary";
        public const string EasMcaGlaDefinedAdjustment1 = "MCA/GLA Defined Adjustment 1";
        public const string EasMcaGlaDefinedAdjustment2 = "MCA/GLA Defined Adjustment 2";
        public const string EasMcaGlaDefinedAdjustment3 = "MCA/GLA Defined Adjustment 3";
    }
}