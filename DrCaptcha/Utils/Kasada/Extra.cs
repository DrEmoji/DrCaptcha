using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DrCaptcha.Utils.Kasada
{
    class Config
    {
        public string PlatformInputs { get; set; }
        public double Difficulty { get; set; }
        public int SubchallengeCount { get; set; }
    }
    internal class Extra
    {
        const string HEX_CHARS = "0123456789abcdef";

        static double GetHashDifficulty(string hashString)
        {
            return 4503599627370496 / (Convert.ToInt64(hashString.Substring(0, 13), 16) + 1);
        }

        static long GetTimeNow()
        {
            long timeNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            return timeNow;
        }

        static string StringToSHA256(string stringTo)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(stringTo);
                byte[] hashBytes = sha256.ComputeHash(bytes);
                StringBuilder builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        static string RandomString(int k = 32)
        {
            Random random = new Random();
            StringBuilder result = new StringBuilder();
            for (int i = 0; i < k; i++)
            {
                result.Append(HEX_CHARS[random.Next(HEX_CHARS.Length)]);
            }
            return result.ToString();
        }

        static (List<long> answers, string finalHash) GetProfOfWork(Config config, string id, long workTime)
        {
            List<long> fList = new List<long>();
            double difficulty = config.Difficulty / config.SubchallengeCount;
            string startString = StringToSHA256(config.PlatformInputs + ", " + workTime + ", " + id);
            for (int i = 0; i < config.SubchallengeCount; i++)
            {
                long d = 1;
                while (true)
                {
                    string startString2 = StringToSHA256(d + ", " + startString);
                    if (GetHashDifficulty(startString2) >= difficulty)
                    {
                        fList.Add(d);
                        startString = startString2;
                        break;
                    }
                    d++;
                }
            }
            return (fList, startString);
        }

        public static dynamic GetKpsdkCd()
        {
            Config config = new Config
            {
                PlatformInputs = "tp-v2-input",
                Difficulty = 10,
                SubchallengeCount = 2
            };

            DateTimeOffset t0 = DateTimeOffset.UtcNow;
            string id = RandomString();
            long workTime = GetTimeNow() - 527;
            (List<long> answers, string finalHash) profOfWork = GetProfOfWork(config, id, workTime);
            DateTimeOffset t1 = DateTimeOffset.UtcNow;

            TimeSpan duration = t1 - t0;

            var result = new
            {
                answers = profOfWork.answers,
                rst = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                st = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                d = duration.TotalMilliseconds,
                id = id,
                workTime = workTime
            };

            return JsonConvert.SerializeObject(result);
        }
    }
}
