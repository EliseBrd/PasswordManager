using Microsoft.Identity.Abstractions;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using PasswordManager.Dto.Vault;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Components;

namespace PasswordManager.Web.Services
{
    public class VaultService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly NavigationManager _navigation;
        private readonly string _apiBaseUrl;
        private readonly string _apiScope;
        private readonly JsonSerializerOptions _jsonOptions;

        public VaultService(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration, 
            ITokenAcquisition tokenAcquisition, 
            AuthenticationStateProvider authenticationStateProvider,
            NavigationManager navigation)
        {
            _httpClientFactory = httpClientFactory;
            _tokenAcquisition = tokenAcquisition;
            _authenticationStateProvider = authenticationStateProvider;
            _navigation = navigation;
            _apiBaseUrl = configuration.GetValue<string>("WebAPI:Endpoint") ?? throw new InvalidOperationException("WebAPI endpoint is not configured");
            _apiScope = configuration.GetValue<string>("WebAPI:Scope") ?? throw new InvalidOperationException("WebAPI scope is not configured");
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private async Task<HttpClient> CreateHttpClientAsync()
        {
            var client = _httpClientFactory.CreateClient("API");
            
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            if (user.Identity?.IsAuthenticated ?? false)
            {
                try
                {
                    var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _apiScope }, user: user);
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                }
                catch (MicrosoftIdentityWebChallengeUserException ex)
                {
                    // This exception means the user needs to re-authenticate.
                    // In Blazor Server, we need to manually redirect.
                    _navigation.NavigateTo($"/MicrosoftIdentity/Account/SignIn", forceLoad: true);
                }
            }

            return client;
        }

        public async Task<IEnumerable<VaultSummaryResponse>?> GetAccessibleVaultsAsync()
        {
            var client = await CreateHttpClientAsync();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/vault");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<VaultSummaryResponse>>(_jsonOptions);
        }

        public async Task<VaultDetailsResponse?> GetVaultDetailsAsync(string vaultId)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/vault/{vaultId}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<VaultDetailsResponse>(_jsonOptions);
        }

        public async Task<string?> GetVaultEntryPasswordAsync(int entryId)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.GetFromJsonAsync<JsonElement>($"{_apiBaseUrl}/api/vault/entry/{entryId}/password", _jsonOptions);
            return response.TryGetProperty("encryptedPassword", out var prop) ? prop.GetString() : null;
        }

        public async Task CreateVaultAsync(CreateVaultRequest request)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/vault", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task CreateVaultEntryAsync(CreateVaultEntryRequest request)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/vault/entry", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
        }

        public async Task ShareVaultAsync(string vaultId)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.PostAsync($"{_apiBaseUrl}/api/vault/{vaultId}/share", null);
            response.EnsureSuccessStatusCode();
        }
    }
}
