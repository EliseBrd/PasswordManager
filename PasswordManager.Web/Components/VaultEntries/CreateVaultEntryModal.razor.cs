//using Microsoft.AspNetCore.Components;

//namespace PasswordManager.Web.Components.VaultEntries
//{
//    public class CreateVaultEntryModal
//    {
//        [Parameter] public bool IsOpen { get; set; }
//        [Parameter] public EventCallback<bool> IsOpenChanged { get; set; }

//        [Parameter] public EventCallback<VaultEntryCreateDto> OnSave { get; set; }

//        string EntryName = "";
//        string EntryUsername = "";
//        string EntryPassword = "";
//        string EntryNotes = "";

//        async Task Close()
//        {
//            await IsOpenChanged.InvokeAsync(false);
//            Reset();
//        }

//        async Task Save()
//        {
//            var dto = new VaultEntryCreateDto
//            {
//                Name = EntryName,
//                Username = EntryUsername,
//                Password = EntryPassword,
//                Notes = EntryNotes
//            };

//            await OnSave.InvokeAsync(dto);
//            await Close();
//        }

//        void Reset()
//        {
//            EntryName = "";
//            EntryUsername = "";
//            EntryPassword = "";
//            EntryNotes = "";
//        }
//    }
//}
