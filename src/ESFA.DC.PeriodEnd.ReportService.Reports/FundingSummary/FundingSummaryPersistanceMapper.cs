using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance;
using ESFA.DC.ReportData.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.FundingSummary
{
    public class FundingSummaryPersistanceMapper : IFundingSummaryPersistanceMapper
    {
        public IEnumerable<FundingSummaryReport> Map(IReportServiceContext reportServiceContext, FundingSummaryReportModel fundingSummaryReportModel, CancellationToken cancellationToken)
        {
            var persistModels = fundingSummaryReportModel.FundingCategories.SelectMany(fc => fc.FundingSubCategories.SelectMany(fsc =>
                fsc.FundLineGroups.SelectMany(flg => flg.FundLines.Select(fl => new FundingSummaryReport
                {
                    Ukprn = reportServiceContext.Ukprn,
                    ReturnPeriod = reportServiceContext.ReturnPeriod,
                    ContractNo = fc.ContractAllocationNumber,
                    FundingCategory = fc.FundingCategoryTitle,
                    FundingSubCategory = fsc.FundingSubCategoryTitle,
                    FundLine = fl.Title,
                    Aug20 = fl.Period1,
                    Sep20 = fl.Period2,
                    Oct20 = fl.Period3,
                    Nov20 = fl.Period4,
                    Dec20 = fl.Period5,
                    Jan21 = fl.Period6,
                    Feb21 = fl.Period7,
                    Mar21 = fl.Period8,
                    Apr21 = fl.Period9,
                    May21 = fl.Period10,
                    Jun21 = fl.Period11,
                    Jul21 = fl.Period12,
                    AugMar = fl.Period1To8,
                    AprJul = fl.Period9To12,
                    YearToDate = fl.YearToDate,
                    Total = fl.Total
                })))).ToList();

            return persistModels;
        }
    }
}
