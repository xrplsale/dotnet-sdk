using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Extensions.Http;

namespace XRPLSale;

/// <summary>
/// Main client for interacting with the XRPL.Sale API
/// </summary>
/// <remarks>
/// The client provides access to all platform services including projects,
/// investments, analytics, webhooks, and authentication.
/// </remarks>
/// <example>
/// <code>
/// var client = new XRPLSaleClient(new XRPLSaleOptions
/// {
///     ApiKey = "your-api-key",
///     Environment = XRPLSaleEnvironment.Production
/// });
/// 
/// var projects = await client.Projects.ListAsync(status: "active");
/// var project = await client.Projects.GetAsync("proj_123");
/// </code>
/// </example>
public class XRPLSaleClient : IDisposable
{
    private readonly HttpClient _httpClient;
    private readonly XRPLSaleOptions _options;
    private readonly ILogger<XRPLSaleClient>? _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private bool _disposed;

    /// <summary>
    /// Authentication token for API requests
    /// </summary>
    public string? AuthToken { get; set; }

    /// <summary>
    /// Projects service for managing token sale projects
    /// </summary>
    public IProjectsService Projects { get; }

    /// <summary>
    /// Investments service for managing investments
    /// </summary>
    public IInvestmentsService Investments { get; }

    /// <summary>
    /// Analytics service for retrieving platform analytics
    /// </summary>
    public IAnalyticsService Analytics { get; }

    /// <summary>
    /// Webhooks service for managing webhook endpoints
    /// </summary>
    public IWebhooksService Webhooks { get; }

    /// <summary>
    /// Authentication service for wallet-based authentication
    /// </summary>
    public IAuthService Auth { get; }

    /// <summary>
    /// Initialize a new XRPL.Sale client
    /// </summary>
    /// <param name="options">Client configuration options</param>
    /// <param name="httpClient">Optional custom HTTP client</param>
    /// <param name="logger">Optional logger instance</param>
    public XRPLSaleClient(XRPLSaleOptions options, HttpClient? httpClient = null, ILogger<XRPLSaleClient>? logger = null)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _logger = logger;

