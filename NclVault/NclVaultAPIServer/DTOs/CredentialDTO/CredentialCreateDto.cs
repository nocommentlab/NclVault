using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NclVaultAPIServer.DTOs.CredentialDTO
{
    public class CredentialCreateDto
    {

        [Required]
        [RegularExpression(@"^[\w.]{3,20}$")]
        public string Username { get; set; }

        [Required]
        [RegularExpression("^((?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*[!$_])).{8,}$")]
        public string Password { get; set; }
    }
}
