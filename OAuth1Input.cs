using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LucidWrapper
{
    public class OAuth1Input
    {
        public string ConsumerKey { get; set; }
        public string ConsumerSecret { get; set; }
        public string AccessTokenEndPoint { get; set; }
        public string RequestTokenEndPoint { get; set; }
        public string AuthorizeEndPoint { get; set; }
        public string Callback { get; set; }

        public OAuth1Input()
        {
            AccessTokenEndPoint = "https://www.lucidchart.com/oauth/accessToken";
            RequestTokenEndPoint = "https://www.lucidchart.com/oauth/requestToken";
            AuthorizeEndPoint = "https://www.lucidchart.com/oauth/authorize";
        }
    }
}
