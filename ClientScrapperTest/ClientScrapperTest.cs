namespace ClientScrapper.Test
{
	using System;
	using Microsoft.VisualStudio.TestTools.UnitTesting;

    [TestClass]
	public class ClientScrapperTest
	{
		[TestMethod]
		public void CrawlTest()
		{
			string link = AppDomain.CurrentDomain.BaseDirectory + @"\Testing.html";

            using (var crawler = new ClientCrawlerSample(link))
			{
				crawler.Start();

				Assert.AreEqual(crawler.CrawledPageCount, 3);
			}
		}
	}
}