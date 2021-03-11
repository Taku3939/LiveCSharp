using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Net;
using System.IO;
using System.Collections.Specialized;
using System.Text;
using Narangec.Twitter;
using UnityEngine;
using UnityEngine.Networking;

namespace Narange.Twitter
{
  public class OAuthTwitter : OAuthBase
  {
    public enum Method { GET, POST };
    public const string REQUEST_TOKEN = "https://api.twitter.com/oauth/request_token";
    public const string AUTHORIZE = "https://api.twitter.com/oauth/authorize";
    public const string ACCESS_TOKEN = "https://api.twitter.com/oauth/access_token";
    public const string REALM = "https://api.twitter.com";
    
    #region Properties
    public string ConsumerKey { get; set; }
    public string ConsumerSecret { get; set; }
    public string OAuthToken { get; set; }
    public string OAuthTokenSecret { get; set; }
    public string Token { get; set; }
    public string TokenSecret { get; set;}
    
    public string UserId { get; set; }
    
    #endregion

    /// <summary>
    /// Get the link to Twitter's authorization page for this application.
    /// </summary>
    /// <returns>The url with a valid request token, or a null string.</returns>
    public string AuthorizationLinkGet()
    {
      string ret = null;

      string response = oAuthWebRequest(Method.GET, REQUEST_TOKEN, String.Empty, String.Empty, false, string.Empty, false);
      if (response.Length > 0) {
        //response contains token and token secret.  We only need the token.
        NameValueCollection qs = ParseQueryString(response);
        if (qs["oauth_token"] != null) {
          OAuthToken = qs["oauth_token"]; // tuck this away for later
          OAuthTokenSecret = qs["oauth_token_secret"];
          ret = AUTHORIZE + "?oauth_token=" + qs["oauth_token"];
        }
      }
      return ret;
    }

    /// <summary>
    /// Exchange the request token for an access token.
    /// </summary>
    /// <param name="authToken">The oauth_token is supplied by Twitter's authorization page following the callback.</param>
    public void AccessTokenGetWithOAuthVerifier(string OAuthVerifier, string OAuthToken)
    {
      //this.PIN = OAuthVerifier;
      this.Token = OAuthToken;

      string response = oAuthWebRequest(Method.GET, ACCESS_TOKEN, String.Empty, String.Empty, true, OAuthVerifier, false);

      if (response.Length > 0) {
        //Store the Token and Token Secret
        NameValueCollection qs = ParseQueryString(response);
        if (qs["oauth_token"] != null) {
          this.Token = qs["oauth_token"];
        }
        if (qs["oauth_token_secret"] != null) {
          this.TokenSecret = qs["oauth_token_secret"];
        }
        if (qs["user_id"] != null)
        {
          this.UserId = qs["user_id"];
        }
        // ここにscreen_nameを追加すれば同じようにとってこれる
        // TwitterUtils.cs(41) に、返すコードを書けば完了
      }
    }


    /// <summary>
    /// Submit a web request using oAuth.
    /// </summary>
    /// <param name="method">GET or POST</param>
    /// <param name="url">The full url, including the querystring.</param>
    /// <param name="postData">Data to post (querystring format)</param>
    /// <returns>The web server response.</returns>
    public string oAuthWebRequest(Method method, string url, string parameter, string postData, bool useOAuthVerifier, string OAuthVerifier, bool useAuthorizationHeader)
    {
      string outUrl = "";
      string querystring = "";
      string ret = "";
      
      //Setup postData for signing.
      //Add the postData to the querystring.
      if (method == Method.POST) {
        if (postData.Length > 0) {
          //Decode the parameters and re-encode using the oAuth UrlEncode method.
          NameValueCollection qs = ParseQueryString(postData);
          postData = "";
          foreach (string key in qs.AllKeys) {
            if (postData.Length > 0) {
              postData += "&";
            }
            qs[key] = UnityWebRequest.EscapeURL(qs[key]);
            //qs[key] = this.UrlEncode(qs[key]);
            qs[key] = this.UrlEncode(qs[key], Encoding.UTF8);
            postData += key + "=" + qs[key];

          }
          if (url.IndexOf("?") > 0) {
            url += "&";
          }
          else {
            url += "?";
          }
          url += postData;
        }
      }
      
      if (method == Method.GET) {
        url += parameter;
      }

      Uri uri = new Uri(url);

      string nonce = this.GenerateNonce();
      string timeStamp = this.GenerateTimeStamp();

      string sig = "";
      //Generate Signature
      if (useOAuthVerifier == false) {
        sig = this.GenerateSignature(uri,
           this.ConsumerKey, this.ConsumerSecret, this.Token, this.TokenSecret,
           method.ToString(),
           timeStamp,
           nonce,
           out outUrl,
           out querystring);
      }
      else {
        sig = this.GenerateSignatureWithOAuthVerifier(uri,
           this.ConsumerKey, this.ConsumerSecret, this.Token, this.TokenSecret,
           method.ToString(),
           timeStamp,
           nonce,
           OAuthVerifier,
           out outUrl,
           out querystring);
      }


      if (useAuthorizationHeader == false) {
        querystring += "&oauth_signature=" + UnityWebRequest.EscapeURL(sig);
        //Convert the querystring to postData
        if (method == Method.POST) {
          postData = querystring;
          querystring = "";
        }

        if (querystring.Length > 0) {
          outUrl += "?";
        }
        ret = WebRequest(method, outUrl + querystring, postData);
      }
      else {
        string AuthorizationHeaderStr = GenerateOAuthAuthorizationHeader(uri,
          this.ConsumerKey, this.Token, this.TokenSecret,
          method.ToString(),
          timeStamp,
          nonce,
          HMACSHA1SignatureType,
          UnityWebRequest.EscapeURL(sig));

        ret = WebRequestUseAuthorizationHeader(method, outUrl, postData, AuthorizationHeaderStr);  
      }
      
      return ret;
    }

