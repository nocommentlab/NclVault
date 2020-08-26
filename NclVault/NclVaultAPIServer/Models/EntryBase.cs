using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime;
using System.Threading.Tasks;

namespace NclVaultAPIServer.Models
{
    public class EntryBase
    {
        [Key]
        public int Id { get; set; }
        public string Group { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime Expired { get; set; }
        public string Notes { get; set; }

        /*public byte[] IV { get; set; }*/
    }
}
