using Microsoft.AspNetCore.Components;



namespace PasswordManager.Web.Components.Fragments;

public partial class LeftNavMenu : ComponentBase
{
    [Inject] private NavigationManager NavManager { get; set; } = default!;
    void GoToCreateVault()
    {
        NavManager.NavigateTo("/createVault");
    }
}