using Microsoft.AspNetCore.Components;

namespace PasswordManager.Web.Components.Modals;

public partial class ModalValidateDelete
{
    [Parameter] public bool IsOpen { get; set; }
    [Parameter] public string Message { get; set; } = "Êtes-vous sûr de vouloir supprimer ?";
    [Parameter] public EventCallback OnConfirm { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }
}