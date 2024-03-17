using AutoMapper;
using IdentityVerificationService.Application.Repositories;
using IdentityVerificationService.Content_Client.Gateway;
using IdentityVerificationService.Domain.DTOs.IdVerification;
using IdentityVerificationService.Domain.DTOs.IdVerification.Request;
using IdentityVerificationService.Domain.DTOs.IdVerification.Response;
using IdentityVerificationService.Domain.Entities.IdVerification;
using Newtonsoft.Json;

namespace IdentityVerificationService.Application.Services
{
    public class IDVerificationService
    {
        private readonly CaaryRepositories _caaryRepositories;
        private readonly ILogger<IDVerificationService> _logger;
        private readonly HttpClientService _httpClientService;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly ContentGateway _contentGateway;

        public string UserName { get; set; }

        public IDVerificationService(CaaryRepositories caaryRepositories, ILogger<IDVerificationService> logger, HttpClientService httpClientService,
            IMapper mapper, IConfiguration configuration, ContentGateway contentGateway)
        {
            _caaryRepositories = caaryRepositories ?? throw new ArgumentNullException(nameof(caaryRepositories));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpClientService = httpClientService ?? throw new ArgumentNullException(nameof(httpClientService));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _configuration = configuration ?? throw new ArgumentNullException($"{nameof(configuration)}");
            _contentGateway = contentGateway ?? throw new ArgumentNullException($"{nameof(contentGateway)}");
        }

        public async Task<(string, AuthorizationDto)> GetAccessRefreshTokenAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var authorizeURL = _configuration["TruliooUrl:AccessToken"];
                var clientResponse = await _httpClientService.SendAsync(targetAbsoluteUrl: authorizeURL, methodType: "POST", cancellationToken: cancellationToken);
                if (!clientResponse.ToLower().Contains("message"))
                {
                    return ("Access token found", JsonConvert.DeserializeObject<AuthorizationDto>(clientResponse));
                }

