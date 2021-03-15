using System;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;
using Narange.Twitter;
using Newtonsoft.Json.Linq;

namespace Auth.Twitter
{
    public class NOauth
    {
        public static NOauth Instance
        {
            get
            {
                if (instance == null) instance = new NOauth();
                return instance;
            }
        }

        private static NOauth instance;
        private NOauth(){}
        // CONSUMER 秘密鍵
        private const string ConsumerKey = "5eNK6zHTeKwSkaRaTtEElI6iw";
        private const string ConsumerSecret = "4BXegpxFThNyb5Yuhb4gdIu9693zgnCyMTqodW8zbbGEDYMwio";
        private readonly TwitterUtils tu = new TwitterUtils();
        private string Token, TokenSecret;

        public void OpenAuthSite() => System.Diagnostics.Process.Start(GetAuthTokenUrl());

        /// <summary>
        /// Twitter認証ページのURLを取得
        /// </summary>
        /// <returns></returns>
        public string GetAuthTokenUrl() => tu.GetOAuthToken(ConsumerKey, ConsumerSecret);

        public bool AuthorizeVerification(string pinCode, out string userId)
        {
            try
            {
                // UserIDやAccessTokenを取得
                tu.GetOAuthAccessTokenWithOAuthVerifier(
                    pinCode,
                    ConsumerKey,
                    ConsumerSecret,
                    out Token, out TokenSecret, out userId);
                return true;
                
            }
            catch (Exception e)
            {
                Debug.Log(e.ToString());
                userId = "";
                return false;
            }
        }
        
        public void Tweet(string message)
        {
            try
            {
                tu.UpdateStatus(message, ConsumerKey, ConsumerSecret, Token, TokenSecret, true);
            }
            catch (Exception e)
            {
                Debug.Log(e);
            }
         
        }
        public static async Task<TwitterObj> GetIcon(long userId)
        {
            // Curl でアイコンのURLを取得
            JToken profile_image_url_https;
            string requestUrl =
                $"https://api.twitter.com/1.1/users/show.json?user_id={userId.ToString()}";
            using (var httpClient = new HttpClient())
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), requestUrl))
            {
                request.Headers.TryAddWithoutValidation("authorization",
                    "Bearer AAAAAAAAAAAAAAAAAAAAAIBDGAEAAAAAw7ZeDqIhsXsAFYwaK5nq7MCl0%2FY%3DnGyxmDaaqHGaRiVJ3SwhOnuJMETF62ffPTQ8ddfZVpJ49MzY68");
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(result);

                //必要なキーだけ取り出す ["profile_image_url_https"]
                profile_image_url_https = obj["profile_image_url_https"];
                var screenName = obj["screen_name"];
                return new TwitterObj(screenName.ToString(), userId, profile_image_url_https.ToString());
            }
        }

        public static TwitterObj Parse(string text)
        {
            var strings = text.Split('&');
            return new TwitterObj(strings[0], long.Parse(strings[1]), strings[2]);
        }
    }
}