using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrCaptcha.Recognizer.HCaptcha
{
    public interface HCaptchaRecognizer
    {
        string[] GrabKeywords(dynamic captcha);

        string[] Recognize(dynamic task, string[] keywords);
    }
}