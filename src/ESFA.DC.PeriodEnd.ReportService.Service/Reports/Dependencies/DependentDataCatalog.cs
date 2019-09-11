using System;
using System.Collections.Generic;
using ESFA.DC.ILR.FundingService.FM36.FundingOutput.Model.Output;
using ESFA.DC.ILR.Model.Interface;
using ESFA.DC.ILR1920.DataStore.EF;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Reports.Dependencies
{
    public static class DependentDataCatalog
    {
        public static readonly Type ValidIlr = typeof(IMessage);

        public static readonly Type InvalidIlr = null;

        public static readonly Type ValidationErrors = typeof(List<ValidationError>);

        public static readonly Type ReferenceData = null;

        public static readonly Type Fm25 = null;

        public static readonly Type Fm35 = null;

        public static readonly Type Fm36 = null;

        public static readonly Type Fm81 = null;

        public static readonly Type Fm99 = null;

        //public static readonly Type ValidIlr = typeof(IMessage);

        //public static readonly Type InvalidIlr = typeof(ILooseMessage);

        //public static readonly Type ValidationErrors = typeof(List<ValidationError>);

        //public static readonly Type ReferenceData = typeof(ReferenceDataRoot);

        //public static readonly Type Fm25 = typeof(FM25Global);

        //public static readonly Type Fm35 = typeof(FM35Global);

        //public static readonly Type Fm36 = typeof(FM36Global);

        //public static readonly Type Fm81 = typeof(FM81Global);

        //public static readonly Type Fm99 = typeof(ALBGlobal);
    }
}