using System.Collections.Generic;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.DataExtractReport;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd
{
    public interface IDataExtractModelBuilder
    {
        IEnumerable<DataExtractModel> BuildModel(IEnumerable<DataExtractModel> summarisationInfo, IEnumerable<DataExtractFcsInfo> fcsInfo);
    }
}