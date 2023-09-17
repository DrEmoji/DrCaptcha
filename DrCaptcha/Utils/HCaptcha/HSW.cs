using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DrCaptcha.Utils.HCaptcha
{
    public class HSW
    {
        private IWebDriver driver;
        private string script = "";

        public HSW()
        {
            Initialize();
        }

        private void Initialize()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("--headless");

            driver = new ChromeDriver(chromeOptions);

            driver.Navigate().GoToUrl("https://example.com");

            script = new System.Net.WebClient().DownloadString("https://newassets.hcaptcha.com/c/31892fb/hsw.js");
        }

        public string GetRequest(string request)
        {

            // Execute the HSW function in the browser context
            var result = ((IJavaScriptExecutor)driver).ExecuteScript($"{script}; return hsw('{request}')");

            // Convert the result to a string
            string resultString = result as string;

            return resultString;
        }

        public void Dispose()
        {
            // Close and dispose of the driver when done
            driver.Quit();
            driver.Dispose();
        }
    }
}
