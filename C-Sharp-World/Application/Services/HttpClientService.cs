using Newtonsoft.Json;
using System.Text;

namespace IdentityVerificationService.Application.Services
{
    public class HttpClientService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<HttpClientService> _logger;

        public HttpClientService(HttpClient httpClient, ILogger<HttpClientService> logger)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException($"{nameof(httpClient)}");
            _logger = logger ?? throw new ArgumentNullException($"{nameof(logger)}");
        }

        public async Task<string> SendAsync(string targetAbsoluteUrl, string methodType, List<KeyValuePair<string, string>> headers = null, object content = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var httpMethod = new HttpMethod(methodType.ToUpper());

                var requestMessage = new HttpRequestMessage(httpMethod, targetAbsoluteUrl);

                if (content != null)
                {
                    var contentBytes = Encoding.UTF8.GetBytes(content.ToString());
                    requestMessage.Content = new ByteArrayContent(contentBytes);
                }

                headers?.ForEach(kvp => { requestMessage.Headers.Add(kvp.Key, kvp.Value); });

                HttpResponseMessage response = await _httpClient.SendAsync(requestMessage, cancellationToken);

                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(HttpClientService)}-{nameof(SendAsync)}");
                return "Something Went Wrong";
            }
        }

        public async Task<string> PostAsync(string endpoint, object jObject, Dictionary<string, string> queryParams = null, CancellationToken cancellationToken = default)
        {
            try
            {
                if (endpoint != null)
                {
                    var json = JsonConvert.SerializeObject(jObject);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    if (queryParams != null)
                    {
                        var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"));

                        endpoint = "?" + queryString;
                    }

                    HttpResponseMessage response = await _httpClient.PostAsync(endpoint, content, cancellationToken);

                    return await response.Content.ReadAsStringAsync();
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(HttpClientService)}-{nameof(PostAsync)}");
                return "Something Went Wrong";
            }
        }

        public async Task<string> PostFileAsync(string baseUrl, Dictionary<string, string> queryParams = null, List<(byte[], string)> file = null, CancellationToken cancellationToken = default)
        {
            try
            {
                var queryString = string.Join("&", queryParams.Select(kv => $"{kv.Key}={kv.Value}"));

                var apiUrl = $"{baseUrl}?{queryString}";

                var content = new MultipartFormDataContent();
                foreach ((byte[], string) item in file)
                {
                    content.Add(new ByteArrayContent(item.Item1), "file", item.Item2);
                }

                HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, content, cancellationToken);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"{nameof(HttpClientService)}-{nameof(PostFileAsync)}");
                return "Something Went Wrong";
            }
        }
    }
}