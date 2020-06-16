using System.Collections.Generic;
using System.Linq;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model
{
    public class FundingSubCategory : IFundingSummaryReportRow
    {
        public FundingSubCategory()
        {
        }

        public FundingSubCategory(string fundingSubCategoryTitle, int currentPeriod)
        {
            FundingSubCategoryTitle = fundingSubCategoryTitle;
            CurrentPeriod = currentPeriod;
        }

        public IList<FundLineGroup> FundLineGroups { get; set; } = new List<FundLineGroup>();

        public string FundingSubCategoryTitle { get; set; }

        public string ContractAllocationNumber { get; }

        public string Title => $"Total {FundingSubCategoryTitle} (£)";

        public decimal Period1 => FundLineGroups.Sum(flg => flg.Period1);

        public decimal Period2 => FundLineGroups.Sum(flg => flg.Period2);

        public decimal Period3 => FundLineGroups.Sum(flg => flg.Period3);

        public decimal Period4 => FundLineGroups.Sum(flg => flg.Period4);

        public decimal Period5 => FundLineGroups.Sum(flg => flg.Period5);

        public decimal Period6 => FundLineGroups.Sum(flg => flg.Period6);

        public decimal Period7 => FundLineGroups.Sum(flg => flg.Period7);

        public decimal Period8 => FundLineGroups.Sum(flg => flg.Period8);

        public decimal Period9 => FundLineGroups.Sum(flg => flg.Period9);

        public decimal Period10 => FundLineGroups.Sum(flg => flg.Period10);

        public decimal Period11 => FundLineGroups.Sum(flg => flg.Period11);

        public decimal Period12 => FundLineGroups.Sum(flg => flg.Period12);

        public decimal Period1To8 => FundLineGroups.Sum(flg => flg.Period1To8);

        public decimal Period9To12 => FundLineGroups.Sum(flg => flg.Period9To12);

        public decimal YearToDate => FundLineGroups.Sum(flg => flg.YearToDate);

        public decimal Total => FundLineGroups.Sum(flg => flg.Total);

        public int CurrentPeriod { get; set; }

        public FundingSubCategory WithFundLineGroup(FundLineGroup fundLineGroup)
        {
            if (fundLineGroup != null)
            {
                FundLineGroups.Add(fundLineGroup);
            }

            return this;
        }
    }
}
