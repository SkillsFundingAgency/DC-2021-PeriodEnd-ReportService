using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.FileService.Interface;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Tests.Stubs
{
    public class FileServiceStub : IFileService
    {
        public async Task<Stream> OpenReadStreamAsync(string fileReference, string container, CancellationToken cancellationToken)
        {
            return await Task.FromResult(File.OpenRead(Path.Combine(container, fileReference)) as Stream);
        }

        public async Task<Stream> OpenWriteStreamAsync(string fileReference, string container, CancellationToken cancellationToken)
        {
            if (!Directory.Exists(container))
            {
                Directory.CreateDirectory(container);
            }

            var directoryFromFileReference = Path.Combine(container, Path.GetDirectoryName(fileReference));

            if (!Directory.Exists(directoryFromFileReference))
            {
                Directory.CreateDirectory(directoryFromFileReference);
            }

            return await Task.FromResult(File.OpenWrite(Path.Combine(container, fileReference)) as Stream);
        }

        public Task<IEnumerable<string>> GetFileReferencesAsync(string container, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string fileReference, string container, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetFileReferencesAsync(string container, string prefix, bool includeSubfolders, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}