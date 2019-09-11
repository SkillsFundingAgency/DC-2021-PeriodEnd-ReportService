namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.Abstract
{
    public abstract class BaseReport
    {
        protected BaseReport(string taskName, string fileName)
        {
            TaskName = taskName;
            FileName = fileName;
        }

        public string TaskName { get; }

        public string FileName { get; }
    }
}