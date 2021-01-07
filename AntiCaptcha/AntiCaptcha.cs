using AnkamaAccGen.AntiCaptcha.Api;
using AnkamaAccGen.Helpers;
using System;

namespace AnkamaAccGen.AntiCaptcha
{
    internal class AntiCaptcha
    {
        private const string AntiCaptchaWebsiteKey = "6LfbFRsUAAAAACrqF5w4oOiGVxOsjSUjIHHvglJx";
        private const string AntiCaptchaCreateTaskUrl = "http://api.anti-captcha.com/createTask";
        private const string AntiCaptchaGetTaskResultUrl = "https://api.anti-captcha.com/getTaskResult";

        internal static string NoCaptchaProxyless(string task, string websiteUrl)
        {

            var api = new NoCaptchaProxyless
            {
                ClientKey = Constants.AntiCaptchaKey,
                WebsiteUrl = new Uri(websiteUrl),
                WebsiteKey = AntiCaptchaWebsiteKey
            };

            if (!api.CreateTaskViaWebClient(task))
            {
                DebugHelper.Out(task, "API v2 send failed. " + api.ErrorMessage, DebugHelper.Type.Error);
            }
            else if (!api.WaitForResultViaWebClient(task))
            {
                DebugHelper.Out(task, "Could not solve the captcha.", DebugHelper.Type.Error);
                return "captcha_token_timed_out";
            }
            else
            {
                var captcha_token = api.GetTaskSolution();

                //DebugHelper.Out(task, "Result: " + captcha_token, DebugHelper.Type.Success);

                return captcha_token;
            }

            return string.Empty;
        }
    }
}
