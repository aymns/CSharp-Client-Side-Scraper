using System.Diagnostics;
using ClientScrapper.Arguments;
using HtmlAgilityPack;

namespace ClientScrapper.Test
{
    public class ClientCrawlerSample : ClientCrawler
    {
        public ClientCrawlerSample(string link) : base(
            link,
            new Logger(),
            new ClientCrawlerConfiguration())
        {
        }

        public int CrawledPageCount { get; set; }

        protected override void OnCrawlerProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(e.Driver.PageSource);
            Debug.WriteLine(doc.DocumentNode.SelectSingleNode("//a").InnerText);
            CrawledPageCount++;
        }

        protected override bool ShouldCrawlLink(string link)
        {
            return true;
        }
    }
}