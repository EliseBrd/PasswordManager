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
using PasswordManager.Dto.User;

namespace PasswordManager.Web.Services
{
    public class VaultService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ITokenAcquisition _tokenAcquisition;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly string _apiBaseUrl;
        private readonly string _apiScope;
        private readonly JsonSerializerOptions _jsonOptions;

        public VaultService(
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

        private async Task<HttpClient> CreateHttpClientAsync()
        {
            var client = _httpClientFactory.CreateClient("API");
            
            var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
            var user = authState.User;

            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { _apiScope }, user: user);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
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

        public async Task<VaultUnlockResponse?> UnlockVaultAsync(string vaultId, string password)
        {
            var client = await CreateHttpClientAsync();
            var request = new AccessVaultRequest { Password = password };
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/vault/{vaultId}/unlock", request, _jsonOptions);
            
            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<VaultUnlockResponse>(_jsonOptions);
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

        public async Task<IEnumerable<UserSummaryResponse>> SearchUsersAsync(string query)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.GetAsync($"{_apiBaseUrl}/api/user/search?query={query}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<IEnumerable<UserSummaryResponse>>(_jsonOptions) ?? new List<UserSummaryResponse>();
        }

        public async Task<bool> UpdateVaultSharingAsync(string vaultId, bool isShared)
        {
            var client = await CreateHttpClientAsync();
            var request = new UpdateVaultSharingRequest { IsShared = isShared };
            var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/vault/{vaultId}/sharing", request, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ShareVaultAsync(string vaultId)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.PostAsync($"{_apiBaseUrl}/api/vault/{vaultId}/share", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> AddUserToVaultAsync(string vaultId, Guid userId)
        {
            var client = await CreateHttpClientAsync();
            var request = new AddUserToVaultRequest { UserId = userId };
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/vault/{vaultId}/users", request, _jsonOptions);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> RemoveUserFromVaultAsync(string vaultId, Guid userId)
        {
            var client = await CreateHttpClientAsync();
            var response = await client.DeleteAsync($"{_apiBaseUrl}/api/vault/{vaultId}/users/{userId}");
            return response.IsSuccessStatusCode;
        }
    }
}
