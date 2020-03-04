CREATE TABLE [dbo].[FundingSummaryReport]
(
	[Ukprn] INT NOT NULL,
	[ReturnPeriod] INT NOT NULL,
    [ContractNo] NVARCHAR(100) NULL,
    [FundingCategory] NVARCHAR(90) NULL,
    [FundingSubCategory] NVARCHAR(70) NULL,
    [FundLine] NVARCHAR(200) NULL,
    [Aug19] DECIMAL(15, 5) NULL,
    [Sep19] DECIMAL(15, 5) NULL,
    [Oct19] DECIMAL(15, 5) NULL,
    [Nov19] DECIMAL(15, 5) NULL,
    [Dec19] DECIMAL(15, 5) NULL,
    [Jan20] DECIMAL(15, 5) NULL,
    [Feb20] DECIMAL(15, 5) NULL,
    [Mar20] DECIMAL(15, 5) NULL,
    [Apr20] DECIMAL(15, 5) NULL,
    [May20] DECIMAL(15, 5) NULL,
    [Jun20] DECIMAL(15, 5) NULL,
    [Jul20] DECIMAL(15, 5) NULL,
    [AugMar] DECIMAL(15, 5) NULL,
    [AprJul] DECIMAL(15, 5) NULL,
    [YearToDate] DECIMAL(15, 5) NULL,
    [Total] DECIMAL(15, 5) NULL
)

GO


CREATE INDEX [IX_FundingSummaryReport_Column] ON [dbo].[FundingSummaryReport] ([Ukprn], [ReturnPeriod])
