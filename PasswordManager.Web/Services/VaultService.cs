using Microsoft.Identity.Abstractions;
using PasswordManager.Dto.Vault.Requests;
using PasswordManager.Dto.Vault.Responses;
using System.Net.Http.Json;

namespace PasswordManager.Web.Services
{
    public class VaultService
    {
        private readonly IDownstreamApi _downstreamApi;

        public VaultService(IDownstreamApi downstreamApi)
        {
            _downstreamApi = downstreamApi;
        }

        public async Task<IEnumerable<VaultSummaryResponse>?> GetAccessibleVaultsAsync()
        {
            // The service name "PasswordManager.api" must match the one in Program.cs
            return await _downstreamApi.GetForUserAsync<IEnumerable<VaultSummaryResponse>>(
                "PasswordManager.api",
                options =>
                {
                    options.RelativePath = "api/vault";
                });
        }

        public async Task CreateVaultAsync(CreateVaultRequest request)
        {
            // The service name "PasswordManager.api" must match the one in Program.cs
            // We use the overload where the relative path is specified in the options action.
            // This avoids ambiguity between the different overloads of PostForUserAsync.
            await _downstreamApi.PostForUserAsync<CreateVaultRequest, object>(
                "PasswordManager.api",
                request,
                options =>
                {
                    options.RelativePath = "api/vault";
                });
        }
    }
}
