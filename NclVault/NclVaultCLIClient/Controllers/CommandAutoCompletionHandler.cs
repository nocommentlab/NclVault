using System;
using System.Collections.Generic;
using System.Text;

namespace NclVaultCLIClient.Controllers
{
    class CommandAutoCompletionHandler : IAutoCompleteHandler
    {

        public char[] Separators { get; set; } = new char[] { '/' };


        public string[] GetSuggestions(string text, int index)
        {
            if (text.StartsWith('/'))
                return new string[] { "signup", "login", "readpassword", "readpasswords", "updatepassword", "exit" };
            return null;

        }
    }
}
