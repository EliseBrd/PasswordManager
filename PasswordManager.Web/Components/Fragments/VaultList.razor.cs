using Microsoft.AspNetCore.Components;

namespace PasswordManager.Web.Components.Fragments;

public partial class VaultList : ComponentBase
{
    [Parameter]
    public List<VaultModel> Vaults { get; set; } = new();

    private string GetBadgeClass(string category)
    {
        return category.ToLower() switch
        {
            "personnel" => "badgePersonnel",
            "partagé" => "badgePartage",
            _ => "badgeDefault"
        };
    }

    public class VaultModel
    {
        public string Identifier { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public bool IsShared { get; set; }
    }
}