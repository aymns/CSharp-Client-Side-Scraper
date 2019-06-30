namespace ClientScrapper.App
{
	using System;
	using ClientScrapper.Utils;

    /// <summary>
    /// Simple Logger implementation
    /// </summary>
	public class Logger : ILogger
	{
		public void Log(string message)
		{
			Console.WriteLine(message);
		}

		public void Log(Exception exception)
		{
			Console.WriteLine(exception.Message);
		}
	}
}