using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LucidWrapper
{
    public class OAuthToken
    {
        public string RequestToken { get; set; }
        public string RequestSecret { get; set; }

        public string Verifier { get; set; }

        public string AccessToken { get; set; }
        public string AccessSecret { get; set; }

        public OAuthToken()
        {
            RequestSecret = string.Empty;
        }
    }
}
