using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.FundingSummaryReport
{
    public class FundingCategory : IFundingCategory
    {
        public FundingCategory(string fundingCategoryTitle, byte currentPeriod, List<IFundingSubCategory>fundingSubCategories, string note = null)
        {
            FundingCategoryTitle = fundingCategoryTitle;
            CurrentPeriod = currentPeriod;
            FundingSubCategories = fundingSubCategories ?? new List<IFundingSubCategory>();
            Note = note;
        }

        public List<IFundingSubCategory> FundingSubCategories { get; }

        public string FundingCategoryTitle { get; }

        public string Title => $"Total {FundingCategoryTitle} (£)";

        public byte CurrentPeriod { get; }

        public string ContractAllocationNumber { get;  }

        public string CumulativeFundingCategoryTitle => $"Total {FundingCategoryTitle} Cumulative (£)";

        public decimal Period1 => FundingSubCategories.Sum(fsc => fsc.Period1);

        public decimal Period2 => FundingSubCategories.Sum(fsc => fsc.Period2);

        public decimal Period3 => FundingSubCategories.Sum(fsc => fsc.Period3);

        public decimal Period4 => FundingSubCategories.Sum(fsc => fsc.Period4);

        public decimal Period5 => FundingSubCategories.Sum(fsc => fsc.Period5);

        public decimal Period6 => FundingSubCategories.Sum(fsc => fsc.Period6);

        public decimal Period7 => FundingSubCategories.Sum(fsc => fsc.Period7);

        public decimal Period8 => FundingSubCategories.Sum(fsc => fsc.Period8);

        public decimal Period9 => FundingSubCategories.Sum(fsc => fsc.Period9);

        public decimal Period10 => FundingSubCategories.Sum(fsc => fsc.Period10);

        public decimal Period11 => FundingSubCategories.Sum(fsc => fsc.Period11);

        public decimal Period12 => FundingSubCategories.Sum(fsc => fsc.Period12);

        public decimal Period1To8 => FundingSubCategories.Sum(fsc => fsc.Period1To8);

        public decimal Period9To12 => FundingSubCategories.Sum(fsc => fsc.Period9To12);

        public decimal YearToDate => FundingSubCategories.Sum(fsc => fsc.YearToDate);

        public decimal Total => FundingSubCategories.Sum(fsc => fsc.Total);

        public decimal CumulativePeriod1 => Period1;

        public decimal CumulativePeriod2 => CumulativePeriod1 + Period2;

        public decimal CumulativePeriod3 => CumulativePeriod2 + Period3;

        public decimal CumulativePeriod4 => CumulativePeriod3 + Period4;

        public decimal CumulativePeriod5 => CumulativePeriod4 + Period5;

        public decimal CumulativePeriod6 => CumulativePeriod5 + Period6;

        public decimal CumulativePeriod7 => CumulativePeriod6 + Period7;

        public decimal CumulativePeriod8 => CumulativePeriod7 + Period8;

        public decimal CumulativePeriod9 => CumulativePeriod8 + Period9;

        public decimal CumulativePeriod10 => CumulativePeriod9 + Period10;

        public decimal CumulativePeriod11 => CumulativePeriod10 + Period11;

        public decimal CumulativePeriod12 => CumulativePeriod11 + Period12;

        public string Note { get; }
    }
}