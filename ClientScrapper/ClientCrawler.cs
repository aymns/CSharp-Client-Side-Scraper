using System.Diagnostics;

namespace ClientScrapper
{
	using System;
	using System.Collections.Concurrent;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Arguments;
	using Utils;
	using OpenQA.Selenium.Chrome;
    using Tasker;

    /// <summary>
    /// Represent an abstract class for crawler uses chrome-drivers to fully load urls.
    /// </summary>
    public abstract class ClientCrawler : IDisposable
	{
	    private const int MillisecondsTimeoutToRecheckIfCrawlingProcessCompleted = 1000;
	    private const int MillisecondsTimeoutToWaitUntilRetryGetIdleChromeDriver = 1000;

        private readonly ILogger _logger;
		private readonly ClientCrawlerConfiguration _crawlerConfiguration;
		private readonly ConcurrentBag<StrictChromeDriver> _drivers;
		private readonly ConcurrentQueue<string> _pagesQueue;
		private readonly TaskManager _threadManager;
		private readonly ConcurrentDictionary<string, bool> _visitedLinks;
	    public event EventHandler<PageCrawlDisallowedArgs> PageCrawlDisallowed;
	    public event EventHandler<PageCrawlCompletedArgs> PageCrawledCompleted;

        /// The url where the crawler start.
	    private string StartupUrl { get; }

        /// Counter for number of errors occurred.
	    private int _errorCount;

        /// Flag represent the state of crawling, if it's true no further tasks will be added to crawler.
	    private bool _stopCrawling;


        /// <summary>
        /// Create new instance of crawler
        /// </summary>
        /// <param name="startupUrl">startup url</param>
        /// <param name="logger">logger instance</param>
        /// <param name="crawlerConfiguration">configuration</param>
        protected ClientCrawler(string startupUrl, ILogger logger, ClientCrawlerConfiguration crawlerConfiguration)
		{
			_threadManager = new TaskManager(crawlerConfiguration.MaxThread);
			this.StartupUrl = startupUrl;
			this._logger = logger;
			this._crawlerConfiguration = crawlerConfiguration;

            this._drivers = new ConcurrentBag<StrictChromeDriver>();
			this._pagesQueue = new ConcurrentQueue<string>();
			this._visitedLinks = new ConcurrentDictionary<string, bool>();

		    this.PageCrawledCompleted += this.crawlerProcessPageCrawlCompletedHandler;
		    this.PageCrawlDisallowed += this.crawlerProcessPageCrawlDisallowedHandler;
		}

        /// <summary>
        /// Dispose the crawler and remove allocated resources, this includes the chrome-driver instances.
        /// </summary>
		public void Dispose()
		{
			foreach (var driver in this._drivers)
			{
				driver.Dispose();
			}
		}

        /// <summary>
        /// Flag the crawler for force stop.
        /// </summary>
		public void ForceStopCrawling()
		{
			this._stopCrawling = true;
		}


        /// <summary>
        /// Start crawling, starting from the <see cref="StartupUrl"/>
        /// </summary>
		public void Start()
		{
			this._pagesQueue.Enqueue(this.StartupUrl);
			this._visitedLinks.TryAdd(this.StartupUrl, true);

			var completed = false;
			while (!completed)
			{
				while (this._pagesQueue.Count > 0)
				{
					this._threadManager.DoWork(
						() =>
						{
						    if (this._pagesQueue.TryDequeue(out var url))
						    {
						        this.CrawlPageAsync(url);
						    }
						});
				}

				if (this._threadManager.NumberOfRunningThreads == 0 && this._pagesQueue.IsEmpty)
				{
					completed = true;
				}
				else
				{
				    Thread.Sleep(MillisecondsTimeoutToRecheckIfCrawlingProcessCompleted);
				}
			}
		}

		protected abstract void OnCrawlerProcessPageCrawlCompleted(object sender, PageCrawlCompletedArgs e);

		protected abstract bool ShouldCrawlLink(string link);

        /// <summary>
        /// Mark <param name="link"></param> as visited.
        /// </summary>
        /// <param name="link"></param>
		private void AddVisitedLink(string link)
		{
			while (true)
			{
				if (!this._visitedLinks.TryAdd(link, true))
				{
					Thread.Sleep(1000);
					continue;
				}

				break;
			}
		}

