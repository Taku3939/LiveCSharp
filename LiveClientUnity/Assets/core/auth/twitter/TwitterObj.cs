// Created by Takuya Isaki on 2021/03/09

namespace Auth.Twitter
{
    public class TwitterObj
    {
        public readonly string screenName;
        public readonly long userId;
        public readonly string iconPath;

        public TwitterObj(string screenName, long userId, string iconPath)
        {
            this.screenName = screenName;
            this.userId = userId;
            this.iconPath = iconPath;
        }
    }
}