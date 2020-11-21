using System.Net;

namespace NclVaultFramework.Models
{
    public class HTTPResponseResult
    {
        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string STRING_JwtToken { get; set; }
        public object OBJECT_RestResult { get; set; }
    }
}
