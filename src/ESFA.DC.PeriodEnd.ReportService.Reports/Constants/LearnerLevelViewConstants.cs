using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Constants
{
    public static class LearnerLevelViewConstants
    {
        public const string ReasonForIssues_CompletionHoldbackPayment = "Completion Holdback";
        public const string ReasonForIssues_Clawback = "Clawback";
        public const string ReasonForIssues_Other = "Other Issue";
        public const string DLockErrorRuleNamePrefix = "DLOCK_";

        // Transaction and funding types for LLV report
        public static readonly List<byte?> eSFATransactionTypes = new List<byte?> { 4, 5, 6, 7, 8, 9, 10, 11, 12, 15, 16 };
        public static readonly List<byte?> eSFANONZPTransactionTypes = new List<byte?> { 13, 14, 15 };
        public static readonly List<byte?> eSFAFundingSources = new List<byte?> { 1, 2, 5 };
        public static readonly List<byte?> eSFAFSTransactionTypes = new List<byte?> { 1, 2, 3 };
        public static readonly List<byte?> coInvestmentundingSources = new List<byte?> { 3 };
        public static readonly List<byte?> coInvestmentTransactionTypes = new List<byte?> { 1, 2, 3 };
    }
}
