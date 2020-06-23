using System;
using System.Collections.Generic;
using System.Text;
using ESFA.DC.ReferenceData.FCS.Model;

namespace ESFA.DC.PeriodEnd.ReportService.Reports.AppsMonthly.Model
{
    public struct RecordKey
    {
        public RecordKey(
            string learnerReferenceNumber,
            long uln,
            string learnAimRef,
            DateTime? learnStartDate,
            int programmeType,
            int standardCode,
            int frameworkCode,
            int pathwayCode,
            string reportingAimFundingLineType,
            string priceEpisodeIdentifier,
            byte contractType)
        {
            LearnerReferenceNumber = learnerReferenceNumber;
            Uln = uln;
            LearningAimReference = learnAimRef;
            LearnStartDate = learnStartDate;
            ProgrammeType = programmeType;
            StandardCode = standardCode;
            FrameworkCode = frameworkCode;
            PathwayCode = pathwayCode;
            ReportingAimFundingLineType = reportingAimFundingLineType;
            PriceEpisodeIdentifier = priceEpisodeIdentifier;
            ContractType = contractType;
        }

        public string LearnerReferenceNumber { get; }

        public long Uln { get; }

        public string LearningAimReference { get;}

        public DateTime? LearnStartDate { get; }

        public int ProgrammeType { get; }

        public int StandardCode { get; }

        public int FrameworkCode { get; }

        public int PathwayCode { get; }

        public string ReportingAimFundingLineType { get; }

        public string PriceEpisodeIdentifier { get; }

        public byte ContractType { get; }

        public override int GetHashCode()
            =>
            (
                Uln,
                ProgrammeType,
                StandardCode,
                FrameworkCode,
                PathwayCode,
                LearnStartDate,
                ContractType,
                LearnerReferenceNumber?.ToUpper(),
                LearningAimReference?.ToUpper(),
                ReportingAimFundingLineType?.ToUpper(),
                PriceEpisodeIdentifier?.ToUpper()
            ).GetHashCode();
    }
}
