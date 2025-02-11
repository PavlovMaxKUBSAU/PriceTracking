using System.Net.Mail;
using System.Net;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp;
using HtmlAgilityPack;
using Microsoft.EntityFrameworkCore;
using PriceTracking.Models;
using System.Net;
using System.Net.Mail;
using AngleSharp.Browser;
using Microsoft.AspNetCore.Http;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using PuppeteerSharp;

namespace PriceTracking
{
    public class PriceTrackingService
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public PriceTrackingService(AppDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        public async Task<Guid> SubscribeAsync(string adUrl, string email)
        {
            Subscription subscription = new Subscription { AdUrl = adUrl, Email = email };
            await _context.Subscriptions.AddAsync(subscription);
            await _context.SaveChangesAsync();

            // Отправляем уведомление (текущая цена)
            var currentPrice = await GetCurrentPriceAsync(adUrl);
            subscription.PreviousPrice = currentPrice;
            await SendNotificationAsync(subscription);

            return subscription.Id;
        }

        public async Task<decimal?> GetCurrentPriceAsync(string adUrl)
        {
            ///AngleSharp не загружает js
            string html = await _httpClient.GetStringAsync(adUrl);
            //string html = "<div class=\"flat-prices__block-current 1 PRICE\">5 115 000 ₽</div>"; //вот так все парсит прекрасно. А в тексте с сайта этой строки нет (не отрабатывает js)
            var parser = new HtmlParser(new HtmlParserOptions() { IsScripting = true });
            var document = parser.ParseDocument(html);
            IElement priceElement = document.QuerySelector(".flat-prices__block-current.PRICE");

            //IElement? priceElement; //не загружает js
            //using (var browser = new BrowsingContext())
            //{
            //    // Загружаем страницу
            //    var document = await browser.OpenAsync(adUrl);

            //    // Ждем, пока страница полностью загрузится и JavaScript выполнится
            //    await document.WaitForReadyAsync(); //.WaitForJavaScriptAsync();

            //    // Теперь вы можете извлечь данные со страницы
            //    priceElement = document.QuerySelector(".flat-prices__block-current.PRICE");
            //}




            ///Selenium
            //IWebDriver driver = new ChromeDriver();

            //// Navigate to the dynamic website
            //driver.Navigate().GoToUrl("adUrl");

            //// Wait for the dynamic content to load
            //WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            //wait.Until(d => d.FindElement(By.Name(".flat-prices__block-current.PRICE")));

            //// Extract the dynamic content
            //var priceElement = driver.FindElement(By.Name(".flat-prices__block-current.PRICE")).Text;

            //// Close the browser
            //driver.Quit();


            ///Puppeteer не работает
            //var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true });
            //var page = await browser.NewPageAsync();
            //await page.GoToAsync(adUrl);

            //var priceElement = await page.EvaluateExpressionAsync<string>("document.querySelector('.flat-prices__block-current.PRICE').innerText");

            //await browser.CloseAsync();




            //using (StreamWriter writer = new StreamWriter("example.txt"))
            //{
            //    writer.WriteLine(html);
            //}
            if (priceElement != null)
            {
                string price = priceElement.TextContent.Trim();
                return decimal.Parse(price.Replace(" ", "").Trim('₽'));
            }
            else return null;
        }

        public async Task<List<Subscription>> GetSubscriptionsAsync(string email)
        {
            var subscriptions = await _context.Subscriptions.Where(x => x.Email == email).ToListAsync(); //Select(x => new {x.AdUrl, x.PreviousPrice})

            return subscriptions;
        }

        public async Task SendNotificationAsync(Subscription subscription)
        {
            var mailMessage = new MailMessage
            {
                From = new MailAddress("testemail@mail.ru", "Имя") //почта и имя отправителя
            };

            mailMessage.To.Add(subscription.Email);
            mailMessage.Subject = "Изменилась цена!!!";
            mailMessage.Body = $"Ссылка: {subscription.AdUrl},\nНовая цена: {subscription.PreviousPrice}";


            var smtpClient = new SmtpClient("smtp.mail.ru")
            {
                Port = 587,
                Credentials = new NetworkCredential("testemail@mail.ru", "пароль"), //почта и пароль отправителя
                EnableSsl = true
            };
            smtpClient.Send(mailMessage);
        }

        public async Task CheckPriceChangesAsync()
        {
            var subscriptions = await _context.Subscriptions.ToListAsync();

            foreach (var subscription in subscriptions)
            {
                var currentPrice = await GetCurrentPriceAsync(subscription.AdUrl);

                if (currentPrice.HasValue && subscription.PreviousPrice != currentPrice)
                {
                    subscription.PreviousPrice = currentPrice;
                    await _context.SaveChangesAsync();
                    await SendNotificationAsync(subscription);
                }
            }
        }
    }
}
