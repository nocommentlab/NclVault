using System;
using System.Collections.Generic;
using System.Text;

namespace NclVaultCLIClient.Models
{
    public class PasswordEntryReadDto
    {
        
        public int Id { get; set; }
        
        public string Group { get; set; }
        
        public string Name { get; set; }
        
        public DateTime Expired { get; set; }
        public string Notes { get; set; }
        
        public string Username { get; set; }
        
        public string Password { get; set; }
        
        public string Url { get; set; }
    }
}
