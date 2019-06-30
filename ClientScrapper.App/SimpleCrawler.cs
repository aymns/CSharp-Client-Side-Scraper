namespace ClientScrapper.App
{
	using System;
	using ClientScrapper;
	using Arguments;
	using Utils;

    /// <summary>
    /// Represent a simple crawler
    /// </summary>
	public class SimpleCrawler : ClientCrawler
	{
		public SimpleCrawler(string startupUrl, ILogger logger, ClientCrawlerConfiguration crawlerConfiguration) : base(startupUrl, logger, crawlerConfiguration)
		{
		}

		protected override void OnCrawlerProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e)
		{
			Console.WriteLine(e.Driver.PageSource);
		}

		protected override bool ShouldCrawlLink(string link)
		{
			return true;
		}
	}
}