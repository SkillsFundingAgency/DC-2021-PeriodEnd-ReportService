using System.Collections.Generic;
using System.Linq;
using Aspose.Cells;
using ESFA.DC.DateTimeProvider.Interface;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.ProviderSubmissions.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.ProviderSubmissions
{
    public class ProviderSubmissionsRenderService : IProviderSubmissionsRenderService
    {
        private readonly IDateTimeProvider _dateTimeProvider;

        public ProviderSubmissionsRenderService(IDateTimeProvider dateTimeProvider)
        {
            _dateTimeProvider = dateTimeProvider;
        }

        public Worksheet Render(int periodNumber, ICollection<ProviderSubmissionModel> providerSubmissionModels, Worksheet worksheet, Workbook workbook)
        {
            var returned = providerSubmissionModels.Count(x => x.Returned);
            var expectedButNotReturned = providerSubmissionModels.Count(x => x.Expected && !x.Returned);
            var zeroLearnerReturns = providerSubmissionModels.Count(x => x.Returned && x.TotalValid == 0);
            var expected = providerSubmissionModels.Count(x => x.Expected);
            var returningExpected = providerSubmissionModels.Count(x => x.Returned && x.Expected);
            var returningUnexpected = providerSubmissionModels.Count(x => x.Returned && !x.Expected);

            worksheet.Cells[1, 1].PutValue($"ILR Provider Submissions Report - R{periodNumber:D2}");
            worksheet.Cells[2, 1].PutValue($"Report Run: {_dateTimeProvider.ConvertUtcToUk(_dateTimeProvider.GetNowUtc()):u}");
            worksheet.Cells[4, 1].PutValue($"Total No of Returning Providers: {returned}");
            worksheet.Cells[5, 1].PutValue($"No of Expected Providers Not Returning: {expectedButNotReturned}");
            worksheet.Cells[6, 1].PutValue($"No of \"0 Learner\" Returns: {zeroLearnerReturns}");
            worksheet.Cells[7, 1].PutValue($"No of Providers Expected to Return: {expected}");
            worksheet.Cells[8, 1].PutValue($"No of Returning Expected Providers: {returningExpected}");
            worksheet.Cells[9, 1].PutValue($"No of Returning Unexpected Providers: {returningUnexpected}");

            var designer = new WorkbookDesigner
            {
                Workbook = workbook
            };

            designer.SetDataSource("ProviderSubmissions", providerSubmissionModels);
            designer.Process();

            return worksheet;
        }
    }
}
