using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LucidWrapper.Classes;

namespace LucidWrapper
{
    public class Lucid
    {
        public static string UrlEncode(string a)
        {
            return Uri.EscapeDataString(a);
        }

        private readonly OAuth1Input _input;

        public Lucid(OAuth1Input input)
        {
            _input = input;
            Token = new OAuthToken();
        }

        public OAuthToken Token { get; set; }

        public string GetAuthorizeUrl()
        {
            List<string> parameters = new List<string>();
            parameters.Add("oauth_token=" + Token.RequestToken);
            string requestUrl = _input.AuthorizeEndPoint + "?" + string.Join("&", parameters.ToArray());
            return requestUrl;
        }

        public void RequestToken()
        {
            /* for oauth_timestamp */
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string timestamp = Convert.ToInt64(ts.TotalSeconds).ToString();

            /* for oauth_nonce */
            /* (obviously you wouldn't create a new Random class every time!) */
            string nonce = new Random().Next(123400, 9999999).ToString();

            /* parameters to include in the request - get the list from       */
            /*  "Obtaining an unauthorized request token"                     */
            List<string> parameters = new List<string>();
            parameters.Add("oauth_callback=" + UrlEncode(_input.Callback));
            parameters.Add("oauth_consumer_key=" + UrlEncode(_input.ConsumerKey));
            parameters.Add("oauth_nonce=" + UrlEncode(nonce));
            parameters.Add("oauth_timestamp=" + UrlEncode(timestamp));
            parameters.Add("oauth_signature_method=HMAC-SHA1");
            parameters.Add("oauth_version=1.0");

            /* the url to get a request token */
            string url = _input.RequestTokenEndPoint;

            /* although not clearly documented, it seems that parameters need to be */
            /*  sorted in order for Fire Eagle to accept the signature              */
            parameters.Sort();
            string parametersStr = String.Join("&", parameters.ToArray());

            string baseStr = "GET" + "&" +
                             UrlEncode(url) + "&" +
                             UrlEncode(parametersStr);

            /* create the crypto class we use to generate a signature for the request */
            HMACSHA1 sha1 = new HMACSHA1();
            byte[] key = Encoding.UTF8.GetBytes(_input.ConsumerSecret + "&" +
                                               Token.RequestSecret);
            if (key.Length > 64)
            {
                /* I had to do this to handle a minor bug in my version of HMACSHA1 */
                /*  which falls over if you give it keys that are too long          */
                /* You probably won't need to do this.                              */
                SHA1CryptoServiceProvider coreSha1 = new SHA1CryptoServiceProvider();
                key = coreSha1.ComputeHash(key);
            }
            sha1.Key = key;

            /* generate the signature and add it to our parameters */
            byte[] baseStringBytes = Encoding.UTF8.GetBytes(baseStr);
            byte[] baseStringHash = sha1.ComputeHash(baseStringBytes);
            String base64StringHash = Convert.ToBase64String(baseStringHash);
            String encBase64StringHash = UrlEncode(base64StringHash);
            parameters.Add("oauth_signature=" + encBase64StringHash);
            parameters.Sort();

            /* we are ready to send the request! */
            string requestUrl = url + "?" + String.Join("&", parameters.ToArray());
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";

            Debug.WriteLine(requestUrl);
            var rawData = GetRawData(request);

            /* if it worked, we should have oauth_token and   */
            /*  oauth_token_secret in the response            */
            foreach (string pair in rawData.Split(new char[] { '&' }))
            {
                string[] splitPair = pair.Split(new char[] { '=' });

                switch (splitPair[0])
                {
                    case "oauth_token":
                        Token.RequestToken = splitPair[1];
                        break;
                    case "oauth_token_secret":
                        Token.RequestSecret = splitPair[1];
                        break;
                }
            }
        }


        private string GetRawData(HttpWebRequest request)
        {
            string rawData;

            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                StreamReader responseStream = new StreamReader(response.GetResponseStream());
                rawData = responseStream.ReadToEnd();
                response.Close();
            }
            catch (WebException err)
            {
                Stream objStream = err.Response.GetResponseStream();
                StreamReader objStreamReader = new StreamReader(objStream);
                rawData = objStreamReader.ReadToEnd();
            }
            return rawData;
        }

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
        public LucidDocument ParseDocument(string content)
        {
            LucidDocument document;
            XmlSerializer serializer = XmlSerializer.FromTypes(new[] { typeof(LucidDocument) })[0];
            //using (StreamReader reader = new StreamReader("c:/dev/lucidchart/doc1.xml"))
            using (StreamReader reader = new StreamReader(GenerateStreamFromString(content)))
            {
                document = (LucidDocument)serializer.Deserialize(reader);
            }
            return document;
        }

