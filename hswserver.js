const puppeteer = require('puppeteer');
const express = require('express');
const fetch = require('node-fetch'); // Add this line to import the 'fetch' function
const bodyParser = require('body-parser');

const app = express();
let browser = null;
let page = null;

app.use(bodyParser.json());

async function main() {
  browser = await puppeteer.launch({
    headless: false,
    args: [
      '--no-sandbox',
      '--disable-setuid-sandbox',
      '--no-first-run',
      '--disable-blink-features=AutomationControlled',
    ],
  });
  
  page = await browser.newPage(); // Move this line here to create the page instance
  
  script = await page.evaluate(async () => {
    const response = await fetch('https://newassets.hcaptcha.com/c/31892fb/hsw.js');
    return response.text();
  });

  await page.goto('https://example.com/');

  await page.evaluate(async (script) => {
    const response = await fetch('https://newassets.hcaptcha.com/c/31892fb/hsw.js');
    window.eval(script); // Evaluate the fetched script
  }, script);

  const server = app.listen(5000, () => { // Adjust the port number here
    console.log('Server is running on port 5000');
  });

}

app.post('/hsw', async (req, res) => {
  const { request } = req.body;
  
  // You can now use the 'request' variable in your code as needed
  const resultString = await page.evaluate((request) => {
    return hsw(request);
  }, request);

  const data = {
    'hsw': resultString,
  };

  res.json(data);
});

main()




