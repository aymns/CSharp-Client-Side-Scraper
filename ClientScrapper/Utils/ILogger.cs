namespace ClientScrapper.Utils
{
	using System;

	public interface ILogger
	{
		void Log(string message);
		void Log(Exception exception);
	}
}