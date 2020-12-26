using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NclVaultAPIServer.Models
{
    public class EntryBase
    {
        [Key]
        public int Id { get; set; }

        /* Used to declare the CredentialFK Section */
        [ForeignKey("Credential")]
        public int CredentialFK { get; set; }
        
        [JsonIgnore] // Prevents System.Text.Json.JsonException: A possible object cycle was detected which is not supported
        public Credential Credential { get; set; }

        public string Group { get; set; }
        [Required]
        public string Name { get; set; }
        public DateTime Expired { get; set; }
        public string Notes { get; set; }


    }
}
