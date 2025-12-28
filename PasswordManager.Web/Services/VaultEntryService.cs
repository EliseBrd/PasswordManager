using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using PasswordManager.Dto.Vault.Requests;

namespace PasswordManager.Web.Services;

public class VaultEntryService
{
    private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly string _apiBaseUrl;
        private readonly string _apiScope;
        private readonly JsonSerializerOptions _jsonOptions;

        public VaultEntryService(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ITokenAcquisition tokenAcquisition, 
            AuthenticationStateProvider authenticationStateProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tokenAcquisition = tokenAcquisition;
            _authenticationStateProvider = authenticationStateProvider;
            _apiBaseUrl = configuration.GetValue<string>("WebAPI:Endpoint") ?? throw new InvalidOperationException("WebAPI endpoint is not configured");
            _apiScope = configuration.GetValue<string>("WebAPI:Scope") ?? throw new InvalidOperationException("WebAPI scope is not configured");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }
        
        public async Task<string?> GetVaultEntryPasswordAsync(Guid identifier)
        {
            var client = await CreateHttpClientAsync();

            var response = await client.GetFromJsonAsync<JsonElement>(
                $"{_apiBaseUrl}/api/vault/entry/{identifier}/password",
                _jsonOptions);

            return response.TryGetProperty("encryptedPassword", out var prop)
                ? prop.GetString()
                : null;
        }

        private async Task<HttpClient> CreateHttpClientAsync()
        {
            var client = _httpClientFactory.CreateClient("API");
            
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _apiScope }, user: user);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return client;
        }
        
        public  async Task CreateVaultEntryAsync(CreateVaultEntryRequest request)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/vault/entry", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }
        
        public  async Task DeleteVaultEntryAsync(Guid identifier)
        {
            var client = await CreateHttpClientAsync();

            var response = await client.DeleteAsync(
                $"{_apiBaseUrl}/api/vault/entry/{identifier}");

            response.EnsureSuccessStatusCode();
        }
}