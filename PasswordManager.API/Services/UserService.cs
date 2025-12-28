using Microsoft.EntityFrameworkCore;
using PasswordManager.API.Context;
using PasswordManager.API.Services.Interfaces;
using PasswordManager.Dto.User;

namespace PasswordManager.API.Services
{
    public class UserService : IUserService
    {
        private readonly PasswordManagerDBContext _context;

        public UserService(PasswordManagerDBContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserSummaryResponse>> SearchUsersAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<UserSummaryResponse>();

            return await _context.Users
                .Where(u => u.Email.Contains(query))
                .Select(u => new UserSummaryResponse
                {
                    Identifier = u.Identifier,
                    Email = u.Email
                })
                .Take(10) // Limit results
                .ToListAsync();
        }
    }
}
