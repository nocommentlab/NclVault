﻿using System.Net;

namespace NclVaultCLIClient.Models
{
    class HTTPResponseResult
    {

        public HttpStatusCode StatusCode { get; set; }
        public string StatusDescription { get; set; }
        public string STRING_JwtToken { get; set; }
        public object OBJECT_RestResult { get; set; }

    }
}