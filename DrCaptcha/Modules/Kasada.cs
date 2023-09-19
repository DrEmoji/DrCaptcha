using DrCaptcha.Recognizer.HCaptcha;
using DrCaptcha.Utils.HCaptcha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrCaptcha.Modules
{
    public class KasadaSolver
    {
        /*
        private string apikey { get; set; }
        public KasadaSolver(string apiKey)
        {
            apikey = apiKey;
        }
        */

        public Dictionary<string,string> Solve()
        {
            return new Dictionary<string, string>
            {
                {"x-kpsdk-cd'", Utils.Kasada.Extra.GetKpsdkCd() },
                {"x-kpsdk-ct", ""},
                {"user-agent", ""}
            };
        }
    }
}
