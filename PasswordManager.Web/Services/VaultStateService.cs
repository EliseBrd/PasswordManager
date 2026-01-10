namespace PasswordManager.Web.Services;

public class VaultStateService
{
    public bool? CurrentFilter { get; private set; } = null;
    public event Action? OnFilterChanged;

    public void SetFilter(bool? isShared)
    {
        if (CurrentFilter != isShared)
        {
            CurrentFilter = isShared;
            OnFilterChanged?.Invoke();
        }
    }
}