        if (string.IsNullOrEmpty(options.ApiKey))
            throw new ArgumentException("API key is required", nameof(options));

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            WriteIndented = false
        };

        _httpClient = httpClient ?? CreateHttpClient();
        ConfigureHttpClient();

        // Initialize services
        Projects = new ProjectsService(this);
        Investments = new InvestmentsService(this);
        Analytics = new AnalyticsService(this);
        Webhooks = new WebhooksService(this);
        Auth = new AuthService(this);
    }

    /// <summary>
    /// Initialize a new XRPL.Sale client with IOptions pattern
    /// </summary>
    /// <param name="options">Configuration options</param>
    /// <param name="httpClient">Optional custom HTTP client</param>
    /// <param name="logger">Optional logger instance</param>
    public XRPLSaleClient(IOptions<XRPLSaleOptions> options, HttpClient? httpClient = null, ILogger<XRPLSaleClient>? logger = null)
        : this(options.Value, httpClient, logger)
    {
    }

    /// <summary>
    /// Make a GET request to the API
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="queryParams">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response data</returns>
    public async Task<T> GetAsync<T>(string endpoint, Dictionary<string, object>? queryParams = null, CancellationToken cancellationToken = default)
    {
        var url = BuildUrl(endpoint, queryParams);
        _logger?.LogDebug("GET {Url}", url);

        var response = await _httpClient.GetAsync(url, cancellationToken);
        return await HandleResponseAsync<T>(response);
    }

    /// <summary>
    /// Make a POST request to the API
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request body data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response data</returns>
    public async Task<T> PostAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("POST {Endpoint}", endpoint);

        var content = data != null ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json") : null;
        var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
        return await HandleResponseAsync<T>(response);
    }

    /// <summary>
    /// Make a PUT request to the API
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request body data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response data</returns>
    public async Task<T> PutAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("PUT {Endpoint}", endpoint);

        var content = data != null ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json") : null;
        var response = await _httpClient.PutAsync(endpoint, content, cancellationToken);
        return await HandleResponseAsync<T>(response);
    }

    /// <summary>
    /// Make a PATCH request to the API
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="data">Request body data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response data</returns>
    public async Task<T> PatchAsync<T>(string endpoint, object? data = null, CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("PATCH {Endpoint}", endpoint);

        var content = data != null ? new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json") : null;
        var response = await _httpClient.PatchAsync(endpoint, content, cancellationToken);
        return await HandleResponseAsync<T>(response);
    }

    /// <summary>
    /// Make a DELETE request to the API
    /// </summary>
    /// <typeparam name="T">Response type</typeparam>
    /// <param name="endpoint">API endpoint</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Response data</returns>
    public async Task<T> DeleteAsync<T>(string endpoint, CancellationToken cancellationToken = default)
    {
        _logger?.LogDebug("DELETE {Endpoint}", endpoint);

        var response = await _httpClient.DeleteAsync(endpoint, cancellationToken);
        return await HandleResponseAsync<T>(response);
    }

    /// <summary>
    /// Verify a webhook signature
    /// </summary>
    /// <param name="payload">Raw webhook payload</param>
    /// <param name="signature">Signature from X-XRPL-Sale-Signature header</param>
    /// <param name="secret">Custom secret (optional, uses client secret by default)</param>
    /// <returns>True if signature is valid</returns>
    public bool VerifyWebhookSignature(string payload, string signature, string? secret = null)
    {
        var secretToUse = secret ?? _options.WebhookSecret;
        if (string.IsNullOrEmpty(secretToUse))
            return false;

        var expectedSignature = "sha256=" + ComputeHmacSha256(payload, secretToUse);
        return string.Equals(expectedSignature, signature, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parse a webhook event from JSON payload
    /// </summary>
    /// <param name="payload">JSON webhook payload</param>
    /// <returns>Parsed webhook event</returns>
    public WebhookEvent ParseWebhookEvent(string payload)
    {
        return JsonSerializer.Deserialize<WebhookEvent>(payload, _jsonOptions) 
            ?? throw new XRPLSaleException("Failed to parse webhook event");
    }

    private HttpClient CreateHttpClient()
    {
        var retryPolicy = Policy
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .WaitAndRetryAsync(
                retryCount: _options.MaxRetries,
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt - 1) * _options.RetryDelay),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    _logger?.LogWarning("Retry {RetryCount} after {Delay}ms", retryCount, timespan.TotalMilliseconds);
                });

        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(_options.Timeout);
        return client;
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri(_options.BaseUrl ?? GetDefaultBaseUrl());
        _httpClient.DefaultRequestHeaders.Clear();
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("XRPL.Sale-DotNet-SDK", "1.0.0"));

        // Add authentication headers
        if (!string.IsNullOrEmpty(AuthToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthToken);
        }
        else if (!string.IsNullOrEmpty(_options.ApiKey))
        {
            _httpClient.DefaultRequestHeaders.Add("X-API-Key", _options.ApiKey);
        }
    }

    private async Task<T> HandleResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            if (string.IsNullOrEmpty(content))
                return default!;

            return JsonSerializer.Deserialize<T>(content, _jsonOptions)!;
        }

        // Handle specific error types
        var errorResponse = !string.IsNullOrEmpty(content) 
            ? JsonSerializer.Deserialize<ErrorResponse>(content, _jsonOptions)
            : new ErrorResponse { Message = "Unknown error occurred" };

        throw response.StatusCode switch
        {
            System.Net.HttpStatusCode.BadRequest => new XRPLSaleValidationException(errorResponse.Message, errorResponse.Details),
            System.Net.HttpStatusCode.Unauthorized => new XRPLSaleAuthenticationException(errorResponse.Message),
            System.Net.HttpStatusCode.NotFound => new XRPLSaleNotFoundException(errorResponse.Message),
            System.Net.HttpStatusCode.TooManyRequests => new XRPLSaleRateLimitException(errorResponse.Message, GetRetryAfter(response)),
            _ => new XRPLSaleException($"API request failed with status {response.StatusCode}: {errorResponse.Message}")
        };
    }

    private string BuildUrl(string endpoint, Dictionary<string, object>? queryParams)
    {
        if (queryParams == null || !queryParams.Any())
            return endpoint;

        var query = string.Join("&", queryParams.Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value?.ToString() ?? "")}"));
        return $"{endpoint}?{query}";
    }

    private string GetDefaultBaseUrl()
    {
        return _options.Environment switch
        {
            XRPLSaleEnvironment.Testnet => "https://api-testnet.xrpl.sale/v1",
            _ => "https://api.xrpl.sale/v1"
        };
    }

    private static string ComputeHmacSha256(string data, string key)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private static int? GetRetryAfter(HttpResponseMessage response)
    {
        return response.Headers.RetryAfter?.Delta?.Seconds;
    }

    /// <summary>
    /// Dispose of resources
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _httpClient?.Dispose();
            _disposed = true;
        }
    }
}