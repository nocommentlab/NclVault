﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace NclVaultAPIServer.DTOs.PasswordEntryDTO
{
    public class PasswordEntryCreateDto
    {
        public string Group { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime Expired { get; set; }
        public string Notes { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
        public string Url { get; set; }
    }
}