                return ("Access token not found", JsonConvert.DeserializeObject<AuthorizationDto>(clientResponse));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IDVerificationService)}-{nameof(GetAccessRefreshTokenAsync)}");
                return ($"Something went wrong", null);
            }
        }

        public async Task<(string, InitializeWebSDK)> GetShortCodeForSdkAsync(long? userId, CancellationToken cancellationToken = default)
        {
            ConfigureTransactionDto response = null;
            try
            {
                var shortcodeURL = _configuration["TruliooUrl:ShortCode"];
                (var responseMessage, AuthorizationDto result) = await GetAccessRefreshTokenAsync(cancellationToken);
                if (result == null)
                {
                    return (responseMessage, null);
                }

                (_, response) = await CreateAndConfigureTransaction(result);
                if (response == null)
                {
                    return ("Transaction Id Not found, Cannot Configure Driving License on SDK", null);
                }

                (_, _) = await AddOrUpdateTruliooDocumentVerificationAsync(userId, response.TransactionId, cancellationToken);

                var clientResponse = await _httpClientService.PostAsync(endpoint: shortcodeURL, jObject: result, cancellationToken: cancellationToken);

                if (!clientResponse.ToLower().Contains("message"))
                {
                    ShortCodeDto code = JsonConvert.DeserializeObject<ShortCodeDto>(clientResponse);
                    return ("Shortcode Found", new InitializeWebSDK() { ShortCode = code.ShortCode, AccessToken = result.AccessToken });
                }

                return ("ShortCode Not Found", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IDVerificationService)}-{nameof(GetShortCodeForSdkAsync)} - ApplicationId : {userId} - TransactionId - {response?.TransactionId} ");
                return ($"Something went wrong", null);
            }
        }

        public async Task<(string, RefreshTokenDto)> GetRefreshTokenAsync(string refreshToken, RefreshTokenContent model, CancellationToken cancellationToken = default)
        {
            try
            {
                var refreshTokenURL = _configuration["TruliooUrl:RefreshToken"];
                if (refreshToken == null)
                {
                    return (string.Empty, null);
                }

                List<KeyValuePair<string, string>> headers = GenerateHeaders(refreshToken, "2.1");

                var clientResponse = await _httpClientService.SendAsync(refreshTokenURL, "POST", headers, model, cancellationToken);
                if (clientResponse != null)
                {
                    RefreshTokenDto refreshTokenResponse = JsonConvert.DeserializeObject<RefreshTokenDto>(clientResponse);
                    return ("Refresh Token Found", refreshTokenResponse);
                }

                return ("Refresh Token Not Found", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IDVerificationService)}-{nameof(GetRefreshTokenAsync)}");
                return ($"Something went wrong", null);
            }
        }

        public async Task<(string, ConfigureTransactionDto)> CreateAndConfigureTransaction(AuthorizationDto model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (model == null)
                {
                    return ("Access Token not found", null);
                }

                var configureTransactionURL = _configuration["TruliooUrl:ConfigureTransaction"];
                var clientResponse = await _httpClientService.PostAsync(endpoint: configureTransactionURL, jObject: model, cancellationToken: cancellationToken);

                if (!clientResponse.ToLower().Contains("message"))
                {
                    return ("Retreive Transaction Id Successfully", JsonConvert.DeserializeObject<ConfigureTransactionDto>(clientResponse));
                }

                return ("Create and Configure new transaciton on Trulio failed", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IDVerificationService)}-{nameof(CreateAndConfigureTransaction)}");
                return ($"Something went wrong", null);
            }
        }

        public async Task<(string, bool)> AddTruliooTransactionStatusAsync(TruliooTransactionStatusDto trulioTransactionStatusDto, CancellationToken cancellationToken = default)
        {
            IDVerificationTransactionStatus trulioTransactionStatusEntity = null;
            try
            {
                if (trulioTransactionStatusDto.Event.Name.ToLower() != "verification")
                {
                    return ("Skipped", true);
                }

                trulioTransactionStatusEntity = _mapper.Map<IDVerificationTransactionStatus>(trulioTransactionStatusDto);

                (_, _) = await _caaryRepositories.IDVerificationRepository.AddEntityAsync(trulioTransactionStatusEntity, cancellationToken);

                (var message, var isUpdate) = await UpdateTruliooDocumentVerificationStatusAsync(trulioTransactionStatusEntity, trulioTransactionStatusDto, cancellationToken);

                return (message, isUpdate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IDVerificationService)}-{nameof(AddTruliooTransactionStatusAsync)} - TransactionId : {trulioTransactionStatusEntity?.TransactionId}");
            }

            return ("Failed", false);
        }

        public async Task<(string, string)> GetTransactionDetailsAsync(TruliooTransactionDetailDto model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (model == null)
                {
                    return ("Please provide the data.", null);
                }

                var transactionDataURL = _configuration["TruliooUrl:TransactionDetail"];
                var response = await _httpClientService.PostAsync(endpoint: transactionDataURL, jObject: model, cancellationToken: cancellationToken);

                _logger.LogInformation($"{nameof(IDVerificationService)}-{nameof(GetTransactionDetailsAsync)} - Response: {response}");
                if (response != null)
                {
                    return (null, response);
                }

                return ("Transaction Not Found.", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Caught exception at {nameof(IDVerificationService)}-{nameof(GetTransactionDetailsAsync)} - TransactionId : {model.TransactionId}");
                return ($"Something went wrong", null);
            }
        }

        public async Task<(string, TransactionDetailsDto)> GetTransactionDetailsByIdAsync(Guid? transactionId, CancellationToken cancellationToken = default)
        {
            try
            {
                (var responseMessage, AuthorizationDto result) = await GetAccessRefreshTokenAsync(cancellationToken);
                if (result == null)
                {
                    return (responseMessage, null);
                }

                var model = new TruliooTransactionDetailDto()
                {
                    AccessToken = result.AccessToken,
                    TransactionId = transactionId,
                };

                var transactionDataURL = _configuration["TruliooUrl:TransactionDetail"];
                var response = await _httpClientService.PostAsync(endpoint: transactionDataURL, jObject: model, cancellationToken: cancellationToken);
                if (!response.ToLower().Contains("message"))
                {
                    if (response.Contains("\\\""))
                    {
                        response = response.Replace("\\", string.Empty);
                    }

                    if (response.StartsWith("\""))
                    {
                        response = response.TrimStart('\"').TrimEnd('\"');
                    }

                    return ("Transaction Retrieved", JsonConvert.DeserializeObject<TransactionDetailsDto>(response));
                }

                return ("Transaction not found", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IDVerificationService)}-{nameof(GetTransactionDetailsAsync)} - TransactionId : {transactionId}");
                return ($"Something went wrong", null);
            }
        }

        public async Task<(string, ImageDocuments)> GetTruliooImageAsync(TruliooImageRequestDto model, CancellationToken cancellationToken = default)
        {
            try
            {
                var imageURL = _configuration["TruliooUrl:Image"];
                var response = await _httpClientService.PostAsync(endpoint: imageURL, jObject: model, cancellationToken: cancellationToken);
                if (!response.ToLower().Contains("message"))
                {
                    return ("Image Retrieved", JsonConvert.DeserializeObject<ImageDocuments>(response));
                }

                return ("Image not found", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IDVerificationService)}-{nameof(GetTruliooImageAsync)}");
                return ($"Something went wrong", null);
            }
        }

        public async Task<(string, object)> RetrieveAndStoreImagesAsync(TruliooImageUploadDataDto model, CancellationToken cancellationToken = default)
        {
            try
            {
                if (model == null)
                {
                    return ("Please provide the model data", null);
                }
                var imageURL = _configuration["TruliooUrl:Image"];
                var contentServiceURL = $"{_configuration["BaseUrl:ContentService"]}/upload";

                (var message, TransactionDetailsDto transactionData) = await GetTransactionDetailsAsync(model, cancellationToken);

                if (!string.IsNullOrEmpty(message))
                {
                    return (message, null);
                }

                _logger.LogInformation($"{nameof(IDVerificationService)}-{nameof(RetrieveAndStoreImagesAsync)} - Saving Images to Blob Storage - UserId: {model.UserId}");

                Dictionary<string, string> imageData = await GetImagesOnRequestAsync(imageURL, transactionData, cancellationToken);

                _logger.LogInformation($"{nameof(IDVerificationService)}-{nameof(RetrieveAndStoreImagesAsync)} - Images Count on Retrieve from Trulioo {imageData.Count} - UserId: {model.UserId}");

                (var imageMessage, ImagesUrlDto imageUrl) = await GetImagesURLFromBlobAsync(contentServiceURL, imageData, model.UserId, transactionData.TransactionId, cancellationToken);

                if (!string.IsNullOrEmpty(imageMessage))
                {
                    return (imageMessage, null);
                }

                (var response, var addOrUpdate) = await AddOrUpdateTruliooImageAsync(imageUrl, model.UserId, transactionData.TransactionId, cancellationToken);

                _logger.LogInformation($"{nameof(IDVerificationService)}-{nameof(RetrieveAndStoreImagesAsync)} - Images Add/Update in tbl_image: {addOrUpdate} - UserId: {model.UserId}");
                if (addOrUpdate)
                {
                    return (null, imageUrl);
                }

                return (response, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Caught exception at {nameof(IDVerificationService)}-{nameof(RetrieveAndStoreImagesAsync)} - ApplicationId : {model.UserId} - TransactionId - {model.TransactionId}");
                return ($"Something went wrong", null);
            }
        }

        public async Task<Dictionary<string, string>> GetImagesOnRequestAsync(string targetUrl, TransactionDetailsDto transactionData, CancellationToken cancellationToken = default)
        {
            var requestModel = new List<TruliooImageRequestDto>
            {
                new TruliooImageRequestDto { TransactionId = transactionData.TransactionId, ImageId = transactionData.Images?.FrontDocumentId, ImageToken = transactionData.Images?.AccessToken },
                new TruliooImageRequestDto { TransactionId = transactionData.TransactionId, ImageId = transactionData.Images?.BackDocumentId, ImageToken = transactionData.Images?.AccessToken },
                new TruliooImageRequestDto { TransactionId = transactionData.TransactionId, ImageId = transactionData.Images?.SelfieId, ImageToken = transactionData.Images?.AccessToken }
            };
            var tasks = new Dictionary<string, Task<string>>();

            foreach (TruliooImageRequestDto model in requestModel)
            {
                if (model.ImageId == null)
                {
                    continue;
                }

                var imageType = string.Empty;

                if (model.ImageId == transactionData.Images?.FrontDocumentId)
                {
                    imageType = "DocumentFrontImage";
                }

                if (model.ImageId == transactionData.Images?.BackDocumentId)
                {
                    imageType = "DocumentBackImage";
                }

                if (model.ImageId == transactionData.Images?.SelfieId)
                {
                    imageType = "SelfieImage";
                }

                tasks.Add(imageType, _httpClientService.PostAsync(endpoint: targetUrl, jObject: model, cancellationToken: cancellationToken));
            }

            if (tasks.Count > 0)
            {
                await Task.WhenAll(tasks.Select(x => x.Value));
            }

            var responses = new Dictionary<string, string>();

            foreach (KeyValuePair<string, Task<string>> task in tasks)
            {
                responses.Add(task.Key, task.Value.Result);
            }

            return responses;
        }

        public async Task<(string, ImagesUrlDto)> GetImagesURLFromBlobAsync(string targetUrl, Dictionary<string, string> imageData, long? userId, Guid? transactionId, CancellationToken cancellationToken = default)
        {
            ImageDocumentsBase64 imageModel = null;
            try
            {
                var parameters = new Dictionary<string, string>
                {
                    { "directory", userId + "/idVerificationDocuments" }
                };

                imageModel = ConvertStringListToModelList(imageData);

                if (imageModel == null)
                {
                    return ("Image's not found", null);
                }

                var file = new List<(byte[], string)>();

                if (imageModel.FrontDocumentBase64 != null)
                {
                    file.Add((Convert.FromBase64String(imageModel.FrontDocumentBase64), "DocumentFrontImage.png"));
                }

                if (imageModel.BackDocumentBase64 != null)
                {
                    file.Add((Convert.FromBase64String(imageModel.BackDocumentBase64), "DocumentBackImage.png"));
                }

                if (imageModel.SelfieDocumentBase64 != null)
                {
                    file.Add((Convert.FromBase64String(imageModel.SelfieDocumentBase64), "SelfieImage.png"));
                }

                var response = await _httpClientService.PostFileAsync(targetUrl, parameters, file, cancellationToken);
                BlobURLDto imageURL = JsonConvert.DeserializeObject<BlobURLDto>(response);

                if (imageURL.Data.Count > 0)
                {
                    var imagesUrlDto = new ImagesUrlDto()
                    {
                        DocumentFrontImage = GetImageUrl(imageURL, "DocumentFrontImage"),
                        DocumentBackImage = GetImageUrl(imageURL, "DocumentBackImage"),
                        SelfieImage = GetImageUrl(imageURL, "SelfieImage"),
                        TransactionId = transactionId
                    };

                    return (null, imagesUrlDto);
                }

                return ("Image Url Not Found", null);

                string GetImageUrl(BlobURLDto blobUrls, string documentType) => blobUrls.Data.FirstOrDefault(x => x.Contains(documentType));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IDVerificationService)}-{nameof(GetImagesURLFromBlobAsync)} - UserId : {userId} - TransactionId - {transactionId}");
                return ($"Something went wrong", null);
            }
        }

        public async Task<(string, object)> GetTruliooImagesAsync(long? userId, CancellationToken cancellationToken = default)
        {
            try
            {
                ImagesUrlDto truliooImages = await GetImageData(userId, cancellationToken);
                if (truliooImages != null)
                {
                    return ("Trulioo Details", truliooImages);
                }

                return ("Trulioo Details not found", null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(IDVerificationService)}-{nameof(GetTruliooImagesAsync)}");
                return ($"Something went wrong", null);
            }
        }

        private async Task<(string, bool)> AddOrUpdateTruliooImageAsync(ImagesUrlDto imagesUrlDto, long? userId, Guid? transactionId, CancellationToken cancellationToken = default)
        {
            IDVerificationImage truliooImageEntity = await _caaryRepositories.IDVerificationRepository.GetTruliooImageAsync(userId, cancellationToken);
            if (truliooImageEntity == null)
            {
                truliooImageEntity = new IDVerificationImage();
            }

            truliooImageEntity = _mapper.Map(imagesUrlDto, truliooImageEntity);
            truliooImageEntity.DocumentTypeId = 1;
            truliooImageEntity.UserId = userId;
            truliooImageEntity.IsDocumentVerified = false;
            truliooImageEntity.IsDocumentMatched = false;
            truliooImageEntity.ServiceStatusId = 4;
            truliooImageEntity.TransactionId = transactionId;

            if (truliooImageEntity?.Id == null || truliooImageEntity.Id == Guid.Empty)
            {
                truliooImageEntity.Id = Guid.NewGuid();
                truliooImageEntity.CreatedDate = DateTime.UtcNow;
                truliooImageEntity.CreatedBy = UserName;
                (_, var isAdded) = await _caaryRepositories.IDVerificationRepository.AddEntityAsync(truliooImageEntity, cancellationToken);
                return (null, isAdded);
            }

            (_, var isUpdated) = await _caaryRepositories.IDVerificationRepository.UpdateEntityAsync(truliooImageEntity, cancellationToken);

            return ("Trulioo image update Successfully.", isUpdated);
        }

        private async Task<(string, bool)> AddOrUpdateTruliooDocumentVerificationAsync(long? userId, Guid? transactionId, CancellationToken cancellationToken = default)
        {
            IDVerificationDocument truliooDocumentVerificationEntity = await _caaryRepositories.IDVerificationRepository.GetDocumentVerificationAsync(userId, cancellationToken);
            if (truliooDocumentVerificationEntity != null)
            {
                return ("Trulioo Document Verification already exists for the existing application.", true);
            }

            truliooDocumentVerificationEntity = new IDVerificationDocument
            {
                TransactionId = transactionId,
                Data = $"{{ \"TransactionId\" : \"{transactionId}\" }}",
                UserId = userId,
                IsDocumentMatched = false,
                Status = "INITIALIZED",
                CreatedDate = DateTime.UtcNow,
                CreatedBy = UserName
            };

            (_, var isAdded) = await _caaryRepositories.IDVerificationRepository.AddEntityAsync(truliooDocumentVerificationEntity, cancellationToken);
            return ("Trulioo Document Verification Status added Successfully.", isAdded);
        }

        private async Task<(string, bool)> UpdateTruliooDocumentVerificationStatusAsync(IDVerificationTransactionStatus trulioTransactionStatusEntity, TruliooTransactionStatusDto trulioTransactionStatusDto, CancellationToken cancellationToken = default)
        {
            IDVerificationDocument truliooDocumentVerificationEntity = await _caaryRepositories.IDVerificationRepository.GetDocumentVerificationByTransactionIdAsync(trulioTransactionStatusEntity.TransactionId, cancellationToken);

            if (truliooDocumentVerificationEntity == null)
            {
                return ("Trulioo Document Data not found", true);
            }

            truliooDocumentVerificationEntity.ModifiedDate = DateTime.UtcNow;
            truliooDocumentVerificationEntity.ModifiedBy = UserName;
            truliooDocumentVerificationEntity.Data = JsonConvert.SerializeObject(trulioTransactionStatusDto);
            truliooDocumentVerificationEntity.Status = trulioTransactionStatusDto.Event.Status == null ? "NONE" : trulioTransactionStatusDto.Event.Status;

            (_, var isUpdated) = await _caaryRepositories.IDVerificationRepository.UpdateEntityAsync(truliooDocumentVerificationEntity, cancellationToken);

            return ("Trulioo Document Verification Status update Successfully.", isUpdated);
        }

        private List<KeyValuePair<string, string>> GenerateHeaders(string token, string version)
        {
            return new List<KeyValuePair<string, string>>()
            {
                new KeyValuePair<string, string>("Authorization", string.Format("Bearer {0}", token)),
                new KeyValuePair<string, string>("Accept-Version", version)
            };
        }

        private ImageDocumentsBase64 ConvertStringListToModelList(Dictionary<string, string> base64Strings)
        {
            if (base64Strings.Count > 0)
            {
                var model = new ImageDocumentsBase64();
                if (base64Strings.Keys.Contains("DocumentFrontImage"))
                {
                    model.FrontDocumentBase64 = JsonConvert.DeserializeObject<ImageDocuments>(base64Strings.Values.First()).Image;
                }

                if (base64Strings.Keys.Contains("DocumentBackImage"))
                {
                    model.BackDocumentBase64 = JsonConvert.DeserializeObject<ImageDocuments>(base64Strings.Values.First()).Image;
                }

                if (base64Strings.Keys.Contains("SelfieImage"))
                {
                    model.SelfieDocumentBase64 = JsonConvert.DeserializeObject<ImageDocuments>(base64Strings.Values.First()).Image;
                }

                return model;
            }

            return null;
        }

        private async Task<ImagesUrlDto> GetImageData(long? userId, CancellationToken cancellationToken = default)
        {
            IDVerificationImage truliooImageDto = await _caaryRepositories.IDVerificationRepository.GetTruliooImageAsync(userId, cancellationToken);
            if (truliooImageDto != null)
            {
                var imagesUrl = new ImagesUrlDto()
                {
                    DocumentFrontImage = truliooImageDto.DocumentFrontImage,
                    DocumentBackImage = truliooImageDto.DocumentBackImage,
                    SelfieImage = truliooImageDto.SelfieImage,
                    TransactionId = truliooImageDto?.TransactionId != null ? truliooImageDto?.TransactionId.Value : null
                };
                return imagesUrl;
            }

            return null;
        }

        private async Task<(string, TransactionDetailsDto)> GetTransactionDetailsAsync(TruliooImageUploadDataDto model, CancellationToken cancellationToken = default)
        {
            try
            {
                var truliooTransactionDetail = new TruliooTransactionDetailDto()
                {
                    TransactionId = model.TransactionId,
                    AccessToken = model.AccessToken
                };
                var blobURL = _configuration["TruliooUrl:Blob"];

                (var message, var transactionData) = await GetTransactionDetailsAsync(truliooTransactionDetail, cancellationToken);

                if (transactionData.Contains("\\\""))
                {
                    transactionData = transactionData.Replace("\\", string.Empty);
                }

                if (transactionData.StartsWith("\""))
                {
                    transactionData = transactionData.TrimStart('\"').TrimEnd('\"');
                }

                TransactionDetailsDto transactionImageData = JsonConvert.DeserializeObject<TransactionDetailsDto>(transactionData);

                if (transactionImageData.Images.FrontDocumentId == null || transactionImageData.Images.BackDocumentId == null || transactionImageData.Images.SelfieId == null)
                {
                    var imagesUrlDto = new ImagesUrlDto()
                    {
                        DocumentFrontImage = blobURL.Replace("{UserId}", model.UserId.ToString()),
                        DocumentBackImage = blobURL.Replace("{UserId}", model.UserId.ToString()),
                        SelfieImage = blobURL.Replace("{UserId}", model.UserId.ToString()),
                    };

                    (_, _) = await AddOrUpdateTruliooImageAsync(imagesUrlDto, model.UserId, transactionImageData.TransactionId, cancellationToken);

                    message = $"Images not Found in Details - UserId: {model.UserId} - TransactionId {transactionImageData?.TransactionId}";

                    _logger.LogInformation($"{nameof(IDVerificationService)}-{nameof(GetTransactionDetailsAsync)} - {message}");
                }

                return (message, transactionImageData);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Caught exception at {nameof(IDVerificationService)}-{nameof(GetTransactionDetailsAsync)}");
                return ($"Something went wrong", null);
            }
        }
    }
}