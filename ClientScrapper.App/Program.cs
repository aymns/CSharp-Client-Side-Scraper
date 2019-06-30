using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientScrapper.App
{
    class Program
    {
        static void Main(string[] args)
        {
			var crawler = new SimpleCrawler("http://google.com", new Logger(), new ClientCrawlerConfiguration());
			crawler.Start();
		}
		
    }
}
