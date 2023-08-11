using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace DrCaptcha.Utils.HCaptcha
{
    internal class AI
    {
        internal static string apiKey = "48e4bb33c47747378d4c2f9a115aa06f";
        internal static string modelId = "aaa03c23b3724a16a56b629203edc62c";

        static bool FindMatchingSubword(string keyword, string predicted)
        {

            if (predicted == keyword)
            {
                return true;
            }

            if (keyword.Contains(predicted))
            {
                return true;
            }

            for (int i = 0; i < predicted.Length; i++)
            {
                for (int j = i + 4; j <= predicted.Length; j++)
                {
                    string subword = predicted.Substring(i, j - i);
                    if (subword.Length > 4 && keyword.Contains(subword))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static string[] Recognise(dynamic task, string keyword)
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
                    bool result = FindMatchingSubword(keyword, concept["name"].ToString());
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
    }
}
