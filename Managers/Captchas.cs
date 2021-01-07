using AnkamaAccGen.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AnkamaAccGen.Managers
{
    public static class Captchas
    {
        public static List<CaptchaResponse> Tokens = new List<CaptchaResponse>();

        public static void Run()
        {
            while (true)
            {
                Thread.Sleep(150);
                if ((Accounts.CreatedAccounts.Count() >= Constants.AccountsCount) || Tokens.Any() && Tokens.Count() >= (Constants.MaxThreads))
                    continue;

                CaptchaResponse resp = new CaptchaResponse();
                Task task = Task.Factory.StartNew(() =>
                {
                    resp.GetCaptcha();
                });

                Tokens.Add(resp);
            }
        }
    }

    public class CaptchaResponse
    {
        public string Token { get; set; }
        public bool GotResponse { get { return !string.IsNullOrEmpty(this.Token); } }

        public void GetCaptcha()
        {
            string taskName = $"[CaptchaTask {Captchas.Tokens.Count()}] ";
            DebugHelper.Out(taskName, "New captcha task created !", DebugHelper.Type.Success);
            string captcha_token = AntiCaptcha.AntiCaptcha.NoCaptchaProxyless(taskName, "https://www.wakfu.com/fr/mmorpg/jouer");

            if (string.IsNullOrEmpty(captcha_token))
            {
                DebugHelper.Out(taskName, "Unable to generate captcha_token.", DebugHelper.Type.Error);
                return;
            }

            DebugHelper.Out(taskName, "Captcha_token generated successfully !", DebugHelper.Type.Success);
            this.Token = captcha_token;
        }
    }
}