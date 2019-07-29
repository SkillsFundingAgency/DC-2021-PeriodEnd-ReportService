using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.PeriodEnd.ReportService.Model.ReportModels;

namespace ESFA.DC.PeriodEnd.ReportService.Interface.Service
{
    public interface IAppsAdditionalPaymentsModelBuilder
    {
        AppsAdditionalPaymentsModel BuildModel(ILearner learner, FM36Learner learnerData);
    }
}