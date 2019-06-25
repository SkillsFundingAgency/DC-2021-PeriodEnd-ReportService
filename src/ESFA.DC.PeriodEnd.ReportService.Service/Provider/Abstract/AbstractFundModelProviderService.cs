using System.Threading;
using System.Threading.Tasks;
using ESFA.DC.FileService.Interface;
using ESFA.DC.IO.Interfaces;
using ESFA.DC.Logging.Interfaces;
using ESFA.DC.Serialization.Interfaces;

namespace ESFA.DC.PeriodEnd.ReportService.Service.Provider.Abstract
{
    public abstract class AbstractFundModelProviderService
    {
        protected readonly ILogger _logger;

        protected readonly ISerializationService _serializationService;

        protected readonly IStreamableKeyValuePersistenceService _streamableKeyValuePersistenceService;

        private readonly IFileService _fileService;

        protected AbstractFundModelProviderService(IStreamableKeyValuePersistenceService streamableKeyValuePersistenceService, ISerializationService serializationService, ILogger logger)
        {
            _streamableKeyValuePersistenceService = streamableKeyValuePersistenceService;
            _serializationService = serializationService;
            _logger = logger;
        }

        protected AbstractFundModelProviderService(IFileService fileService, ISerializationService serializationService, ILogger logger)
        {
            _fileService = fileService;
            _serializationService = serializationService;
            _logger = logger;
        }

        protected async Task<TModel> Provide<TModel>(string fileReference, string container, CancellationToken cancellationToken)
        {
            using (var stream = await _fileService.OpenReadStreamAsync(fileReference, container, cancellationToken))
            {
                return _serializationService.Deserialize<TModel>(stream);
            }
        }
    }
}
