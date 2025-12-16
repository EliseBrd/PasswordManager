using Microsoft.Identity.Abstractions;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;

namespace PasswordManager.Web.Services
{
    public class VaultService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _apiBaseUrl;

        public VaultService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _apiBaseUrl = configuration.GetValue<string>("WebAPI:Endpoint") ?? throw new InvalidOperationException("WebAPI endpoint is not configured");
        }

        private async Task<HttpClient> CreateHttpClientAsync()
        {
            var client = _httpClientFactory.CreateClient("API");
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext == null)
            {
                throw new InvalidOperationException("HttpContext is not available.");
            }

            var accessToken = await httpContext.GetTokenAsync("access_token");
            if (string.IsNullOrEmpty(accessToken))
            {
                throw new InvalidOperationException("Access token is not available.");
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }

        public async Task<IEnumerable<VaultSummaryResponse>?> GetAccessibleVaultsAsync()
        {
            var client = await CreateHttpClientAsync();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/vault");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<VaultSummaryResponse>>();
        }

        public async Task CreateVaultAsync(CreateVaultRequest request)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/vault", request);
            response.EnsureSuccessStatusCode();
        }
    }
}
