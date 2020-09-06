using System;
using System.Collections.Generic;
using System.Text;

namespace NclVaultCLIClient.Models
{
    class LoginResponseResult : HTTPResponseResult
    {
        public string STRING_JwtToken { get; set; }
    }
}
