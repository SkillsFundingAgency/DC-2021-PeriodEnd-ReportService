using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.ILR1920.DataStore.EF;
using ESFA.DC.PeriodEnd.ReportService.Interface;
using ESFA.DC.PeriodEnd.ReportService.Service.Provider;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests.Providers
{
    public class PeriodisedValueLookupProviderTests
    {
        private const string ConnectionString = string.Empty;

        [Fact]
        public async Task BuildFm35Dictionary()
        {
            var result = await NewService().BuildFm35DictionaryAsync(10005788, CancellationToken.None);
        }

        [Fact]
        public async Task BuildFm25Dictionary()
        {
            var result = await NewService().BuildFm25DictionaryAsync(10005788, CancellationToken.None);
        }

        [Fact]
        public async Task BuildAlbDictionary()
        {
            var result = await NewService().BuildFm99DictionaryAsync(10005788, CancellationToken.None);
        }

        [Fact]
        public async Task BuildTblDictionary()
        {
            var result = await NewService().BuildFm81DictionaryAsync(10005788, CancellationToken.None);
        }

        [Fact]
        public async Task ProvideAsync()
        {
            var context = new Mock<IReportServiceContext>();

            context.SetupGet(c => c.Ukprn).Returns(10005788);

            var result = await NewService().ProvideAsync(context.Object, CancellationToken.None);
        }

        public static PeriodisedValuesLookupProviderService NewService()
        {
            var builder = new DbContextOptionsBuilder<ILR1920_DataStoreEntities>();
            builder.UseSqlServer(ConnectionString);

            return new PeriodisedValuesLookupProviderService(() => new ILR1920_DataStoreEntities(builder.Options));
        }
    }
}
