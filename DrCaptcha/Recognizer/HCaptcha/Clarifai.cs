using DrCaptcha.Utils.HCaptcha;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrCaptcha.Recognizer.HCaptcha
{
    public class ClarifaiRecognizer : HCaptchaRecognizer
    {
        public static string apiKey { get; set; }
        public static float threshold { get; set; }
        internal static string modelId = "general-image-recognition";
        internal static string modelversion = "aa7f35c01e0642fda5cf400f543e7c40";
        internal static List<string> GeneralKeywords = new List<string> { "still life", "summer", "square", "design", "window", "monochrome", "no person", "isolated", "glazed", "reflection", "bright", "shape", "isolated", "dark", "indoors", "decoration", "image", "art", "round out", "one", "delicious" };

        public ClarifaiRecognizer(string apikey, float Threshold = 0.97f)
        {
            apiKey = apikey ?? string.Empty;
            threshold = Threshold;
        }

        public string[] GrabKeywords(dynamic captcha)
        {
            string imagelink = captcha["requester_question_example"][0].ToString();
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
                List<string> keywords = new List<string>();
                //string questionkeyword = Extra.GetKeyword(captcha);
                //Console.WriteLine(questionkeyword);
                keywords.Add(Extra.GetKeyword(captcha));
                foreach (dynamic concept in jsonResponse["outputs"][0]["data"]["concepts"])
                {
                    if (float.Parse(concept["value"].ToString()) >= threshold && !GeneralKeywords.Contains(concept["name"].ToString()))
                    {
                        keywords.Add(concept["name"].ToString());
                        //Console.WriteLine(concept["name"].ToString());
                    }
                }
                return keywords.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new string[] { Extra.GetKeyword(captcha) };
            }
        }

        public string[] Recognize(dynamic task, string[] keywords)
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
                    foreach (string keyword in keywords)
                    {
                        if (keyword.Contains(concept["name"].ToString()))
                        {
                            return new string[] { taskkey, "true" };
                        }
                    }
                }
                return new string[] { taskkey, "false" };
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return new string[] { taskkey, "false" };
            }
        }
    }
}
