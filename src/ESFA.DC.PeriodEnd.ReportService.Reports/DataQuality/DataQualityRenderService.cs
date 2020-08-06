using System;
using System.Collections.Generic;
using System.Text;
using Aspose.Cells;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.DataQuality.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.DataQuality
{
    public class DataQualityRenderService : IDataQualityRenderService
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public DataQualityRenderService(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public Worksheet Render(string periodNumberName, DataQualityProviderModel dataQualityProvideModel, Worksheet worksheet, Workbook workbook)
        {
            worksheet.Cells[1, 1].PutValue($"ILR Data Quality Reports - {periodNumberName}");
            worksheet.Cells[2, 1].PutValue($"Report Run: {_dateTimeProvider.ConvertUtcToUk(_dateTimeProvider.GetNowUtc()):u}");

            var designer = new WorkbookDesigner
            {
                Workbook = workbook
            };

            designer.SetDataSource("ReturningProvidersInfo",dataQualityProvideModel.ReturningProviders);
            designer.SetDataSource("RuleViolationsInfo", dataQualityProvideModel.RuleViolations);
            designer.SetDataSource("ProviderWithoutValidLearnerInfo", dataQualityProvideModel.ProvidersWithoutValidLearners);
            designer.SetDataSource("Top10ProvidersWithInvalidLearners", dataQualityProvideModel.ProvidersWithMostInvalidLearners);
            designer.Process();

            return worksheet;
        }
    }
}
