using DrCaptcha.Utils.HCaptcha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrCaptcha.Recognizer.HCaptcha
{
    public class GuesserRecognizer : HCaptchaRecognizer
    {
        internal static Random random = new Random();
        public GuesserRecognizer()
        {
        }

        public string[] GrabKeywords(dynamic captcha)
        {
            return new string[] { Extra.GetKeyword(captcha) };
        }

        public string[] Recognize(dynamic task, string[] keywords)
        {
            string taskkey = task["task_key"].ToString();
            int randomNumber = random.Next(2);
            if (randomNumber == 0)
            {
                return new string[] { taskkey, "true" };
            }
            else
            {
                return new string[] { taskkey, "false" };
            }

        }
    }
}
