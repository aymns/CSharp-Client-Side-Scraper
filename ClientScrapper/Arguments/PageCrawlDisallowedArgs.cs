namespace ClientScrapper.Arguments
{
	public class PageCrawlDisallowedArgs
	{
		public PageCrawlDisallowedArgs(string link)
		{
			this.Link = link;
		}

		public string Link { get; set; }
	}
}