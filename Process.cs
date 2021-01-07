using AccountCreatorV2.Helper;
using AnkamaAccGen.Helpers;
using AnkamaAccGen.Managers;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace AnkamaAccGen
{
    public class Process : IDisposable
    {
        private string Username { get; set; }
        private string Password { get; set; }
        private string Email { get; set; }
        private string CaptchaToken { get; set; }
        private string TaskName { get; set; }

        public Process(string captchaToken, int taskId)
        {
            this.Username = StringHelper.GetUniqueStringUpper();
            this.Password = StringHelper.GetUniqueStringUpper();
            this.Email = StringHelper.GetFixedLengthString(9) + "@boufton.com";
            CaptchaToken = captchaToken;
            TaskName = $"[AccountTask {taskId}] ";
            Start();
        }

        public void Start()
        {
            DebugHelper.Out(TaskName, "New account task created !", DebugHelper.Type.Info);
            var proxy = new WebProxy
            {
                Address = new Uri($"http://megaproxy.rotating.proxyrack.net:10000"),
                Credentials = new NetworkCredential(userName: "klaasvaakjes-country-FR-refreshSeconds-1", password: "ef4c02-aab4d8-4c59a0-555771-c9188b")
            };

            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

            var client = new HttpClient(handler: httpClientHandler, disposeHandler: true);
            int retries = 0;
            client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0");
            client.DefaultRequestHeaders.Add("Accept", "text/html, */*; q=0.01");
            client.DefaultRequestHeaders.Add("Accept-Language", "fr,fr-FR;q=0.8,en-US;q=0.5,en;q=0.3");
            client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
            client.DefaultRequestHeaders.Add("X-PJAX", "true");
            client.DefaultRequestHeaders.Add("X-PJAX-Container", ".ak-registerform-container");
            client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest");
            client.DefaultRequestHeaders.Add("Origin", "https://www.wakfu.com");
            client.DefaultRequestHeaders.Add("Connection", "keep-alive");
            client.DefaultRequestHeaders.Add("Referer", "https://www.wakfu.com/fr/mmorpg/jouer");
            Random rdn = new Random();

            var pairs = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("userlogin", Username),
                new KeyValuePair<string, string>("userpassword", Password),
                new KeyValuePair<string, string>("user_password_confirm", Password),
                new KeyValuePair<string, string>("useremail", Email),
                new KeyValuePair<string, string>("birth_day", "1" + rdn.Next(1, 9).ToString()),
                new KeyValuePair<string, string>("birth_month", rdn.Next(1, 9).ToString()),
                new KeyValuePair<string, string>("birth_year", "199" + rdn.Next(1, 9).ToString()),
                new KeyValuePair<string, string>("parentemail", "noreply@ankama.com"),
                new KeyValuePair<string, string>("sAction", "submit"),
                new KeyValuePair<string, string>("g-recaptcha-response", CaptchaToken),
                };

            FormUrlEncodedContent content = new FormUrlEncodedContent(pairs);
        retry:
            retries++;
            HttpResponseMessage test = client.PostAsync("https://www.wakfu.com/fr/mmorpg/jouer", content).Result;
            string contentString = test.Content.ReadAsStringAsync().Result;
            if (contentString.Contains("Proxy") && retries <= 5)
                goto retry;


            if (contentString.Contains("registration confirmed"))
            {
                Account acc = new Account(Username, Password, Email);
                DebugHelper.Out(TaskName, "account created succesfully !", DebugHelper.Type.Success);
            }
            else
            {
                DebugHelper.Out(TaskName, $"error occured while creating account : [{Username}:{Password}:{Email}] !", DebugHelper.Type.Error);
                Console.WriteLine(contentString);
            }
        }

        public void Dispose()
        {
        }
    }
}