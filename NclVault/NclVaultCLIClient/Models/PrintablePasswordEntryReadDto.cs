using NclVaultCLIClient.Attributes;
using NclVaultFramework.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace NclVaultCLIClient.Models
{
    public class PrintablePasswordEntryReadDto
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