    /// <summary>
    /// Web Request Wrapper
    /// </summary>
    /// <param name="method">Http Method</param>
    /// <param name="url">Full url to the web resource</param>
    /// <param name="postData">Data to post in querystring format</param>
    /// <returns>The web server response.</returns>
    public string WebRequest(Method method, string url, string postData)
    {
      HttpWebRequest webRequest = null;
      StreamWriter requestWriter = null;
      string responseData = "";

      webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
      webRequest.Method = method.ToString();
      webRequest.ServicePoint.Expect100Continue = false;
      //webRequest.UserAgent  = "Identify your application please.";
      //webRequest.Timeout = 20000;

      if (method == Method.POST) {
        webRequest.ContentType = "application/x-www-form-urlencoded";

        //POST the data.
        requestWriter = new StreamWriter(webRequest.GetRequestStream());
        try {
          requestWriter.Write(postData);
        }
        catch {
          throw;
        }
        finally {
          requestWriter.Close();
          requestWriter = null;
        }
      }

      responseData = WebResponseGet(webRequest);

      webRequest = null;

      return responseData;
    }

    /// <summary>
    /// Web Request Wrapper
    /// </summary>
    /// <param name="method">Http Method</param>
    /// <param name="url">Full url to the web resource</param>
    /// <param name="postData">Data to post in querystring format</param>
    /// <returns>The web server response.</returns>
    public string WebRequestUseAuthorizationHeader(Method method, string url, string postData, string Signature)
    {
      HttpWebRequest webRequest = null;
      StreamWriter requestWriter = null;
      string responseData = "";

      webRequest = System.Net.WebRequest.Create(url) as HttpWebRequest;
      webRequest.Method = method.ToString();
      webRequest.ServicePoint.Expect100Continue = false;
      //webRequest.UserAgent  = "Identify your application please.";
      //webRequest.Timeout = 20000;

      webRequest.Headers.Add("Authorization", Signature);

      if (method == Method.POST) {
        webRequest.ContentType = "application/x-www-form-urlencoded";
        //webRequest.ContentLength = postData.Length;

        //POST the data.
        requestWriter = new StreamWriter(webRequest.GetRequestStream());
        try {
          requestWriter.Write(postData);
        }
        catch {
          throw;
        }
        finally {
          requestWriter.Close();
          requestWriter = null;
        }
      }
      responseData = WebResponseGet(webRequest);
      webRequest = null;
      return responseData;
    }

    /// <summary>
    /// Process the web response.
    /// </summary>
    /// <param name="webRequest">The request object.</param>
    /// <returns>The response data.</returns>
    public string WebResponseGet(HttpWebRequest webRequest)
    {
      StreamReader responseReader = null;
      string responseData = "";

      try {
        responseReader = new StreamReader(webRequest.GetResponse().GetResponseStream());
        responseData = responseReader.ReadToEnd();
      }
      catch (WebException exc) {
        System.Diagnostics.Debug.WriteLine(exc.Response);
        throw;
      }
      catch {
        throw;
      }
      finally {
        webRequest.GetResponse().GetResponseStream().Close();
        responseReader.Close();
        responseReader = null;
      }

      return responseData;
    }
    public static NameValueCollection ParseQueryString(string query)
    {
      var ret = new NameValueCollection();
      foreach (string pair in query.Split('&'))
      {
        string[] kv = pair.Split('=');

        string key = kv.Length == 1
          ? null : Uri.UnescapeDataString(kv[0]).Replace('+', ' ');

        string[] values = Uri.UnescapeDataString(
          kv.Length == 1 ? kv[0] : kv[1]).Replace('+', ' ').Split(',');

        foreach (string value in values)
        {
          ret.Add(key, value);
        }
      }
      return ret;
    }
  }
}