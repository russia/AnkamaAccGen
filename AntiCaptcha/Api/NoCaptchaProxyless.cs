using AnkamaAccGen.AntiCaptcha;
using AnkamaAccGen.AntiCaptcha.ApiResponse;
using Newtonsoft.Json.Linq;
using System;

namespace AnkamaAccGen.AntiCaptcha.Api
{
    public class NoCaptchaProxyless : AnticaptchaBase
    {
        public Uri WebsiteUrl { protected get; set; }
        public string WebsiteKey { protected get; set; }
        public string WebsiteSToken { protected get; set; }
        public override JObject GetPostData()
        {
            return new JObject
            {
                {"type", "NoCaptchaTaskProxyless"},
                {"websiteURL", WebsiteUrl},
                {"websiteKey", WebsiteKey},
                {"websiteSToken", WebsiteSToken}
            };
        }
        public string GetTaskSolution()
        {
            return TaskInfo;
        }
    }
}