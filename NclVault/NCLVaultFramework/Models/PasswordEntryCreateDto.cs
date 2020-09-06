using System;

namespace NclVaultFramework.Models
{
    public class PasswordEntryCreateDto
    {
        public string Group { get; set; }
        public string Name { get; set; }
        public DateTime Expired { get; set; }
        public string Notes { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
    }
}
