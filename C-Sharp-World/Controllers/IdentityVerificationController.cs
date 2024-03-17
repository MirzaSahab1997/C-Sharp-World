using IdentityVerificationService.Application.Services;
using IdentityVerificationService.Domain.DTOs.IdVerification.Request;
using IdentityVerificationService.Domain.DTOs.IdVerification;
using IdentityVerificationService.Domain.DTOs.IdVerification.Response;
using Microsoft.AspNetCore.Mvc;

namespace IdentityVerificationService.Controllers
{
    [Route("api/id-verification")]
    [ApiController]

    public class IdentityVerificationController : ControllerBase
    {
        public readonly ILogger<IdentityVerificationController> _logger;
        private readonly IDVerificationService _iDVerificationService;

        public IdentityVerificationController(ILogger<IdentityVerificationController> logger, IDVerificationService iDVerificationService)
        {
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
            _iDVerificationService = iDVerificationService ?? throw new ArgumentNullException($"{nameof(iDVerificationService)}");
        }

        [HttpPost]
        [Route("GetShortCode/{userId}")]
        public async Task<IActionResult> GetShortCodeForTrulioSdkAsync(long? userId, CancellationToken cancellationToken = default)
        {
            if (userId == null)
            {
                return BadRequest("Please provide application Id.");
            }

            (var shortCodeMessage, InitializeWebSDK response) = await _iDVerificationService.GetShortCodeForSdkAsync(userId, cancellationToken);
            if (response == null || shortCodeMessage.Contains("Something went wrong"))
            {
                return BadRequest(shortCodeMessage);
            }

            return Ok(response);
        }

        [HttpPost]
        [Route("CreateAndConfigureTransaction")]
        public async Task<IActionResult> CreateAndConfigureTransaction(AuthorizationDto authorizationDto, CancellationToken cancellationToken = default)
        {
            if (authorizationDto == null || !ModelState.IsValid)
            {
                BadRequest(ModelState);
            }

            (var transactionMessage, ConfigureTransactionDto transactionResponse) = await _iDVerificationService.CreateAndConfigureTransaction(authorizationDto, cancellationToken);
            if (transactionResponse == null || transactionMessage.Contains("Something went wrong"))
            {
                return BadRequest("TransactionId Not Found/Configured.");
            }

            return Ok(transactionResponse);
        }

        [HttpPost]
        [Route("TransactionStatusCallBack")]
        public async Task<IActionResult> AddTruliooTransacationStatusAsync(TruliooTransactionStatusDto truliooTransactionStatusDto, CancellationToken cancellationToken = default)
        {
            if (truliooTransactionStatusDto == null || !ModelState.IsValid)
            {
                BadRequest(ModelState);
            }

            (var message, var result) = await _iDVerificationService.AddTruliooTransactionStatusAsync(truliooTransactionStatusDto, cancellationToken);

            if (result)
            {
                return Ok(message);
            }

            return BadRequest(message);
        }

        [HttpPost]
        [Route("GetTransactionDetails")]
        public async Task<IActionResult> GetTransactionDetailsTruliooAsync(TruliooTransactionDetailDto model, CancellationToken cancellationToken = default)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            (var transactionMessage, var response) = await _iDVerificationService.GetTransactionDetailsAsync(model, cancellationToken);
            if (!string.IsNullOrWhiteSpace(transactionMessage))
            {
                return BadRequest(transactionMessage);
            }

            return Ok(response);
        }

        [HttpGet]
        [Route("GetTransactionDetails/{transactionId}")]
        public async Task<IActionResult> GetTransactionDetailsByIdAsync(Guid? transactionId, CancellationToken cancellationToken = default)
        {
            if (transactionId == null)
            {
                return BadRequest("TransactionId not Found");
            }

            (var transactionMessage, TransactionDetailsDto transactionData) = await _iDVerificationService.GetTransactionDetailsByIdAsync(transactionId, cancellationToken);
            if (transactionData == null || transactionMessage.Contains("Something went wrong"))
            {
                return BadRequest("Transaction Details Not Found.");
            }

            return Ok(transactionData);
        }

        [HttpPost]
        [Route("GetCustomerImage")]
        public async Task<IActionResult> GetImageFromTruliooAsync(TruliooImageRequestDto model, CancellationToken cancellationToken = default)
        {
            if (model == null || !ModelState.IsValid)
            {
                BadRequest(ModelState);
            }

            (var imageMessage, ImageDocuments imageData) = await _iDVerificationService.GetTruliooImageAsync(model, cancellationToken);
            if (imageData == null || imageMessage.Contains("Something went wrong"))
            {
                return BadRequest("Images Not Found.");
            }

            return Ok(imageData);
        }

        [HttpPost]
        [Route("GetAndUploadImages")]
        public async Task<IActionResult> GetAndUploadImagesAsync(TruliooImageUploadDataDto model, CancellationToken cancellationToken = default)
        {
            if (model == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            (var responseMessage, var imageData) = await _iDVerificationService.RetrieveAndStoreImagesAsync(model, cancellationToken);
            if (!string.IsNullOrWhiteSpace(responseMessage))
            {
                return BadRequest(responseMessage);
            }

            return Ok(imageData);
        }

        [HttpGet]
        [Route("GetImages/{userId}")]
        public async Task<IActionResult> GetTruliooImagesAsync(long? userId, CancellationToken cancellationToken = default)
        {
            if (userId == null)
            {
                return BadRequest("Please provide application Id.");
            }

            (var responseMessage, var imageData) = await _iDVerificationService.GetTruliooImagesAsync(userId, cancellationToken);
            if (responseMessage.Contains("Something went wrong"))
            {
                return BadRequest("Images Not Found.");
            }

            return Ok(imageData);
        }
    }
}
