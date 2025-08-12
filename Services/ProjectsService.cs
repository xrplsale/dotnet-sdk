namespace XRPLSale;

/// <summary>
/// Interface for the Projects service
/// </summary>
public interface IProjectsService
{
    Task<PaginatedResponse<Project>> ListAsync(string? status = null, int page = 1, int limit = 10, string? sortBy = null, string? sortOrder = null, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Project>> ActiveAsync(int page = 1, int limit = 10, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Project>> UpcomingAsync(int page = 1, int limit = 10, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Project>> CompletedAsync(int page = 1, int limit = 10, CancellationToken cancellationToken = default);
    Task<Project> GetAsync(string projectId, CancellationToken cancellationToken = default);
    Task<Project> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Project> UpdateAsync(string projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default);
    Task<Project> LaunchAsync(string projectId, CancellationToken cancellationToken = default);
    Task<Project> PauseAsync(string projectId, CancellationToken cancellationToken = default);
    Task<Project> ResumeAsync(string projectId, CancellationToken cancellationToken = default);
    Task<Project> CancelAsync(string projectId, CancellationToken cancellationToken = default);
    Task<ProjectStats> GetStatsAsync(string projectId, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Investment>> GetInvestorsAsync(string projectId, int page = 1, int limit = 10, CancellationToken cancellationToken = default);
    Task<List<ProjectTier>> GetTiersAsync(string projectId, CancellationToken cancellationToken = default);
    Task<List<ProjectTier>> UpdateTiersAsync(string projectId, List<ProjectTier> tiers, CancellationToken cancellationToken = default);
    Task<PaginatedResponse<Project>> SearchAsync(string query, string? status = null, int page = 1, int limit = 10, CancellationToken cancellationToken = default);
    Task<List<Project>> GetFeaturedAsync(int limit = 5, CancellationToken cancellationToken = default);
    Task<List<Project>> GetTrendingAsync(string period = "24h", int limit = 10, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service for managing token sale projects
/// </summary>
/// <remarks>
/// This service provides methods for creating, updating, launching, and managing
/// token sale projects on the XRPL.Sale platform. It also includes functionality
/// for retrieving project statistics, investors, and tier information.
/// </remarks>
public class ProjectsService : IProjectsService
{
    private readonly XRPLSaleClient _client;

    public ProjectsService(XRPLSaleClient client)
    {
        _client = client;
    }

    /// <summary>
    /// List all projects with optional filtering and pagination
    /// </summary>
    /// <param name="status">Filter by project status</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="limit">Number of items per page</param>
    /// <param name="sortBy">Field to sort by</param>
    /// <param name="sortOrder">Sort order (asc or desc)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated response with project data</returns>
    public async Task<PaginatedResponse<Project>> ListAsync(
        string? status = null,
        int page = 1,
        int limit = 10,
        string? sortBy = null,
        string? sortOrder = null,
        CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object>
        {
            ["page"] = page,
            ["limit"] = limit
        };

        if (!string.IsNullOrEmpty(status))
            queryParams["status"] = status;
        if (!string.IsNullOrEmpty(sortBy))
            queryParams["sort_by"] = sortBy;
        if (!string.IsNullOrEmpty(sortOrder))
            queryParams["sort_order"] = sortOrder;

        return await _client.GetAsync<PaginatedResponse<Project>>("/projects", queryParams, cancellationToken);
    }

    /// <summary>
    /// Get active projects
    /// </summary>
    public async Task<PaginatedResponse<Project>> ActiveAsync(int page = 1, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await ListAsync("active", page, limit, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get upcoming projects
    /// </summary>
    public async Task<PaginatedResponse<Project>> UpcomingAsync(int page = 1, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await ListAsync("upcoming", page, limit, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get completed projects
    /// </summary>
    public async Task<PaginatedResponse<Project>> CompletedAsync(int page = 1, int limit = 10, CancellationToken cancellationToken = default)
    {
        return await ListAsync("completed", page, limit, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get a specific project by ID
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Project data</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    public async Task<Project> GetAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<Project>($"/projects/{projectId}", cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    /// <param name="request">Project creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created project data</returns>
    /// <exception cref="XRPLSaleValidationException">When validation fails</exception>
    public async Task<Project> CreateAsync(CreateProjectRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<Project>("/projects", request, cancellationToken);
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="request">Project update data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated project data</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    /// <exception cref="XRPLSaleValidationException">When validation fails</exception>
    public async Task<Project> UpdateAsync(string projectId, UpdateProjectRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.PatchAsync<Project>($"/projects/{projectId}", request, cancellationToken);
    }

    /// <summary>
    /// Launch a project (make it active)
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated project data</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    public async Task<Project> LaunchAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<Project>($"/projects/{projectId}/launch", cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Pause a project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated project data</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    public async Task<Project> PauseAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<Project>($"/projects/{projectId}/pause", cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Resume a paused project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated project data</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    public async Task<Project> ResumeAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<Project>($"/projects/{projectId}/resume", cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Cancel a project
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated project data</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    public async Task<Project> CancelAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await _client.PostAsync<Project>($"/projects/{projectId}/cancel", cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get project statistics
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Project statistics</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    public async Task<ProjectStats> GetStatsAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<ProjectStats>($"/projects/{projectId}/stats", cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Get project investors
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="limit">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated response with investor data</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    public async Task<PaginatedResponse<Investment>> GetInvestorsAsync(string projectId, int page = 1, int limit = 10, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object>
        {
            ["page"] = page,
            ["limit"] = limit
        };

        return await _client.GetAsync<PaginatedResponse<Investment>>($"/projects/{projectId}/investors", queryParams, cancellationToken);
    }

    /// <summary>
    /// Get project tiers
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Project tiers</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    public async Task<List<ProjectTier>> GetTiersAsync(string projectId, CancellationToken cancellationToken = default)
    {
        return await _client.GetAsync<List<ProjectTier>>($"/projects/{projectId}/tiers", cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Update project tiers
    /// </summary>
    /// <param name="projectId">The project ID</param>
    /// <param name="tiers">New tier configuration</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated tiers</returns>
    /// <exception cref="XRPLSaleNotFoundException">When project doesn't exist</exception>
    /// <exception cref="XRPLSaleValidationException">When validation fails</exception>
    public async Task<List<ProjectTier>> UpdateTiersAsync(string projectId, List<ProjectTier> tiers, CancellationToken cancellationToken = default)
    {
        return await _client.PutAsync<List<ProjectTier>>($"/projects/{projectId}/tiers", new { tiers }, cancellationToken);
    }

    /// <summary>
    /// Search projects
    /// </summary>
    /// <param name="query">Search query</param>
    /// <param name="status">Filter by status</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="limit">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated response with matching projects</returns>
    public async Task<PaginatedResponse<Project>> SearchAsync(string query, string? status = null, int page = 1, int limit = 10, CancellationToken cancellationToken = default)
    {
        var queryParams = new Dictionary<string, object>
        {
            ["q"] = query,
            ["page"] = page,
            ["limit"] = limit
        };

        if (!string.IsNullOrEmpty(status))
            queryParams["status"] = status;

        return await _client.GetAsync<PaginatedResponse<Project>>("/projects/search", queryParams, cancellationToken);
    }

    /// <summary>
    /// Get featured projects
    /// </summary>
    /// <param name="limit">Maximum number of projects to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Featured projects</returns>
    public async Task<List<Project>> GetFeaturedAsync(int limit = 5, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync<PaginatedResponse<Project>>("/projects/featured", 
            new Dictionary<string, object> { ["limit"] = limit }, cancellationToken);
        return response.Data ?? new List<Project>();
    }

    /// <summary>
    /// Get trending projects
    /// </summary>
    /// <param name="period">Time period (24h, 7d, 30d)</param>
    /// <param name="limit">Maximum number of projects to return</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Trending projects</returns>
    public async Task<List<Project>> GetTrendingAsync(string period = "24h", int limit = 10, CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync<PaginatedResponse<Project>>("/projects/trending", 
            new Dictionary<string, object> { ["period"] = period, ["limit"] = limit }, cancellationToken);
        return response.Data ?? new List<Project>();
    }
}