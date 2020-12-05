using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace NclVaultCLIClient.Models
{
    public class Context
    {
        public IPEndPoint IPENDPOINT_VaultServerApi { get; set; }
        public string STRING_InitIdFilePath { get; set; }
    }
}
