using DrCaptcha.Utils.HCaptcha;
using System.Collections.Generic;
using System.Net.Http;
using System.Net;

namespace DrCaptcha.Modules
{
    namespace DrCaptcha.Modules
    {
        public class HCaptchaSolver
        {
            static string widgetid = "0gdec1jq6oa";
            static HSW HSWSolver = new HSW();
            public WebProxy WebProxy { get; set; }
            public HCaptchaSolver(WebProxy proxy = null)
            {
                if (proxy != null) WebProxy = proxy;
            }

            public string Solve(string website, string sitekey)
            {
                HttpClient Client = null;
                if (WebProxy != null)
                    Client = Extra.CreateClient(WebProxy);
                else
                    Client = Extra.CreateClient();
                string host = Extra.GetHost(website);
                string version = API.GetVersion(Client).Result;
                dynamic captcha;
                string motionData = Extra.GetMotionData(website, widgetid);
                string c, hsw;
                while (true)
                {
                    dynamic siteData = API.CheckSiteKey(Client, version, host, sitekey).Result;
                    string siteReq = siteData["c"]["req"].ToString();
                    c = $"{{\"type\":\"hsw\",\"req\":\"{siteReq}\"}}";
                    hsw = HSWSolver.GetRequest(siteReq);
                    captcha = API.GetCaptcha(Client, version, host, sitekey, c, hsw, motionData);
                    if (captcha["request_type"].ToString() == "image_label_binary") break;
                }
                string key = captcha["key"];
                string captchaReq = captcha["c"]["req"].ToString();
                c = $"{{\"type\":\"hsw\",\"req\":\"{captchaReq}\"}}";
                hsw = HSWSolver.GetRequest(captchaReq);
                string keyword = Extra.GetKeyword(captcha);
                dynamic captchaResponse;
                Dictionary<string, string> answers = new Dictionary<string, string>();
                foreach (dynamic task in captcha["tasklist"])
                {
                    string[] answer = API.Recognize(task, keyword);
                    answers.Add(answer[0], answer[1]);
                }
                captchaResponse = API.SubmitCaptcha(Client, version, host, sitekey, key, c, hsw, motionData, answers).Result;
                if (captchaResponse["pass"] == true)
                    return captchaResponse["generated_pass_UUID"];
                else
                    return Solve(website, sitekey);
            }
        }
    }
}