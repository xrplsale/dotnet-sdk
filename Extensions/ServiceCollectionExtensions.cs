using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace XRPLSale.Extensions;

/// <summary>
/// Extension methods for configuring XRPL.Sale services in dependency injection container
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add XRPL.Sale client to the service collection
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configuration">Configuration section for XRPL.Sale options</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddXRPLSale(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<XRPLSaleOptions>(configuration);
        services.AddSingleton<XRPLSaleClient>();
        services.AddHttpClient<XRPLSaleClient>();
        
        return services;
    }

    /// <summary>
    /// Add XRPL.Sale client to the service collection with configuration delegate
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration delegate</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddXRPLSale(this IServiceCollection services, Action<XRPLSaleOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<XRPLSaleClient>();
        services.AddHttpClient<XRPLSaleClient>();
        
        return services;
    }

    /// <summary>
    /// Add XRPL.Sale client to the service collection with specific options
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="options">XRPL.Sale options</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddXRPLSale(this IServiceCollection services, XRPLSaleOptions options)
    {
        services.AddSingleton(Options.Create(options));
        services.AddSingleton<XRPLSaleClient>();
        services.AddHttpClient<XRPLSaleClient>();
        
        return services;
    }

    /// <summary>
    /// Add XRPL.Sale services as scoped services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Configuration delegate</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddXRPLSaleScoped(this IServiceCollection services, Action<XRPLSaleOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddScoped<XRPLSaleClient>();
        services.AddHttpClient<XRPLSaleClient>();
        
        return services;
    }

    /// <summary>
    /// Add XRPL.Sale webhook middleware services
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="configureOptions">Webhook configuration delegate</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddXRPLSaleWebhooks(this IServiceCollection services, Action<XRPLSaleWebhookOptions>? configureOptions = null)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }

        services.TryAddSingleton<IWebhookEventHandler, DefaultWebhookEventHandler>();
        services.AddScoped<WebhookSignatureValidator>();
        
        return services;
    }
}

/// <summary>
/// Configuration options for XRPL.Sale webhooks
/// </summary>
public class XRPLSaleWebhookOptions
{
    /// <summary>
    /// Webhook secret for signature verification
    /// </summary>
    public string? Secret { get; set; }

    /// <summary>
    /// Webhook endpoint path
    /// </summary>
    public string Path { get; set; } = "/webhooks/xrplsale";

    /// <summary>
    /// Enable automatic webhook signature verification
    /// </summary>
    public bool VerifySignatures { get; set; } = true;

    /// <summary>
    /// Tolerated timestamp drift in seconds
    /// </summary>
    public int TimestampTolerance { get; set; } = 300; // 5 minutes
}