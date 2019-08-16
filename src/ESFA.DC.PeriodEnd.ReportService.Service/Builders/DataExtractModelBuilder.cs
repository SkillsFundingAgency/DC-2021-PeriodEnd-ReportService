using System;
using System.Collections.Generic;
using System.Linq;
using ESFA.DC.PeriodEnd.ReportService.Interface.Builders.PeriodEnd;
using ESFA.DC.PeriodEnd.ReportService.Interface.Service;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.AppsAdditionalPayment;
using ESFA.DC.PeriodEnd.ReportService.Model.PeriodEnd.DataExtractReport;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;
using ESFA.DC.PeriodEnd.ReportService.Service.Constants;
using ESFA.DC.PeriodEnd.ReportService.Service.Extensions;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Builders.PeriodEnd
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
