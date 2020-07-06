using System.Collections.Generic;
using System.Threading;
using ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsAdditionalPayments.Persistance
{
    public interface IAppsAdditionalPaymentPersistanceMapper
    {
        IEnumerable<ReportData.Model.AppsAdditionalPayment> Map(IReportServiceContext reportServiceContext, IEnumerable<AppsAdditionalPaymentReportModel> appsAdditionalPaymentReportModels, CancellationToken cancellationToken);
    }
}