CREATE TABLE [dbo].[AppsCoInvestmentContribution]
(
	[Ukprn] INT NOT NULL , 
    [ReturnPeriod] INT NOT NULL, 
    [LearnRefNumber] VARCHAR(12) NULL, 
    [UniqueLearnerNumber] BIGINT NULL, 
    [LearningStartDate] DATETIME NULL, 
    [ProgType] INT NULL, 
    [StandardCode] INT NULL, 
    [FrameworkCode] INT NULL, 
    [ApprenticeshipPathway] INT NULL, 
    [SoftwareSupplierAimIdentifier] VARCHAR(36) NULL, 
    [LearningDeliveryFAMTypeApprenticeshipContractType] TINYINT NULL, 
    [EmployerIdentifierAtStartOfLearning] INT NULL, 
    [EmployerNameFromApprenticeshipService] VARCHAR(100) NULL, 
    [TotalPMRPreviousFundingYears] DECIMAL(15,5) NULL, 
    [TotalCoInvestmentDueFromEmployerInPreviousFundingYears] DECIMAL(15,5) NULL, 
    [TotalPMRThisFundingYear] DECIMAL(15,5) NULL, 
    [TotalCoInvestmentDueFromEmployerThisFundingYear] DECIMAL(15,5) NULL, 
    [PercentageOfCoInvestmentCollected] DECIMAL(28,10) NOT NULL, 
    [LDM356Or361] VARCHAR(3) NULL, 
    [CoInvestmentDueFromEmployerForAugust] DECIMAL(15,5) NOT NULL, 
    [CoInvestmentDueFromEmployerForSeptember] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForOctober] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForNovember] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForDecember] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForJanuary] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForFebruary] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForMarch] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForApril] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForMay] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForJune] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForJuly] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForR13] DECIMAL(15,5) NOT NULL,
    [CoInvestmentDueFromEmployerForR14] DECIMAL(15,5) NOT NULL, 
    [CompletionEarningThisFundingYear] DECIMAL(15,5) NOT NULL, 
    [CompletionPaymentsThisFundingYear] DECIMAL(15,5) NOT NULL, 
    [EmployerCoInvestmentPercentage] DECIMAL(15,5) NULL, 
    [ApplicableProgrammeStartDate] DATETIME NULL 
)

GO

CREATE CLUSTERED INDEX [IX_AppsCoInvestmentContribution_Ukprn_ReturnPeriod] ON [dbo].[AppsCoInvestmentContribution] ([Ukprn],[ReturnPeriod])
