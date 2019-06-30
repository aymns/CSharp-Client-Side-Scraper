namespace ClientScrapper
{
	using OpenQA.Selenium.Chrome;

    /// <summary>
    /// Represent <see cref="ClientCrawler"/> configuration, where all the configuration to manage the crawler.
    /// </summary>
	public class ClientCrawlerConfiguration
	{
		public ChromeOptions ChromeDriverConfiguration { get; }
	    public int ErrorThreshold { get; set; } = 30;
	    public int MaxThread { get; set; } = 20;
	    public bool ForceStopOnErrorThresholdReached { get; set; }

        public ClientCrawlerConfiguration(bool headlessBrowser = false)
		{
			this.ChromeDriverConfiguration = new ChromeOptions();

			if (headlessBrowser)
				this.ChromeDriverConfiguration.AddArgument("headless");
		}

		public ClientCrawlerConfiguration(ChromeOptions chromeDriverConfiguration, int errorThreshold, int maxThread)
		{
			this.ChromeDriverConfiguration = chromeDriverConfiguration;
			this.ErrorThreshold = errorThreshold;
			this.MaxThread = maxThread;
		}
	}
}