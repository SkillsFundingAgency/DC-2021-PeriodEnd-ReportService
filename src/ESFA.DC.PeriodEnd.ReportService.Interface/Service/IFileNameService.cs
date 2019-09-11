using System;
using System.Collections.Generic;
using System.Text;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IFileNameService
    {
        string GetFilename(IReportServiceContext reportServiceContext, string fileName, OutputTypes outputType, bool includeDateTime = true);
    }
}
