# XRPL.Sale .NET SDK

Official .NET SDK for integrating with the XRPL.Sale platform - the native XRPL launchpad for token sales and project funding.

[![NuGet Version](https://img.shields.io/nuget/v/XRPLSale.svg)](https://www.nuget.org/packages/XRPLSale/)
[![.NET](https://img.shields.io/badge/.NET-8.0+-purple.svg)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

## Features

- 🚀 **Modern .NET 8+** - Built with latest .NET features and C# 12
- 🏗️ **ASP.NET Core Integration** - Seamless integration with ASP.NET Core applications
- 🔐 **XRPL Wallet Authentication** - Wallet-based authentication support
- 📊 **Project Management** - Create, launch, and manage token sales
- 💰 **Investment Tracking** - Monitor investments and analytics
- 🔔 **Webhook Support** - Real-time event notifications with signature verification
- 📈 **Analytics & Reporting** - Comprehensive data insights
- 🛡️ **Structured Exceptions** - Type-safe error handling
- 🔄 **Auto-retry Logic** - Resilient API calls with Polly
- ⚡ **Async/Await** - Full async support throughout
- 🧩 **Dependency Injection** - Native DI container integration
- 📝 **Source Generators** - AOT-compatible with minimal APIs

## Installation

Install the package via NuGet Package Manager:

```bash
dotnet add package XRPLSale
```

Or via Package Manager Console:

```powershell
Install-Package XRPLSale
```

## Quick Start

### Basic Usage

```csharp
using XRPLSale;

// Initialize the client
var client = new XRPLSaleClient(new XRPLSaleOptions
{
    ApiKey = "your-api-key",
    Environment = XRPLSaleEnvironment.Production
});

// Create a new project
var project = await client.Projects.CreateAsync(new CreateProjectRequest
{
    Name = "My DeFi Protocol",
    Description = "Revolutionary DeFi protocol on XRPL",
    TokenSymbol = "MDP",
    TotalSupply = "100000000",
    Tiers = new List<ProjectTier>
    {
        new()
        {
            Tier = 1,
            PricePerToken = "0.001",
            TotalTokens = "20000000"
        }
    },
    SaleStartDate = DateTimeOffset.Parse("2025-02-01T00:00:00Z"),
    SaleEndDate = DateTimeOffset.Parse("2025-03-01T00:00:00Z")
});

Console.WriteLine($"Project created: {project.Id}");
```

### ASP.NET Core Integration

Add to your `Program.cs`:

```csharp
using XRPLSale.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add XRPL.Sale services
builder.Services.AddXRPLSale(options =>
{
    options.ApiKey = builder.Configuration["XRPLSale:ApiKey"];
    options.Environment = XRPLSaleEnvironment.Production;
    options.WebhookSecret = builder.Configuration["XRPLSale:WebhookSecret"];
});

// Add webhook handling
builder.Services.AddXRPLSaleWebhooks(options =>
{
    options.Secret = builder.Configuration["XRPLSale:WebhookSecret"];
    options.Path = "/webhooks/xrplsale";
});

var app = builder.Build();

// Configure webhook endpoint
app.MapPost("/webhooks/xrplsale", async (WebhookEvent webhookEvent, ILogger<Program> logger) =>
{
    logger.LogInformation("Received webhook: {EventType}", webhookEvent.Type);
    
    return webhookEvent.Type switch
    {
        "investment.created" => HandleInvestmentCreated(webhookEvent.Data),
        "project.launched" => HandleProjectLaunched(webhookEvent.Data),
        "tier.completed" => HandleTierCompleted(webhookEvent.Data),
        _ => Results.Ok()
    };
});

app.Run();
```

Use in your controllers and services:

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly XRPLSaleClient _xrplSaleClient;

    public ProjectsController(XRPLSaleClient xrplSaleClient)
    {
        _xrplSaleClient = xrplSaleClient;
    }

    [HttpGet]
    public async Task<ActionResult<List<Project>>> GetActiveProjects()
    {
        try
        {
            var projects = await _xrplSaleClient.Projects.ActiveAsync(page: 1, limit: 10);
            return Ok(projects.Data);
        }
        catch (XRPLSaleException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(string id)
    {
        try
        {
            var project = await _xrplSaleClient.Projects.GetAsync(id);
            return Ok(project);
        }
        catch (XRPLSaleNotFoundException)
        {
            return NotFound();
        }
    }
}
```

### Configuration via appsettings.json

```json
{
  "XRPLSale": {
    "ApiKey": "your-api-key",
    "Environment": "Production",
    "WebhookSecret": "your-webhook-secret",
    "Timeout": 30,
    "MaxRetries": 3,
    "Debug": false
  }
}
```

Then configure with IConfiguration:

```csharp
builder.Services.AddXRPLSale(builder.Configuration.GetSection("XRPLSale"));
```

## Authentication

### XRPL Wallet Authentication

```csharp
// Generate authentication challenge
var challenge = await client.Auth.GenerateChallengeAsync("rYourWalletAddress...");

// Sign the challenge with your wallet
// (implementation depends on your wallet library)
var signature = SignMessage(challenge.Challenge);

// Authenticate
var authResponse = await client.Auth.AuthenticateAsync(new AuthenticateRequest
{
    WalletAddress = "rYourWalletAddress...",
    Signature = signature,
    Timestamp = challenge.Timestamp
});

Console.WriteLine($"Authentication successful: {authResponse.Token}");

// Set the auth token for subsequent requests
client.AuthToken = authResponse.Token;
```

## Core Services

### Projects Service

```csharp
// List active projects
var projects = await client.Projects.ActiveAsync(page: 1, limit: 10);

// Get project details
var project = await client.Projects.GetAsync("proj_abc123");

// Launch a project
await client.Projects.LaunchAsync("proj_abc123");

// Get project statistics
var stats = await client.Projects.GetStatsAsync("proj_abc123");
Console.WriteLine($"Total raised: {stats.TotalRaisedXrp} XRP");

// Search projects
var results = await client.Projects.SearchAsync("DeFi", status: "active");

// Get trending projects
var trending = await client.Projects.GetTrendingAsync(period: "24h", limit: 5);
```

### Investments Service

```csharp
// Create an investment
var investment = await client.Investments.CreateAsync(new CreateInvestmentRequest
{
    ProjectId = "proj_abc123",
    AmountXrp = "100",
    InvestorAccount = "rInvestorAddress..."
});

// List investments for a project
var investments = await client.Investments.GetByProjectAsync("proj_abc123", page: 1, limit: 10);

// Get investor summary
var summary = await client.Investments.GetInvestorSummaryAsync("rInvestorAddress...");

// Simulate an investment
var simulation = await client.Investments.SimulateAsync(new SimulateInvestmentRequest
{
    ProjectId = "proj_abc123",
    AmountXrp = "100"
});
Console.WriteLine($"Expected tokens: {simulation.TokenAmount}");
```

### Analytics Service

```csharp
// Get platform analytics
var analytics = await client.Analytics.GetPlatformAnalyticsAsync();
Console.WriteLine($"Total raised: {analytics.TotalRaisedXrp} XRP");

// Get project-specific analytics
var projectAnalytics = await client.Analytics.GetProjectAnalyticsAsync(
    "proj_abc123",
    startDate: DateOnly.Parse("2025-01-01"),
    endDate: DateOnly.Parse("2025-01-31")
);

// Get market trends
var trends = await client.Analytics.GetMarketTrendsAsync("30d");

// Export data
var export = await client.Analytics.ExportAsync(new ExportRequest
{
    Type = "projects",
    Format = "csv",
    StartDate = DateOnly.Parse("2025-01-01"),
    EndDate = DateOnly.Parse("2025-01-31")
});
Console.WriteLine($"Download URL: {export.DownloadUrl}");
```

## Webhook Integration

### ASP.NET Core Minimal API

```csharp
app.MapPost("/webhooks/xrplsale", async (
    HttpContext context,
    WebhookSignatureValidator validator,
    ILogger<Program> logger) =>
{
    var payload = await new StreamReader(context.Request.Body).ReadToEndAsync();
    var signature = context.Request.Headers["X-XRPL-Sale-Signature"].FirstOrDefault();

    if (!validator.IsValid(payload, signature))
    {
        return Results.Unauthorized();
    }

    var webhookEvent = JsonSerializer.Deserialize<WebhookEvent>(payload);

    return webhookEvent?.Type switch
    {
        "investment.created" => await HandleInvestmentCreated(webhookEvent.Data),
        "project.launched" => await HandleProjectLaunched(webhookEvent.Data),
        "tier.completed" => await HandleTierCompleted(webhookEvent.Data),
        _ => Results.Ok()
    };
});

async Task<IResult> HandleInvestmentCreated(JsonElement data)
{
    var investment = data.Deserialize<InvestmentCreatedEvent>();
    logger.LogInformation("New investment: {Amount} XRP", investment?.AmountXrp);
    
    // Send confirmation email
    // await emailService.SendConfirmationAsync(investment);
    
    return Results.Ok();
}
```

### Controller-based Webhooks

```csharp
[ApiController]
[Route("webhooks")]
public class WebhooksController : ControllerBase
{
    private readonly XRPLSaleClient _client;
    private readonly ILogger<WebhooksController> _logger;

    public WebhooksController(XRPLSaleClient client, ILogger<WebhooksController> logger)
    {
        _client = client;
        _logger = logger;
    }

    [HttpPost("xrplsale")]
    public async Task<IActionResult> HandleXRPLSaleWebhook()
    {
        var payload = await Request.GetRawBodyStringAsync();
        var signature = Request.Headers["X-XRPL-Sale-Signature"].FirstOrDefault();

        if (!_client.VerifyWebhookSignature(payload, signature))
        {
            return Unauthorized("Invalid signature");
        }

        var webhookEvent = _client.ParseWebhookEvent(payload);

        switch (webhookEvent.Type)
        {
            case "investment.created":
                await HandleInvestmentCreated(webhookEvent.Data);
                break;
            case "project.launched":
                await HandleProjectLaunched(webhookEvent.Data);
                break;
            case "tier.completed":
                await HandleTierCompleted(webhookEvent.Data);
                break;
        }

        return Ok();
    }

    private async Task HandleInvestmentCreated(JsonElement data)
    {
        var investment = data.Deserialize<InvestmentCreatedEvent>();
        _logger.LogInformation("New investment: {Amount} XRP", investment?.AmountXrp);
    }
}
```

### Background Service Integration

```csharp
public class WebhookProcessingService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<WebhookProcessingService> _logger;

    public WebhookProcessingService(IServiceProvider serviceProvider, ILogger<WebhookProcessingService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await using var scope = _serviceProvider.CreateAsyncScope();
        var client = scope.ServiceProvider.GetRequiredService<XRPLSaleClient>();

        // Subscribe to webhook events
        // Implementation depends on your event processing strategy
    }
}
```

## Error Handling

```csharp
try
{
    var project = await client.Projects.GetAsync("invalid-id");
}
catch (XRPLSaleNotFoundException)
{
    Console.WriteLine("Project not found");
}
catch (XRPLSaleAuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (XRPLSaleValidationException ex)
{
    Console.WriteLine($"Validation error: {ex.Message}");
    if (ex.Details != null)
    {
        foreach (var detail in ex.Details)
        {
            Console.WriteLine($"  {detail.Field}: {detail.Message}");
        }
    }
}
catch (XRPLSaleRateLimitException ex)
{
    Console.WriteLine($"Rate limit exceeded. Retry after: {ex.RetryAfter} seconds");
}
catch (XRPLSaleException ex)
{
    Console.WriteLine($"API error: {ex.Message}");
}
```

## Configuration Options

```csharp
var client = new XRPLSaleClient(new XRPLSaleOptions
{
    ApiKey = "your-api-key",              // Required
    Environment = XRPLSaleEnvironment.Production, // Production or Testnet
    BaseUrl = "https://custom-api.com",   // Custom API URL (optional)
    Timeout = 30,                         // Request timeout in seconds
    MaxRetries = 3,                       // Maximum retry attempts
    RetryDelay = 1,                       // Base delay between retries
    WebhookSecret = "your-secret",        // For webhook verification
    Debug = false                         // Enable debug logging
});
```

## Pagination

```csharp
// Manual pagination
var response = await client.Projects.ListAsync(
    status: "active",
    page: 1,
    limit: 50,
    sortBy: "created_at",
    sortOrder: "desc"
);

foreach (var project in response.Data)
{
    Console.WriteLine($"Project: {project.Name}");
}

Console.WriteLine($"Page {response.Pagination.Page} of {response.Pagination.TotalPages}");
Console.WriteLine($"Total projects: {response.Pagination.Total}");

// Automatic pagination with async enumerable
await foreach (var project in client.Projects.GetAllAsync(status: "active"))
{
    Console.WriteLine($"Project: {project.Name}");
}
```

## Testing

```csharp
// Use TestServer for integration tests
public class XRPLSaleIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public XRPLSaleIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetProjects_ReturnsSuccessStatusCode()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/projects");

        // Assert
        response.EnsureSuccessStatusCode();
    }
}

// Mock the XRPL.Sale client for unit tests
public class ProjectsServiceTests
{
    [Fact]
    public async Task GetProject_ReturnsProject()
    {
        // Arrange
        var mockClient = new Mock<XRPLSaleClient>();
        var expectedProject = new Project { Id = "proj_123", Name = "Test Project" };
        
        mockClient.Setup(x => x.Projects.GetAsync("proj_123", It.IsAny<CancellationToken>()))
                  .ReturnsAsync(expectedProject);

        // Act
        var result = await mockClient.Object.Projects.GetAsync("proj_123");

        // Assert
        Assert.Equal("proj_123", result.Id);
        Assert.Equal("Test Project", result.Name);
    }
}
```

## Building and Testing

```bash
# Restore packages
dotnet restore

# Build the project
dotnet build

# Run tests
dotnet test

# Pack NuGet package
dotnet pack --configuration Release

# Install locally
dotnet add package XRPLSale --source ./bin/Release
```

## Examples

Check out the [examples directory](https://github.com/xrplsale/dotnet-sdk/tree/main/examples) for complete sample applications:

- **Console Application** - Basic CLI integration
- **ASP.NET Core Web API** - REST API with XRPL.Sale integration
- **Blazor Server App** - Real-time dashboard
- **Worker Service** - Background processing
- **Minimal API** - Lightweight webhook handling

## Support

- 📖 [Documentation](https://xrpl.sale/docs)
- 💬 [Discord Community](https://discord.gg/xrpl-sale)
- 🐛 [Issue Tracker](https://github.com/xrplsale/dotnet-sdk/issues)
- 📧 [Email Support](mailto:developers@xrpl.sale)

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Links

- [XRPL.Sale Platform](https://xrpl.sale)
- [API Documentation](https://xrpl.sale/docs/api)
- [Other SDKs](https://xrpl.sale/docs/developers/sdk-downloads)
- [GitHub Organization](https://github.com/xrplsale)

---

Made with ❤️ by the XRPL.Sale team