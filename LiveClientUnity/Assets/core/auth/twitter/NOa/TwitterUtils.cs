using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Narange.Twitter
{
  public class TwitterUtils
  {
    private const string UPDATE_STATUS = "https://api.twitter.com/1.1/statuses/update.json";
    private const string STATUSES_MENTIONS = "https://api.twitter.com/1.1/statuses/mentions_timeline.json";
    private const string STATUSES_HOME_TIMELINE = "https://api.twitter.com/1.1/statuses/home_timeline.json";

    OAuthTwitter oAuth;
    public TwitterUtils()
    {
      oAuth = new OAuthTwitter();
    }

    public string GetOAuthToken(string ConsumerKey, string ConsumerSecret)
    {
      oAuth.ConsumerKey = ConsumerKey;
      oAuth.ConsumerSecret = ConsumerSecret;

      string authurl = oAuth.AuthorizationLinkGet();
      return authurl;
    }

    public void GetOAuthAccessTokenWithOAuthVerifier(string OAuthVerifier, string ConsumerKey, string ConsumerSecret,
      out string AccessToken, out string AccessTokenSecret, out string UserId)
    {
      oAuth.ConsumerKey = ConsumerKey;
      oAuth.ConsumerSecret = ConsumerSecret;

      oAuth.AccessTokenGetWithOAuthVerifier(OAuthVerifier, oAuth.OAuthToken);
      AccessToken = oAuth.Token;
      AccessTokenSecret = oAuth.TokenSecret;
      UserId = oAuth.UserId;
    }

    private string GetDataOAuth(string url, string parameter,
      string ConsumerKey, string ConsumerSecret, string AccessToken, string AccessTokenSecret, bool useAuthorizationHeader)
    {
      oAuth.ConsumerKey = ConsumerKey;
      oAuth.ConsumerSecret = ConsumerSecret;
      oAuth.Token = AccessToken;
      oAuth.TokenSecret = AccessTokenSecret;
      string xml = oAuth.oAuthWebRequest(OAuthTwitter.Method.GET, url, parameter, "", false, string.Empty, useAuthorizationHeader);
      return xml;
    }

    private string PostDataOAuth(string url, string mes,
      string ConsumerKey, string ConsumerSecret, string AccessToken, string AccessTokenSecret, bool useAuthorizationHeader)
    {
      string xml = "";

      oAuth.ConsumerKey = ConsumerKey;
      oAuth.ConsumerSecret = ConsumerSecret;
      oAuth.Token = AccessToken;
      oAuth.TokenSecret = AccessTokenSecret;
      xml = oAuth.oAuthWebRequest(OAuthTwitter.Method.POST, url, String.Empty, mes, false, string.Empty, useAuthorizationHeader);
      return xml;
    }
    public void UpdateStatus(string status, string ConsumerKey, string ConsumerSecret, string AccessToken, string AccessTokenSecret,
      bool useAuthorizationHeader)
    {
      string encStatus = UnityWebRequest.EscapeURL(status, System.Text.Encoding.UTF8);
      encStatus = string.Format("status={0:s}", encStatus);
      PostDataOAuth(UPDATE_STATUS, encStatus, ConsumerKey, ConsumerSecret, AccessToken, AccessTokenSecret, useAuthorizationHeader);
    }
  }
}