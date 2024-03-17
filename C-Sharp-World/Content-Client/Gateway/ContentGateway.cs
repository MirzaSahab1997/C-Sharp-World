using IdentityVerificationService.Content_Client.Client;
using Refit;

namespace IdentityVerificationService.Content_Client.Gateway
{
    public class ContentGateway
    {
        private readonly IContentClient _contentClient;
        private readonly ILogger<ContentGateway> _logger;

        public ContentGateway(IContentClient contentClient, ILogger<ContentGateway> logger)
        {
            _contentClient = contentClient ?? throw new ArgumentNullException($"{nameof(contentClient)}");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }

        public async Task<List<string>> UploadDocuments(List<IFormFile> files, string directory)
        {
            List<string> validateResponse = null;
            List<StreamPart> file = new();
            try
            {
                foreach (IFormFile input in files)
                {
                    file.Add(new StreamPart(input.OpenReadStream(), input.FileName, input.ContentType));
                }
                var response = await _contentClient.UploadDocuments(file, directory);
                if (response != null && response.Succeeded)
                    validateResponse = response.Data;
                else
                    _logger.LogError($"upload-documents - Error occured against user {directory} while accessing upload document api {string.Join("\n", response)}");
            }
            catch (AggregateException ex)
            {
                _logger.LogError($"upload-documents - Caught exception against user {directory} ", ex);
                return validateResponse;
            }
            return validateResponse;
        }

        public async Task<List<string>> GetDocuments(string prefix)
        {
            List<string> validateResponse = null;
            try
            {
                var response = await _contentClient.GetDocuments(prefix);
                if (response != null && response.Succeeded)
                    validateResponse = response.Data;
                else
                    _logger.LogError("Error occured while accessing get document api ");
            }
            catch (AggregateException ex)
            {
                _logger.LogError(ex.Message);
                return validateResponse;
            }
            return validateResponse;
        }
    }
}
