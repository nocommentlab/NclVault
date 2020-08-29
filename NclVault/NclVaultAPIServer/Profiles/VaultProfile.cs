using AutoMapper;
using NclVaultAPIServer.DTOs.CredentialDTO;
using NclVaultAPIServer.DTOs.PasswordEntryDTO;
using NclVaultAPIServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NclVaultAPIServer.Profiles
{
    public class VaultProfile: Profile
    {
        public VaultProfile()
        {
            CreateMap<Credential, CredentialReadDto>();
            CreateMap<PasswordEntryCreateDto, PasswordEntry>();
            CreateMap<PasswordEntry, PasswordEntryReadDto>();

            CreateMap<List<PasswordEntry>, List<PasswordEntryReadDto>>();
        }
    }
}
