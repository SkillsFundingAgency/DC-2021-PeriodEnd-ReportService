using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Model;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.FundingSummary.Persistance.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.FundingSummary
{
    public class FundingSummaryPersistanceMapper : IFundingSummaryPersistanceMapper
    {
        public IEnumerable<FundingSummaryPersistModel> Map(IReportServiceContext reportServiceContext, FundingSummaryReportModel fundingSummaryReportModel, CancellationToken cancellationToken)
        {
            var persistModels = fundingSummaryReportModel.FundingCategories.SelectMany(fc => fc.FundingSubCategories.SelectMany(fsc =>
                fsc.FundLineGroups.SelectMany(flg => flg.FundLines.Select(fl => new FundingSummaryPersistModel
                {
                    Ukprn = reportServiceContext.Ukprn,
                    ReturnPeriod = reportServiceContext.ReturnPeriod,
                    ContractNo = fc.ContractAllocationNumber,
                    FundingCategory = fc.FundingCategoryTitle,
                    FundingSubCategory = fsc.FundingSubCategoryTitle,
                    FundLine = fl.Title,
                    Aug19 = fl.Period1,
                    Sep19 = fl.Period2,
                    Oct19 = fl.Period3,
                    Nov19 = fl.Period4,
                    Dec19 = fl.Period5,
                    Jan20 = fl.Period6,
                    Feb20 = fl.Period7,
                    Mar20 = fl.Period8,
                    Apr20 = fl.Period9,
                    May20 = fl.Period10,
                    Jun20 = fl.Period11,
                    Jul20 = fl.Period12,
                    AugMar = fl.Period1To8,
                    AprJul = fl.Period9To12,
                    YearToDate = fl.YearToDate,
                    Total = fl.Total
                })))).ToList();

            return persistModels;
        }
    }
}
