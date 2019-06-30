namespace ClientScrapper.Test
{
	using System;
	using Utils;

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