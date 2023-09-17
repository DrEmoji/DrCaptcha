using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DrCaptcha.Utils.HCaptcha
{
    internal class Extra
    {
        private static char[] numbers = "123456789".ToCharArray();
        public static string[] BlacklistedCaptchas = { "something you can eat" };
        public static long GetTimestamp()
        {
            return ((DateTimeOffset)DateTime.UtcNow.ToUniversalTime()).ToUniversalTime().ToUnixTimeMilliseconds();
        }

        public static string GetMovement()
        {
            return "[" + GetUniqueInt(50, 150) + "," + GetUniqueInt(7, 40) + "," + (GetTimestamp() + GetUniqueInt(750, 950)) + "]";
        }

        public static string GetMotionData(string page, string widgetId)
        {
            int steps = GetUniqueInt(20, 25);
            int otherSteps = GetUniqueInt(15, 33);

            string movements = "", otherMovements = "";

            for (int i = 0; i < steps; i++)
            {
                if (movements == "")
                {
                    movements += GetMovement();
                }
                else
                {
                    movements += "," + GetMovement();
                }
            }

            for (int i = 0; i < otherSteps; i++)
            {
                if (otherMovements == "")
                {
                    otherMovements += GetMovement();
                }
                else
                {
                    otherMovements += "," + GetMovement();
                }
            }

            return "{\"st\":" + GetTimestamp() + ",\"mm\":[" + movements + "]," +
                "\"mm-mp\":" + GetUniqueInt(9, 13) + "." + GetUniqueInt(31513, 49593) + GetUniqueInt(73123, 98111) +
                GetUniqueInt(16432, 56321) + "," +
                "\"md\":[" + GetMovement() + "],\"md-mp\":0,\"mu\":[" + GetMovement() + "],\"mu-mp\":0,\"v\":1," +
                "\"topLevel\":{\"st\":" + GetTimestamp() + GetUniqueInt(1000, 3000) + ",\"sc\":{\"availWidth\":1920," +
                "\"availHeight\":1040," +
                "\"width\":1920,\"height\":1080,\"colorDepth\":24,\"pixelDepth\":24,\"availLeft\":0,\"availTop\":0}" +
                ",\"nv\":{\"vendorSub\":\"\",\"productSub\":\"20030107\",\"vendor\":\"Google Inc.\",\"maxTouchPoints\":0," +
                "\"userActivation\":{},\"doNotTrack\":null,\"geolocation\":{},\"connection\":{},\"webkitTemporaryStorage\":{}," +
                "\"webkitPersistentStorage\":{},\"hardwareConcurrency\":12,\"cookieEnabled\":true,\"appCodeName\":\"Mozilla\"," +
                "\"appName\":\"Netscape\"," +
                "\"appVersion\":\"5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko)" +
                "Chrome/96.0.4664.110 Safari/537.36\",\"platform\":\"Win32\",\"product\":\"Gecko\"," +
                "\"userAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) " +
                "Chrome/96.0.4664.110 Safari/537.36\",\"language\":\"it-IT\",\"languages\":[\"it-IT\",\"it\",\"en-US\",\"en\"]," +
                "\"onLine\":true,\"webdriver\":false,\"pdfViewerEnabled\":true,\"scheduling\":{},\"bluetooth\":{},\"clipboard\":{}," +
                "\"credentials\":{},\"keyboard\":{},\"managed\":{},\"mediaDevices\":{},\"storage\":{},\"serviceWorker\":{}," +
                "\"wakeLock\":{},\"deviceMemory\":8,\"ink\":{},\"hid\":{},\"locks\":{},\"mediaCapabilities\":{},\"mediaSession\":{}," +
                "\"permissions\":{},\"presentation\":{},\"serial\":{},\"virtualKeyboard\":{},\"usb\":{},\"xr\":{}," +
                "\"userAgentData\":{\"brands\":[{\"brand\":\" Not A;Brand\",\"version\":\"99\"},{\"brand\":\"Chromium\"," +
                "\"version\":\"96\"},{\"brand\":\"Google Chrome\",\"version\":\"96\"}],\"mobile\":false}," +
                "\"plugins\":[\"internal-pdf-viewer\",\"internal-pdf-viewer\",\"internal-pdf-viewer\",\"internal-pdf-viewer\"," +
                "\"internal-pdf-viewer\"]},\"dr\":\"\",\"inv\":false,\"exec\":false,\"wn\":[[1920,970,1," + (GetTimestamp() +
                GetUniqueInt(1000, 3000)) + "]]," +
                "\"wn-mp\":0,\"xy\":[[0,0,1," + (GetTimestamp() + GetUniqueInt(1000, 3000)) + "]]," +
                "\"xy-mp\":0,\"mm\":[" + otherMovements + "],\"mm-mp\":" + GetUniqueInt(3, 12) + "." + GetUniqueInt(31513, 49593) +
                GetUniqueInt(73123, 98111) + GetUniqueInt(16432, 56321) +
                "},\"session\":[],\"widgetList\":[\"" + widgetId + "\"],\"widgetId\":\"" + widgetId + "\",\"href\":\"" + page + "\"," +
                "\"prev\":{\"escaped\":false,\"passed\":false,\"expiredChallenge\":false,\"expiredResponse\":false}" +
                "}";
        }

        public static string GetHost(string url)
        {
            string host = url.Split('/')[2];
            string[] sub = host.Split('.');
            if (sub.Length > 2)
            {
                host = $"{sub[1]}.{sub[2]}";
            }
            return host;
        }

        public static HttpClient CreateClient(WebProxy Proxy = null)
        {
            HttpClient Client = new HttpClient(new HttpClientHandler() { UseCookies = true, CookieContainer = new CookieContainer() });
            if (Proxy != null)
            {
                Client = new HttpClient(new HttpClientHandler() { UseCookies = true, CookieContainer = new CookieContainer(), UseProxy = true, Proxy = Proxy });
            }
            Client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/115.0.0.0 Safari/537.36");
            Client.DefaultRequestHeaders.Add("Origin", "https://newassets.hcaptcha.com");
            Client.DefaultRequestHeaders.Add("Referer", "https://newassets.hcaptcha.com/");
            return Client;
        }
        public static string GetKeyword(dynamic captcha)
        {
            string question = captcha["requester_question"]["en"].ToString();
            if (question.Contains("each"))
            {
                string searchText = "Please click each image containing a";
                int startIndex = question.IndexOf(searchText);
                string keyword = question.Substring(startIndex + searchText.Length);
                return keyword;
            }
            else
            {
                return question.Split(' ')[4];
            }
        }

        public static int GetUniqueInt(int min, int max)
        {
            int theResult = 0;

            while (theResult > max || theResult == 0 || theResult < min)
            {
                byte[] data = new byte[4 * max.ToString().Length];

                using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
                {
                    crypto.GetBytes(data);
                }

                StringBuilder result = new StringBuilder(max.ToString().Length);

                for (int i = 0; i < max.ToString().Length; i++)
                {
                    var rnd = BitConverter.ToUInt32(data, i * 4);
                    var idx = rnd % numbers.Length;

                    result.Append(numbers[idx]);
                }

                theResult = int.Parse(result.ToString());
            }

            return theResult;
        }
    }
}