        /// <summary>
        /// Check if new chrome-driver can be created
        /// </summary>
        /// <returns></returns>
		private bool CanCreateDriver()
		{
			lock (this._drivers)
			{
				if (this._drivers.Count < this._crawlerConfiguration.MaxThread)
				{
					return true;
				}
			}

			return false;
		}

        /// <summary>
        /// Method to handle when page crawling has been completed.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
		private void crawlerProcessPageCrawlCompletedHandler(object sender, PageCrawlCompletedArgs e)
		{
			try
			{
				this.OnCrawlerProcessPageCrawlCompleted(sender, e);

				Debug.WriteLine("Complete crawl page {0}", e.Driver.Url);
			}
			catch (Exception exception)
			{
				this._logger.Log(exception);
				this._errorCount++;

				if (this._crawlerConfiguration.ForceStopOnErrorThresholdReached && this._errorCount >= this._crawlerConfiguration.ErrorThreshold)
				{
					this.ForceStopCrawling();
				}
			}
		}

        /// <summary>
        /// Method to handle when page crawling was disallowed 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
		private void crawlerProcessPageCrawlDisallowedHandler(object sender, PageCrawlDisallowedArgs args)
		{
			Debug.WriteLine("Did not crawl page {0}", args);
			this.PageCrawlDisallowed?.Invoke(this, args);
		}

        /// <summary>
        /// Async method to load url using chrome-driver, scrap
        /// </summary>
        /// <param name="pageLink"></param>
        /// <returns></returns>
		private async Task CrawlPageAsync(string pageLink)
		{
			if (this._stopCrawling || !this.ShouldCrawlLink(pageLink))
			{
				this.crawlerProcessPageCrawlDisallowedHandler(this, new PageCrawlDisallowedArgs(pageLink));
				return;
			}

			var driver = this.GetIdleDriver();
			driver.Navigate().GoToUrl(pageLink);
			this.ScrapePageLinks(driver);

			this.PageCrawledCompleted?.Invoke(this, new PageCrawlCompletedArgs(this, driver, pageLink));
		}

        /// <summary>
        /// Get an idle chrome-driver
        /// </summary>
        /// <returns></returns>
		private StrictChromeDriver GetIdleDriver()
		{
			if (this.CanCreateDriver() && this.TryCreateDriver(out var driver))
			{
				return driver;
			}

			if (!this.IsThereAnIdleDriver())
			{
			    Thread.Sleep(MillisecondsTimeoutToWaitUntilRetryGetIdleChromeDriver);
				return this.GetIdleDriver();
			}

			StrictChromeDriver availableDriver;
			lock (this._drivers)
			{
				availableDriver = this._drivers.FirstOrDefault(t => !t.IsBusy);
			}

			if (availableDriver == null)
			{
				Thread.Sleep(MillisecondsTimeoutToWaitUntilRetryGetIdleChromeDriver);
				availableDriver = this.GetIdleDriver();
			}

			availableDriver.Lock();
			return availableDriver;
		}

        /// <summary>
        /// Check if there is an idle chrome-driver
        /// </summary>
        /// <returns></returns>
		private bool IsThereAnIdleDriver()
		{
			lock (_drivers)
			{
				return _drivers.Any(d => !d.IsBusy);
			}
		}

        /// <summary>
        /// Scrape page links if they are not visited and if they <see cref="ShouldCrawlLink"/>
        /// </summary>
        /// <param name="driver"></param>
		private void ScrapePageLinks(ChromeDriver driver)
		{
			var hrefs = driver.FindElementsByTagName("a").Select(t => t.GetAttribute("href"));
			foreach (var href in hrefs)
			{
				if (href == null)
				{
					continue;
				}

				if (!_visitedLinks.ContainsKey(href) && ShouldCrawlLink(href))
				{
					AddVisitedLink(href);
					this._pagesQueue.Enqueue(href);
				}
			}
		}

        /// <summary>
        /// Try create new chrome-driver instance.
        /// </summary>
        /// <param name="driver"></param>
        /// <returns></returns>
		private bool TryCreateDriver(out StrictChromeDriver driver)
		{
			lock (_drivers)
			{
				if (_drivers.Count < _crawlerConfiguration.MaxThread)
				{
					driver = new StrictChromeDriver(_crawlerConfiguration.ChromeDriverConfiguration);
					driver.Lock();
					this._drivers.Add(driver);
					return true;
				}
			}

			driver = null;
			return false;
		}
	}
}