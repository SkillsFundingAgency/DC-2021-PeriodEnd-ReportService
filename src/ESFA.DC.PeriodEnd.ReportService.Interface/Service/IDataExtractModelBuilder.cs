using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataExtractReport;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IDataExtractModelBuilder
    {
        IEnumerable<DataExtractModel> BuildModel(IEnumerable<DataExtractModel> summarisationInfo, IEnumerable<DataExtractFcsInfo> fcsInfo);
    }
}