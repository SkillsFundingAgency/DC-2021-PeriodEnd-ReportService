using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model;
using ESFA.DC.PeriodEnd.ReportService.Interface.Model.FundingSummaryReport;
using ESFA.DC.PeriodEnd.ReportService.Interface.Reports;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract;
using ESFA.DC.PeriodEnd.ReportService.Service.Reports.Dependencies;

namespace ESFA.DC.ILR.ReportService.Reports.Funding.FundingSummary
{
    public class FundingSummaryReport : BaseReport, IBaseReport
    {
        private readonly IFileNameService _fileNameService;
        private readonly IModelBuilder<IFundingSummaryReport> _fundingSummaryReportModelBuilder;
        private readonly IExcelService _excelService;
        private readonly IRenderService<IFundingSummaryReport> _fundingSummaryReportRenderService;

        public FundingSummaryReport(
            IFileNameService fileNameService,
            IModelBuilder<IFundingSummaryReport> fundingSummaryReportModelBuilder,
            IExcelService excelService,
            IRenderService<IFundingSummaryReport> fundingSummaryReportRenderService)
            : base(ReportTaskNameConstants.FundingSummaryReport, "Funding Summary Report")
        {
            _fileNameService = fileNameService;
            _fundingSummaryReportModelBuilder = fundingSummaryReportModelBuilder;
            _excelService = excelService;
            _fundingSummaryReportRenderService = fundingSummaryReportRenderService;
        }

        public virtual IEnumerable<Type> DependsOn
            => new[]
            {
                DependentDataCatalog.Fm25,
                DependentDataCatalog.Fm35,
                DependentDataCatalog.Fm36,
                DependentDataCatalog.Fm81,
                DependentDataCatalog.Fm99,
                DependentDataCatalog.ReferenceData,
            };

        public async Task<IEnumerable<string>> GenerateAsync(
            IReportServiceContext reportServiceContext,
            IReportServiceDependentData reportsDependentData,
            CancellationToken cancellationToken)
        {
            var fundingSummaryReportModel = _fundingSummaryReportModelBuilder.Build(reportServiceContext, reportsDependentData);

            var fileName = _fileNameService.GetFilename(reportServiceContext, FileName, OutputTypes.Excel);

            using (var workbook = _excelService.NewWorkbook())
            {
                var worksheet = _excelService.GetWorksheetFromWorkbook(workbook, 0);

                _fundingSummaryReportRenderService.Render(fundingSummaryReportModel, worksheet);

                await _excelService.SaveWorkbookAsync(workbook, fileName, reportServiceContext.Container, cancellationToken);
            }

            return new[] { fileName };
        }
    }
}
