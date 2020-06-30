using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Builders
{
    public interface IPaymentsBuilder
    {
        IDictionary<AppsCoInvestmentRecordKey, List<Payment>> BuildPaymentsLookupDictionary(ICollection<Payment> payments);

        List<AppsCoInvestmentRecordKey> GetUniqueCombinationsOfKeyFromPaymentsAsync(ICollection<Payment> payments);

        IEnumerable<Payment> GetPaymentsForRecord(IDictionary<AppsCoInvestmentRecordKey, List<Payment>> paymentsDictionary, AppsCoInvestmentRecordKey record);

        IEnumerable<Payment> FilterByFundingSourceAndTransactionType(IEnumerable<Payment> payments, int fundingSource, HashSet<int> transactionTypes);

        decimal CalculateCompletionEarningsThisFundingYear(LearningDelivery learningDelivery, ICollection<AECApprenticeshipPriceEpisodePeriodisedValues> aecApprenticeshipPriceEpisodePeriodisedValues);

        decimal CalculateCompletionPaymentsInAcademicYear(IEnumerable<Payment> payments, int currentAcademicYear);

        decimal TotalCoInvestmentDueFromEmployerInPreviousFundingYears(IEnumerable<Payment> payments, int currentAcademicYear);

        Dictionary<byte, decimal> BuildCoInvestmentPaymentsPerPeriodDictionary(IEnumerable<Payment> payments, int currentAcademicYear);

        Payment GetEarliestPayment(IEnumerable<Payment> payments);

        decimal? GetEmployerCoInvestmentPercentage(IEnumerable<Payment> payments);

        EarningsAndPayments BuildEarningsAndPayments(IEnumerable<Payment> filteredPayments, IEnumerable<Payment> allPayments, LearningDelivery learningDelivery,
            ICollection<AECApprenticeshipPriceEpisodePeriodisedValues> aecPriceEpisodePeriodisedValues,
            int currentAcademicYear, DateTime academicYearStart, DateTime nextAcademicYearStart);

        decimal? GetTotalPmrBetweenDates(LearningDelivery learningDelivery, DateTime? startDate, DateTime? endDate);

        decimal GetPercentageOfInvestmentCollected(decimal? totalDueCurrentYear, decimal? totalDuePreviousYear, decimal? totalCollectedCurrentYear, decimal? totalCollectedPreviousYear);
    }
}
