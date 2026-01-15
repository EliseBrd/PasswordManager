using System;

namespace PasswordManager.Dto.Vault.Requests
{
    public class UpdateUserAccessRequest
    {
        public Guid UserId { get; set; }
        public bool IsAdmin { get; set; }
    }
}
