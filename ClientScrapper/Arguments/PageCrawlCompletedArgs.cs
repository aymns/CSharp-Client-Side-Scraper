namespace ClientScrapper.Arguments
{
	public class PageCrawlCompletedArgs
	{
		public PageCrawlCompletedArgs(ClientCrawler clientCrawler, StrictChromeDriver driver, string pageLink)
		{
			this.Context = clientCrawler;
			this.Driver = driver;
		    this.PageLink = pageLink;
		}

		public ClientCrawler Context { get; }
        public string PageLink { get; }
		public StrictChromeDriver Driver { get; }
	}
}