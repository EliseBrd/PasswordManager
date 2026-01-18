using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using PasswordManager.Web.Components.Pages;

namespace PasswordManager.Web.Components.Modals;

public partial class ModalCreateOrUpdateVaultEntry : ComponentBase
{
    [Inject] IJSRuntime JS { get; set; } = default!;
    [Parameter] public bool IsOpen { get; set; }

    // Mise à jour du type pour correspondre au nouveau ViewModel
    [Parameter] public VaultDetails.VaultEntryViewModel Entry { get; set; } = new();

    [Parameter] public EventCallback OnConfirm { get; set; }

    [Parameter] public EventCallback OnCancel { get; set; }
    
    [Parameter] public VaultEntryModalMode Mode { get; set; } = VaultEntryModalMode.Create;
    
    public enum VaultEntryModalMode
    {
        Create,
        Edit
    }
    
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen
            && Mode == VaultEntryModalMode.Edit
            && !string.IsNullOrEmpty(Entry.EncryptedData))
        {
            // Appel JS pour déchiffrer et remplir les champs
            await JS.InvokeVoidAsync(
                "cryptoFunctions.decryptAndFillEditModal",
                Entry.EncryptedData,
                "vaultEntryTitleInput",
                "vaultEntryUsernameInput",
                "vaultEntryCommentInput",
                "vaultEntryUrlInput"
            );
        }
    }

}