using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DrCaptcha.Utils.HCaptcha
{
    internal class API
    {
        internal static string apiKey = "f4ff298a4a304ac18b0145456afa17ea";
        internal static string modelId = "general-image-recognition";
        internal static string modelversion = "aa7f35c01e0642fda5cf400f543e7c40";
        public static async Task<dynamic> CheckSiteKey(HttpClient Client, string version, string host, string sitekey)
        {
            HttpResponseMessage response = await Client.PostAsync($"https://api2.hcaptcha.com/checksiteconfig?v={version}&host={host}&sitekey={sitekey}&sc=1&swa=1&spst=0", null);
            return JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
        }

        public static async Task<string> GetVersion(HttpClient Client)
        {
            HttpResponseMessage response = await Client.GetAsync("https://hcaptcha.com/1/api.js");
            if (response.IsSuccessStatusCode)
            {
                string data = await response.Content.ReadAsStringAsync();
                string pattern = @"https:\/\/newassets.hcaptcha.com\/captcha\/v1\/(\w+)\/static";
                Match match = Regex.Match(data, pattern);
                if (match.Success && match.Groups.Count > 1)
                {
                    return match.Groups[1].Value;
                }
            }
            return "";
        }

        public static async Task<dynamic> GetCaptcha(HttpClient Client, string version, string host, string sitekey, string c, string n, string motiondata)
        {

            var regdata = new[]
            {
                new KeyValuePair<string, string>("v",version),
                new KeyValuePair<string, string>("sitekey", sitekey),
                new KeyValuePair<string, string>("host", host),
                new KeyValuePair<string, string>("hl", "en"),
                new KeyValuePair<string, string>("c", c),
                new KeyValuePair<string, string>("n", n),
                new KeyValuePair<string, string>("motionData", motiondata)
            };

            HttpResponseMessage response = await Client.PostAsync($"https://hcaptcha.com/getcaptcha/{sitekey}", new FormUrlEncodedContent(regdata));
            return JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
        }

        public static string[] Recognize(dynamic task, string keyword)
        {
            string imagelink = task["datapoint_uri"].ToString();
            string taskkey = task["task_key"].ToString();
            string payload = JsonConvert.SerializeObject(new
            {
                inputs = new[]
            {
                new
                {
                    data = new
                    {
                        image = new
                        {
                            url = imagelink
                        }
                    }
                }
            }
            });

            WebRequest request = WebRequest.Create($"https://api.clarifai.com/v2/users/clarifai/apps/main/models/{modelId}/versions/{modelversion}/outputs");
            request.Method = "POST";
            request.Headers.Add("Authorization", "Key " + apiKey);
            request.ContentType = "application/json";

            using (var streamWriter = new StreamWriter(request.GetRequestStream()))
            {
                streamWriter.Write(payload);
            }

            try
            {
                WebResponse response = request.GetResponse();
                var streamReader = new StreamReader(response.GetResponseStream());
                string responseContent = streamReader.ReadToEnd();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                foreach (dynamic concept in jsonResponse["outputs"][0]["data"]["concepts"])
                {
                    bool result = keyword.Contains(concept["name"].ToString());
                    if (result)
                    {
                        return new string[] { taskkey, "true" };
                    };
                }
                return new string[] { taskkey, "false" };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new string[] { taskkey, "false" };
            }
        }

        public static async Task<string> GetHsw(HttpClient Client, string req)
        {
            string payload = JsonConvert.SerializeObject(new
            {
                request = req
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PostAsync(new Uri("http://127.0.0.1:5000/hsw"), content);
            return JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result)["hsw"];
        }

        public static async Task<dynamic> SubmitCaptcha(HttpClient Client, string version, string host, string sitekey, string key, string c, string n, string motiondata, Dictionary<string, string> answers)
        {
            string link = $"https://hcaptcha.com/checkcaptcha/{sitekey}/{key}";
            string payload = JsonConvert.SerializeObject(new
            {
                answers = answers,
                c = c,
                job_mode = "image_label_binary",
                n = n,
                serverdomain = host,
                motionData = motiondata,
                sitekey = sitekey,
                v = version
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PostAsync(link, content);
            return JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
        }
    }
}
