using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DrCaptcha.Utils.HCaptcha;

namespace DrCaptcha
{
    public class HCaptcha
    {
        public static async Task<string> Solve(string website, string sitekey, string host, string widgetid, WebProxy Proxy = null)
        {
            HttpClient Client = new HttpClient(new HttpClientHandler() { UseCookies = true, CookieContainer = new CookieContainer() });
            if (Proxy != null)
            {
                Client = new HttpClient(new HttpClientHandler() { UseCookies = true, CookieContainer = new CookieContainer(), UseProxy = true, Proxy = Proxy });
            }
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            Client.DefaultRequestHeaders.Add("Origin", "https://newassets.hcaptcha.com");
            Client.DefaultRequestHeaders.Add("Referer", "https://newassets.hcaptcha.com/");
            string version = API.GetVersion(Client).Result;
            dynamic captcha = null;
            bool validcaptcha = false;
            string hsw = "";
            string c = "";
            string motiondata = "";
            while (!validcaptcha)
            {
                dynamic sitedata = API.CheckSiteKey(Client, version, host, sitekey).Result;
                string siten = sitedata["c"]["req"].ToString();
                c = "{\"type\":\"hsw\",\"req\":\"" + siten + "\"}";
                hsw = API.GetHsw(Client, siten).Result;
                motiondata = Extra.GetMotionData(website, widgetid);
                captcha = API.GetCaptcha(Client, version, host, sitekey, c, hsw, motiondata).Result;
                if (captcha["request_type"].ToString() == "image_label_binary") { validcaptcha = true; }
            }
            string key = captcha["key"];
            string captchan = captcha["c"]["req"].ToString();
            c = "{\"type\":\"hsw\",\"req\":\"" + captchan + "\"}";
            hsw = API.GetHsw(Client, captchan).Result;
            string question = captcha["requester_question"]["en"].ToString();
            string searchText = "Please click each image containing a ";
            int startIndex = question.IndexOf(searchText);
            string keyword = question.Substring(startIndex + searchText.Length);
            Dictionary<string, string> answers = new Dictionary<string, string>();
            List<Task<string[]>> tasks = new List<Task<string[]>>();
            foreach (dynamic task in captcha["tasklist"])
            {
                string[] answer = AI.Recognise(task, keyword);
                answers.Add(answer[0], answer[1]);
            }
            dynamic captcharesponse = API.SubmitCaptcha(Client, version, host, sitekey, key, c, hsw, motiondata, answers).Result;
            if (captcharesponse["pass"] == true)
            {
                return captcharesponse["generated_pass_UUID"];
            }
            else
            {
                return "";
            }
        }
    }
}
