using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace NclVaultAPIServer.Models
{
    public class PasswordEntry: EntryBase
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
    }
}
