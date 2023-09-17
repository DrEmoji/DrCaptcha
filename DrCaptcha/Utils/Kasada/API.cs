using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DrCaptcha.Utils.Kasada
{
    internal class API
    {
        public static string CreateCT()
        {
            Console.WriteLine("Solving Kasada");
            var request = WebRequest.Create($"http://85.215.165.154:9104/ct_solutions");
            request.Method = "GET";
            request.ContentType = "application/json";
            var response = (HttpWebResponse)request.GetResponse();
            var responseStream = response.GetResponseStream();
            var reader = new StreamReader(responseStream);
            var jsonResponse = reader.ReadToEnd();
            dynamic jsondata = JsonConvert.DeserializeObject(jsonResponse);
            Console.WriteLine("Kasada Solved");
            return jsondata[0]["ct_solution"].ToString();
        }
    }
}
