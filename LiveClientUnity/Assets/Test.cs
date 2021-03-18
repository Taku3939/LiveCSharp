using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using CoreTweet;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Auth.Twitter
{
    public class Test : MonoBehaviour
    {
        private const string _apiKey = "zEFt6eHsSAY3jDKYqcmpt5rQa";
        private const string _apiSecret = "bVJEAUkxhJO62hU5829ilGP76WwxVnr5ct5V9pftpWfVt8JMSV";

        [SerializeField] private string text;
        [SerializeField] private Button _button;
        [SerializeField] private string pincode;

        void Start()
        {
            var session = OAuth.Authorize(_apiKey, _apiSecret);
            _button.onClick.AddListener(async () =>
            {
                var tokens = session.GetTokens(pincode);
                var task = tokens.Media.Upload(new {status = "c#でのテスト", media = new FileInfo("D:\\icon.png")});
                var id = JObject.Parse(task.Json)["media_id"];
                tokens.Statuses.Update(status => text, media_ids => id);

                var icon = await GetIcon(tokens.UserId);
            });
            Application.OpenURL(session.AuthorizeUri.ToString());
        }
        
        public static async Task<TwitterObj> GetIcon(long userId)
        {
            // Curl でアイコンのURLを取得
            JToken profile_image_url_https;
            string requestUrl =
                $"https://api.twitter.com/1.1/users/show.json?user_id={userId.ToString()}";
            using (var httpClient = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, requestUrl))
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
    }
}