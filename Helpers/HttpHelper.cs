using AnkamaAccGen.Managers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AnkamaAccGen.Helper
{
    public class HttpHelper
    {
        public static dynamic Post(Uri url, string post, out string error)
        {
            error = null;
            dynamic result = null;
            var postBody = Encoding.UTF8.GetBytes(post);
            var request = (HttpWebRequest)WebRequest.Create(url);

            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = postBody.Length;
            request.Timeout = 30000;

            try
            {
                using (var stream = request.GetRequestStream())
                {
                    stream.Write(postBody, 0, postBody.Length);
                    stream.Close();
                }

                using (var response = (HttpWebResponse)request.GetResponse())
                {
                    var strreader = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                    result = JsonConvert.DeserializeObject(strreader.ReadToEnd());
                    response.Close();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

            return result;
        }

        internal static dynamic PostViaWebClient(string task, Uri url, string post, out string error)
        {
            error = null;
            dynamic result = null;

            try
            {
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(url.ToString());
                httpWebRequest.ContentType = "application/json";
                httpWebRequest.Method = "POST";

                using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
                {
                    streamWriter.Write(post);
                }

                var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    result = streamReader.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                error = ex.Message;

                return false;
            }

            return result;
        }

        public static async Task<string> EmailValidation(Account acc, string link)
        {
            var proxy = new WebProxy
            {
                Address = new Uri($"http://megaproxy.rotating.proxyrack.net:10000"),
                Credentials = new NetworkCredential(userName: "klaasvaakjes-country-FR-refreshSeconds-1", password: "ef4c02-aab4d8-4c59a0-555771-c9188b")
            };
            int retries = 0;
            var httpClientHandler = new HttpClientHandler
            {
                Proxy = proxy,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            };
            HttpClient client = new HttpClient(httpClientHandler);
            Dictionary<string, string> Header = new Dictionary<string, string>
            {
                { "accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8" },
                { "accept-language", "fr,fr-FR;q=0.8,en-US;q=0.5,en;q=0.3" },
                { "accept-encoding", "gzip, deflate, br" },
                { "Connection", "keep-alive" },
                { "user-agent",  "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:85.0) Gecko/20100101 Firefox/85.0"}
            };
            foreach (var header in Header)
            {
                client.DefaultRequestHeaders.Add(header.Key, header.Value);
            }
        retry:
            retries++;
            var message = await client.GetAsync(link);
            var result = await message.Content.ReadAsStringAsync();
            if (!result.Contains("terminée à 100%") && retries >= 5)
                goto retry;

            return result.ToString();
        }
    }
}