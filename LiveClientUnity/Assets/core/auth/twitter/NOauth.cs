using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CoreTweet;
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
        private const string ConsumerKey = "5eNK6zHTeKwSkaRaTtEElI6iw";
        private const string ConsumerSecret = "4BXegpxFThNyb5Yuhb4gdIu9693zgnCyMTqodW8zbbGEDYMwio";
        /// <summary>
        /// Constructor
        /// </summary>
        private NOauth() => session = OAuth.Authorize(ConsumerKey, ConsumerSecret);

        public void OpenAuthSite() => Application.OpenURL(session.AuthorizeUri.ToString());

        private Tokens tokens;
        public bool AuthorizeVerification(string pinCode, out string userId)
        {

            userId = "";
            try
            {
                tokens = session.GetTokens(pinCode);
                return true;
            }
            catch (Exception e)
            {
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
        /// </summary>
        /// <param name="userId">TwitterのユーザID</param>
        /// <returns></returns>
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