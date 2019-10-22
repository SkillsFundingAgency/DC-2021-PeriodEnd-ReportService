using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.InternalReports.DataExtractReport;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders
{
    public class DataExtractModelBuilder : IDataExtractModelBuilder
    {
        public IEnumerable<DataExtractModel> BuildModel(
            IEnumerable<DataExtractModel> summarisationInfo,
            IEnumerable<DataExtractFcsInfo> fcsInfo)
        {
            foreach (DataExtractModel dataExtractModel in summarisationInfo)
            {
                dataExtractModel.UkPrn =
                    fcsInfo.SingleOrDefault(x => string.Equals(x.OrganisationIdentifier, dataExtractModel.OrganisationId, StringComparison.OrdinalIgnoreCase))?.UkPrn;
            }

            return summarisationInfo;
        }
    }
}
