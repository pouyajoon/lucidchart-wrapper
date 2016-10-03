# Lucid chart c# wrapper.

[https://www.lucidchart.com/pages/api_documentation](https://www.lucidchart.com/pages/api_documentation)


## Get AccessToken and AccessSecret

As OAuth 1 require to call a callback, you can run this inside a MVC App, to have a callback.

Inside a LucidChartController

Use RequestToken to request the token, you will be redirected to lucidchart page, when validated you will be send to the callback where the AccessToken are AccessSecret are set.


```c#

private static Lucid _wrapper;

static readonly OAuth1Input Input = new OAuth1Input
{
    ConsumerKey = "MY_KEY",
    ConsumerSecret = "MY_KEY_SECRET",
    Callback = "https://localhost/MVC_SERVER/OAuth1/callback"
};

/// <summary>
/// Requests the token.
/// </summary>
/// <returns></returns>
[HttpGet]
[Route("Lucid/GetToken")]
public void RequestToken()
{
    var input = Input;
    _wrapper = new Lucid(input);
    _wrapper.RequestToken();
    var redirect = _wrapper.GetAuthorizeUrl();
    Response.Redirect(redirect);
}

/// <summary>
/// oes the auth1 callback.
/// </summary>
/// <returns></returns>
[HttpGet]
[Route("OAuth1/callback")]
public JsonResult OAuth1Callback()
{
    _wrapper.Token.Verifier = Request.Params["oauth_verifier"];
    var oAuthToken = Request.Params["oauth_token"];
    _wrapper.GetAccessToken();
	// _wrapper.Token.AccessToken && _wrapper.Token.AccessSecret are set to the correct values
    var doc = _wrapper.GetDocument("bd3f4ae8-8cdf-45b0-82c1-97c095c4fcc0");
    return CwBackEndHelper.ReturnJsonSuccess(_wrapper.Token);
}



```


## When AccessToken and AccessSecret are known, you can either use MVC or any C# projet

```c#
/// <summary>
/// Gets the document.
/// </summary>
/// <param name="documentId">The document identifier.</param>
/// <returns></returns>
[HttpGet]
[Route("Lucid/GetDocument/{documentId}")]
public JsonResult GetDocument(string documentId)
{
    _wrapper = new Lucid(Input)
    {
        Token =
        {
            AccessToken = "ACCESS_TOKEN",
            AccessSecret = "ACCESS_SECRET"
        }
    };
    var doc = _wrapper.GetDocument(documentId);
    return CwBackEndHelper.ReturnJsonSuccess(doc);
} 
```