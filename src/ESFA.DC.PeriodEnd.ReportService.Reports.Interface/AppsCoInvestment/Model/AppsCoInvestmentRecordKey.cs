using System;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.Interface.AppsCoInvestment.Model
{
    public struct AppsCoInvestmentRecordKey
    {
        public AppsCoInvestmentRecordKey(
            string learnRefNumber,
            DateTime? learnStartDate,
            int progType,
            int stdCode,
            int fworkCode,
            int pwayCode)
        {
            LearningAimReference = "ZPROG001";
            LearnerReferenceNumber = learnRefNumber;
            LearningStartDate = learnStartDate;
            ProgrammeType = progType;
            StandardCode = stdCode;
            FrameworkCode = fworkCode;
            PathwayCode = pwayCode;
        }

        public string LearningAimReference { get; }

        public string LearnerReferenceNumber { get; }

        public DateTime? LearningStartDate { get; }

        public int ProgrammeType { get; }

        public int StandardCode { get; }

        public int FrameworkCode { get; }

        public int PathwayCode { get; }

        public override int GetHashCode()
            =>
            (
            LearningAimReference.ToUpper(),
            LearnerReferenceNumber.ToUpper(),
            LearningStartDate,
            ProgrammeType,
            StandardCode,
            FrameworkCode,
            PathwayCode
            ).GetHashCode();
    }
}
