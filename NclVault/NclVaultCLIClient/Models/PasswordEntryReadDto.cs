using NclVaultCLIClient.Attributes;
using System;
using System.Collections.Generic;
using System.Text;

namespace NclVaultCLIClient.Models
{
    public class PasswordEntryReadDto
    {
        [Printable]
        public int Id { get; set; }
        [Printable]
        public string Group { get; set; }
        [Printable]
        public string Name { get; set; }
        [Printable]
        public DateTime Expired { get; set; }
        public string Notes { get; set; }
        [Printable]
        public string Username { get; set; }
        [Printable]
        public string Password { get; set; }
        [Printable]
        public string Url { get; set; }
    }
}
