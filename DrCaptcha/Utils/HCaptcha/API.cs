using DrCaptcha.Models;
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
        internal static string apiKey = "6aa68b6bfc434896b6e48195ef8a2899";
        internal static string modelId = "aaa03c23b3724a16a56b629203edc62c";
        internal static string PAT = "6d55ecadf5914b1e883af844d9f8e5e9";
        internal static string PosmodelId = "general-image-detection";
        internal static string PosModelVersion = "1580bb1932594c93b7e2e04456af7c6f";
        public static async Task<dynamic> CheckSiteKey(HttpClient Client, string version, string host, string sitekey)
        {
            HttpResponseMessage response = await Client.PostAsync($"https://api2.hcaptcha.com/checksiteconfig?v={version}&host={host}&sitekey={sitekey}&sc=1&swa=1&spst=0", null);
            return JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
        }

        public static async Task<string> GetHsw(HttpClient Client, string req)
        {
            string payload = JsonConvert.SerializeObject(new
            {
                script = "https://newassets.hcaptcha.com/c/0d3295f3/hsw.js",
                req = req
            });
            var content = new StringContent(payload, Encoding.UTF8, "application/json");
            HttpResponseMessage response = await Client.PostAsync(new Uri("https://hcaptcha.vxxx.cf/hsw"), content);
            return JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result)["result"];
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

            WebRequest request = WebRequest.Create($"https://api.clarifai.com/v2/models/{modelId}/outputs");
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

        public static int[] RecognisePos(dynamic task, string keyword)
        {
            string imagelink = task["datapoint_uri"].ToString();
            string taskkey = task["task_key"].ToString();
            string payload = JsonConvert.SerializeObject(new
            {
                user_app_id = new
                {
                    user_id = "clarifai",
                    app_id = "main"
                },
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

            WebRequest request = WebRequest.Create($"https://api.clarifai.com/v2/models/{PosmodelId}/versions/{PosModelVersion}/outputs");
            request.Method = "POST";
            request.Headers.Add("Authorization", "Key " + PAT);
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
                Console.WriteLine(jsonResponse);
                Console.WriteLine();
                foreach (dynamic region in jsonResponse["outputs"][0]["data"]["regions"])
                {
                    string name = region["data"]["concepts"][0]["name"].ToString().ToLower();
                    if (keyword.Contains(name) || keyword == name) 
                    {
                        dynamic bbox = region["region_info"]["bounding_box"];
                        Console.WriteLine(bbox);
                        return Extra.FindRelativePosition(bbox);
                    }
                }
                return new int[0];
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new int[0];
            }
        }

        public static async Task<dynamic> SubmitBinaryCaptcha(HttpClient Client, string version, string host, string sitekey, string key, string c, string n, string motiondata, Dictionary<string, string> answers)
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

        public static async Task<dynamic> SubmitAreaCaptcha(HttpClient Client, string version, string host, string sitekey, string key, string c, string n, string motiondata, Dictionary<string, Entity> answers)
        {
            string link = $"https://hcaptcha.com/checkcaptcha/{sitekey}/{key}";
            string payload = JsonConvert.SerializeObject(new
            {
                answers = answers,
                c = c,
                job_mode = "image_label_area_select",
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
