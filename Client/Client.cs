using System;
using System.Net;
using System.Net.Http;

namespace Client
{
	class Client
	{
		static void Main()
		{
			var baseUrl = "http://localhost:1337/";
			var authUrl = baseUrl + "auth/apphost?format=json";
			var testUrl = baseUrl + "hello/John";

			var handler = new HttpClientHandler
			{
				CookieContainer = new CookieContainer(),
				UseDefaultCredentials = true
			};
			var client = new HttpClient(handler);

			for (int i = 0; i < 5; i++)
			{
				Console.WriteLine("Round {0}", i);

				try
				{
					var authResult = client.PostAsync(authUrl, new StringContent("{}"))
						.Result;

					var data = client.GetAsync(testUrl).Result;

					Console.WriteLine(data.IsSuccessStatusCode ? "Success!" : $"Failed with: {data.StatusCode}");
				}
				catch (Exception ex)
				{
					Console.WriteLine("Failed with {0}", ex);
				}
			}
		}
	}
}
