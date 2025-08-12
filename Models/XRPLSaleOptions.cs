namespace XRPLSale;

/// <summary>
/// Configuration options for the XRPL.Sale client
/// </summary>
public class XRPLSaleOptions
{
    /// <summary>
    /// API key for authentication (required)
    /// </summary>
    public string? ApiKey { get; set; }

    /// <summary>
    /// API environment (Production or Testnet)
    /// </summary>
    public XRPLSaleEnvironment Environment { get; set; } = XRPLSaleEnvironment.Production;

    /// <summary>
    /// Custom base URL for the API (optional)
    /// </summary>
    public string? BaseUrl { get; set; }

    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int Timeout { get; set; } = 30;

    /// <summary>
    /// Maximum retry attempts for failed requests
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Base delay between retries in seconds
    /// </summary>
    public int RetryDelay { get; set; } = 1;

    /// <summary>
    /// Webhook secret for signature verification
    /// </summary>
    public string? WebhookSecret { get; set; }

    /// <summary>
    /// Enable debug logging
    /// </summary>
    public bool Debug { get; set; }
}

/// <summary>
/// XRPL.Sale API environments
/// </summary>
public enum XRPLSaleEnvironment
{
    /// <summary>
    /// Production environment
    /// </summary>
    Production,

    /// <summary>
    /// Testnet environment for testing
    /// </summary>
    Testnet
}