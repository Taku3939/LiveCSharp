using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CoreTweet;
using JetBrains.Annotations;
using UnityEngine;
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
        private readonly OAuth.OAuthSession session;

        // CONSUMER 秘密鍵
        private const string ConsumerKey = "iSR3pQXzNinfy6DXln5l7euzY";
        private const string ConsumerSecret = "AZtsEvISNkQK1vslORnMdsL1PWYBidsPMPblWuzFYqJaea6pRv";
        /// <summary>
        /// Constructor
        /// </summary>
        private NOauth() => session = OAuth.Authorize(ConsumerKey, ConsumerSecret);

        public void OpenAuthSite() => Application.OpenURL(session.AuthorizeUri.ToString());

        private Tokens tokens;
        public bool AuthorizeVerification(string pinCode, out ulong userId)
        {
            try
            {
                tokens = session.GetTokens(pinCode);
                userId = (ulong) tokens.UserId;
                return true;
            }
            catch (Exception e)
            {
                userId = UInt64.MaxValue;
                Debug.Log(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// Twitterでのコメントのみのツイート
        /// </summary>
        /// <param name="message">ツイート内容</param>
        /// <returns></returns>
        public StatusResponse Tweet(string message) => tokens.Statuses.Update(status => message);
        
        /// <summary>
        /// Twitterでの画像付きのツイート
        /// </summary>
        /// <param name="message">ツイート内容</param>
        /// <param name="imgPath">添付画像(.png, .jpeg, ...)</param>
        /// <returns></returns>
        public StatusResponse Tweet(string message, string imgPath)
        {
            var task = tokens.Media.Upload(new {status = message, media = new FileInfo(imgPath)});
            var id = JObject.Parse(task.Json)["media_id"];
            return tokens.Statuses.Update(status => message, media_ids => id);
        }

        
        /// <summary>
        /// アイコンの取得
        /// Warning : you must check null
        /// </summary>
        /// <param name="userId">TwitterのユーザID</param>
        /// <returns></returns>
        [CanBeNull]
        public static async Task<TwitterObj> GetIcon(ulong userId)
        {
            // Curl でアイコンのURLを取得
            JToken profile_image_url_https;
            string requestUrl =
                $"https://api.twitter.com/1.1/users/show.json?user_id={userId.ToString()}";
            using (var httpClient = new HttpClient())
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), requestUrl))
            {
                request.Headers.TryAddWithoutValidation("authorization",
                    "Bearer AAAAAAAAAAAAAAAAAAAAAIBDGAEAAAAA3%2B3NRRFVfO6g12h2AE1L4MOgVOI%3DntT5nUHONPAZgsQURAEFPe4IcLkRzRmrvYpcJU9LXEzQqIRmqf");
                HttpResponseMessage response = await httpClient.SendAsync(request);
                var result = await response.Content.ReadAsStringAsync();
                var obj = JObject.Parse(result);

                //必要なキーだけ取り出す ["profile_image_url_https"]
                profile_image_url_https = obj["profile_image_url_https"];
                var screenName = obj["screen_name"];
                Debug.Log(screenName);
                Debug.Log(profile_image_url_https);
                if (screenName == null || profile_image_url_https == null) return null; 
                return new TwitterObj(screenName.ToString(), userId, profile_image_url_https.ToString()); 
            }
        }

        public static TwitterObj Parse(string text)
        {
            var strings = text.Split('&');
            return new TwitterObj(strings[0], ulong.Parse(strings[1]), strings[2]);
        }
    }
}