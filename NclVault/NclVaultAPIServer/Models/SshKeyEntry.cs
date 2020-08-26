using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NclVaultAPIServer.Models
{
    public class SshKeyEntry : EntryBase
    {
        public int BitStrenght { get; set; }
        public string Format { get; set; }
        
        public string Passpharase { get; set; }
        public string PrivateKey { get; set; }
        public string PublicKey { get; set; }

    }
}
