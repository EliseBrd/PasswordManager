using PasswordManager.Dto.User;

namespace PasswordManager.API.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserSummaryResponse>> SearchUsersAsync(string query);
    }
}
