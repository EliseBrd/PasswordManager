using System;

namespace PasswordManager.Dto.User
{
    public class UserSummaryResponse
    {
        public Guid Identifier { get; set; }
        public string Email { get; set; } = string.Empty;
    }
}
