using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Identity.Web;
using PasswordManager.Dto.VaultEntries.Requests;
using PasswordManager.Dto.VaultsEntries.Requests;

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
        
        public async Task<string?> GetEntryPasswordAsync(Guid identifier, string encryptedLog)
        {
            var client = await CreateHttpClientAsync();

            var request = new GetVaultEntryPasswordRequest
            {
                EntryIdentifier = identifier,
                EncryptedLog = encryptedLog
            };

            var response = await client.PostAsJsonAsync(
                $"{_apiBaseUrl}/api/VaultEntry/{identifier}/password/access",
                request,
                _jsonOptions);

            if (!response.IsSuccessStatusCode) return null;

            var content = await response.Content.ReadFromJsonAsync<JsonElement>(_jsonOptions);
            return content.TryGetProperty("encryptedPassword", out var prop)
                ? prop.GetString()
                : null;
        }

    
        public async Task<Guid> CreateEntryAsync(CreateVaultEntryRequest request)
        { 
            var client = await CreateHttpClientAsync();
            var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/VaultEntry", request, _jsonOptions);
            response.EnsureSuccessStatusCode();
            
            // L'API retourne le GUID directement en JSON
            var createdId = await response.Content.ReadFromJsonAsync<Guid>(_jsonOptions);
            return createdId;
        }
        
        public async Task UpdateVaultEntryAsync(UpdateVaultEntryRequest request)
        {
            var client = await CreateHttpClientAsync();

            var response = await client.PutAsJsonAsync(
                $"{_apiBaseUrl}/api/VaultEntry/{request.EntryIdentifier}",
                request,
                _jsonOptions
            );

            response.EnsureSuccessStatusCode();
        }

        
        public async Task DeleteVaultEntryAsync(Guid identifier, string encryptedLog)
        {
            var client = await CreateHttpClientAsync();
            
            var request = new HttpRequestMessage(HttpMethod.Delete, $"{_apiBaseUrl}/api/VaultEntry/{identifier}");
            
            var deleteRequest = new DeleteVaultEntryRequest
            {
                EntryIdentifier = identifier,
                EncryptedLog = encryptedLog
            };
            
            request.Content = JsonContent.Create(deleteRequest, options: _jsonOptions);
            
            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
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
}