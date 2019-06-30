using OpenQA.Selenium.Chrome;

namespace ClientScrapper.App
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            const string starterUrl = "http://google.com";
            const int errorThreshold = 10;
            const int maxThread = 10;

            // create new instance of Logger logic
            var logger = new Logger();

            // configure chrome driver
            var chromeDriverConfiguration = new ChromeOptions();
            chromeDriverConfiguration.AddArgument("headless");

            // client crawler configuration
            var clientCrawlerConfiguration =
                new ClientCrawlerConfiguration(chromeDriverConfiguration, errorThreshold, maxThread);

            // new instance of ClientCrawler
            var crawler = new SimpleCrawler(starterUrl, logger, clientCrawlerConfiguration);

            // start crawling
            crawler.Start();
        }
    }
}