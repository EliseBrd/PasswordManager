using Microsoft.AspNetCore.Components;
using PasswordManager.Web.Components.Pages;

namespace PasswordManager.Web.Components.Modals;

public partial class ModalCreateVaultEntry : ComponentBase
{
    [Parameter] public bool IsOpen { get; set; }

    [Parameter] public VaultDetails.DecryptedVaultEntry Entry { get; set; } = new();

    [Parameter] public EventCallback OnConfirm { get; set; }

    [Parameter] public EventCallback OnCancel { get; set; }
}