        /// <summary>
        /// Gets the document.
        /// </summary>
        /// <param name="docId">The document identifier.</param>
        /// <returns></returns>
        public LucidDocument GetDocument(string docId)
        {
            /* for oauth_timestamp */
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string timestamp = Convert.ToInt64(ts.TotalSeconds).ToString();

            /* for oauth_nonce */
            /* (obviously you wouldn't create a new Random class every time!) */
            string nonce = new Random().Next(100000, 9999999).ToString();

            /* parameters to include in the request - get the list from       */
            /*  "Obtaining user-specific access token"                        */
            List<string> parameters = new List<string>();
            parameters.Add("oauth_consumer_key=" + UrlEncode(_input.ConsumerKey));
            parameters.Add("oauth_token=" + UrlEncode(Token.AccessToken));
            parameters.Add("oauth_nonce=" + UrlEncode(nonce));
            parameters.Add("oauth_timestamp=" + UrlEncode(timestamp));
            parameters.Add("oauth_signature_method=HMAC-SHA1");
            parameters.Add("oauth_version=1.0");

            /* the url to get an access token */
            string url = string.Format("https://www.lucidchart.com/api/graph/{0}", docId);

            /* although not clearly documented, it seems that parameters need to be */
            /*  sorted in order for Fire Eagle to accept the signature              */
            parameters.Sort();
            string parametersStr = String.Join("&", parameters.ToArray());

            string baseStr = "GET" + "&" +
                             UrlEncode(url) + "&" +
                             UrlEncode(parametersStr);

            /* create the crypto class we use to generate a signature for the request */
            HMACSHA1 sha1 = new HMACSHA1();
            byte[] key = Encoding.UTF8.GetBytes(_input.ConsumerSecret + "&" +
                                                Token.AccessSecret);
            if (key.Length > 64)
            {
                /* I had to do this to handle a minor bug in my version of HMACSHA1 */
                /*  which falls over if you give it keys that are too long          */
                /* You probably won't need to do this.                              */
                SHA1CryptoServiceProvider coreSha1 = new SHA1CryptoServiceProvider();
                key = coreSha1.ComputeHash(key);
            }
            sha1.Key = key;

            /* generate the signature and add it to our parameters */
            byte[] baseStringBytes = Encoding.UTF8.GetBytes(baseStr);
            byte[] baseStringHash = sha1.ComputeHash(baseStringBytes);
            String base64StringHash = Convert.ToBase64String(baseStringHash);
            String encBase64StringHash = UrlEncode(base64StringHash);
            parameters.Add("oauth_signature=" + encBase64StringHash);
            parameters.Sort();


            string requestUrl = url + "?" + String.Join("&", parameters.ToArray());

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";
            var data = GetRawData(request).Replace("\n", string.Empty);
            return ParseDocument(data);
        }

        public void GetAccessToken()
        {
            /* these are the values we got from Fire Eagle in step 1 */
            String requestToken = Token.RequestToken;
            String requestSecret = Token.RequestSecret;

            /* this is the value the user got from Fire Eagle website in step 2 */
            String verifier = Token.Verifier;

            /* these are the values we want to get from Fire Eagle */


            /* for oauth_timestamp */
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            string timestamp = Convert.ToInt64(ts.TotalSeconds).ToString();

            /* for oauth_nonce */
            /* (obviously you wouldn't create a new Random class every time!) */
            string nonce = new Random().Next(100000, 9999999).ToString();

            /* parameters to include in the request - get the list from       */
            /*  "Obtaining user-specific access token"                        */
            List<string> parameters = new List<string>();
            parameters.Add("oauth_consumer_key=" + UrlEncode(_input.ConsumerKey));
            parameters.Add("oauth_verifier=" + verifier);
            parameters.Add("oauth_token=" + UrlEncode(requestToken));
            parameters.Add("oauth_nonce=" + UrlEncode(nonce));
            parameters.Add("oauth_timestamp=" + UrlEncode(timestamp));
            parameters.Add("oauth_signature_method=HMAC-SHA1");
            parameters.Add("oauth_version=1.0");

            /* the url to get an access token */
            string url = _input.AccessTokenEndPoint;

            /* although not clearly documented, it seems that parameters need to be */
            /*  sorted in order for Fire Eagle to accept the signature              */
            parameters.Sort();
            string parametersStr = String.Join("&", parameters.ToArray());

            string baseStr = "GET" + "&" +
                             UrlEncode(url) + "&" +
                             UrlEncode(parametersStr);

            /* create the crypto class we use to generate a signature for the request */
            HMACSHA1 sha1 = new HMACSHA1();
            byte[] key = Encoding.UTF8.GetBytes(_input.ConsumerSecret + "&" +
                                                requestSecret);
            if (key.Length > 64)
            {
                /* I had to do this to handle a minor bug in my version of HMACSHA1 */
                /*  which falls over if you give it keys that are too long          */
                /* You probably won't need to do this.                              */
                SHA1CryptoServiceProvider coreSha1 = new SHA1CryptoServiceProvider();
                key = coreSha1.ComputeHash(key);
            }
            sha1.Key = key;

            /* generate the signature and add it to our parameters */
            byte[] baseStringBytes = Encoding.UTF8.GetBytes(baseStr);
            byte[] baseStringHash = sha1.ComputeHash(baseStringBytes);
            String base64StringHash = Convert.ToBase64String(baseStringHash);
            String encBase64StringHash = UrlEncode(base64StringHash);
            parameters.Add("oauth_signature=" + encBase64StringHash);
            parameters.Sort();

            /* we are ready to send the request! */
            string requestUrl = url + "?" + String.Join("&", parameters.ToArray());
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(requestUrl);
            request.Method = "GET";
            request.ContentType = "application/x-www-form-urlencoded";


            var rawData = GetRawData(request);
            /* if it worked, we should have oauth_token and   */
            /*  oauth_token_secret in the response            */
            foreach (string pair in rawData.Split(new char[] { '&' }))
            {
                string[] split_pair = pair.Split(new char[] { '=' });

                switch (split_pair[0])
                {
                    case "oauth_token":
                        Token.AccessToken = split_pair[1];
                        break;
                    case "oauth_token_secret":
                        Token.AccessSecret = split_pair[1];
                        break;
                }
            }
        }
    }
}
