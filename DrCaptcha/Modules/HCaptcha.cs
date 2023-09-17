using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DrCaptcha.Utils.HCaptcha;
using Newtonsoft.Json.Linq;
using DrCaptcha.Models;

namespace DrCaptcha
{
    public class HCaptcha
    {
        internal static string widgetid = "0gdec1jq6oa";
        internal static HSW HSWSolver = new HSW();
        public static string Solve(string website, string sitekey, WebProxy Proxy = null)
        {
            HttpClient Client = Extra.CreateClient(Proxy);
            string host = Extra.GetHost(website);
            string version = API.GetVersion(Client).Result;
            Console.WriteLine(version);
            dynamic captcha = null;
            string hsw = "";
            string c = "";
            bool validcaptcha = false;  
            string motiondata = "";
            while (!validcaptcha)
            {
                dynamic sitedata = API.CheckSiteKey(Client, version, host, sitekey).Result;
                string sitereq = sitedata["c"]["req"].ToString();
                c = "{\"type\":\"hsw\",\"req\":\"" + sitereq + "\"}";
                hsw = HSWSolver.GetRequest(sitereq);
                motiondata = Extra.GetMotionData(website, widgetid);
                captcha = API.GetCaptcha(Client, version, host, sitekey, c, hsw, motiondata).Result;
                if (captcha["request_type"].ToString() == "image_label_binary") { validcaptcha = true; }
            }
            string key = captcha["key"];
            string captchareq = captcha["c"]["req"].ToString();
            c = "{\"type\":\"hsw\",\"req\":\"" + captchareq + "\"}";
            hsw = HSWSolver.GetRequest(captchareq);
            string keyword = Extra.GetKeyword(captcha);
            Console.WriteLine(keyword);
            dynamic captcharesponse = null;
            //Dictionary<string, string> answers = new Dictionary<string, string>();
            if (captcha["request_type"].ToString() == "image_label_area_select")
            {
                string entitytype = captcha["requester_restricted_answer_set"].ToString().Split('"')[1];
                Dictionary<string, Entity> answers = new Dictionary<string, Entity>();
                foreach (dynamic task in captcha["tasklist"])
                {
                    string taskkey = task["task_key"].ToString();
                    int[] pos = API.RecognisePos(task, keyword);
                    Entity APIEntity = new Entity();
                    APIEntity.entity_type = entitytype;
                    APIEntity.entity_coords = pos;
                    answers.Add(taskkey, APIEntity);

                }
                captcharesponse = API.SubmitAreaCaptcha(Client, version, host, sitekey, key, c, hsw, motiondata, answers).Result;
            }
            else
            {
                Dictionary<string, string> answers = new Dictionary<string, string>();
                foreach (dynamic task in captcha["tasklist"])
                {
                    string[] answer = API.Recognize(task, keyword);
                    answers.Add(answer[0], answer[1]);
                }
                captcharesponse = API.SubmitBinaryCaptcha(Client, version, host, sitekey, key, c, hsw, motiondata, answers).Result;
            }
            if (captcharesponse["pass"] == true)
            {
                return captcharesponse["generated_pass_UUID"];
            }
            else
            {
                return Solve(website, sitekey, Proxy);
            }
        }
    }
}
