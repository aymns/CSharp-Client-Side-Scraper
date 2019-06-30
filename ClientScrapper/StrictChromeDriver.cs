namespace ClientScrapper
{
	using OpenQA.Selenium.Chrome;

    /// <summary>
    /// Represent a chrome-driver instance with a flag to represent if it's busy or not, <see cref="IsBusy"/>
    /// </summary>
	public class StrictChromeDriver : ChromeDriver
	{
		public StrictChromeDriver(ChromeOptions options) : base(options)
		{
			this.IsBusy = false;
		}

		public bool IsBusy { get; private set; }

		public void Lock()
		{
			this.IsBusy = true;
		}

		public void Release()
		{
			this.IsBusy = false;
		}
	}
}