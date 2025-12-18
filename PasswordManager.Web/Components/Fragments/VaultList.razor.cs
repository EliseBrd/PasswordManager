using Microsoft.AspNetCore.Components;
using PasswordManager.Dto.Vault.Responses;

namespace PasswordManager.Web.Components.Fragments;

public partial class VaultList : ComponentBase
{
    [Parameter]
    public List<VaultSummaryResponse> Vaults { get; set; } = new();

    private string GetBadgeClass(bool isShared)
    {
        return isShared ? "badgePartage" : "badgePersonnel";
    }

    private string GetCategoryLabel(bool isShared)
    {
        return isShared ? "Partagé" : "Personnel";
    }

}