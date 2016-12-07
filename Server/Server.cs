using System;
using System.Collections.Concurrent;

using ServiceStack;
using ServiceStack.Auth;

class Program
{
	[Route("/hello/{Name}")]
	public class Hello
	{
		public string Name { get; set; }
	}

	public class HelloResponse
	{
		public string Result { get; set; }
	}

	[Authenticate]
	public class HelloService : Service
	{
		public object Any(Hello request)
		{
			return new HelloResponse { Result = "Hello, " + request.Name };
		}
	}

	public class HelloAuthProvider : AuthProvider
	{
		private readonly ConcurrentDictionary<string, bool> _sessions = new ConcurrentDictionary<string, bool>();

		public HelloAuthProvider()
		{
			Provider = "apphost";
			AuthRealm = "/auth/" + Provider;
		}

		public override object Authenticate(IServiceBase authService, IAuthSession session, Authenticate request)
		{
			Console.WriteLine($"Authenticate called: session.IsAuthenticated = {session.IsAuthenticated}");

			session.IsAuthenticated = true;

			_sessions[session.Id] = true;

			return new AuthenticateResponse
				{
					SessionId = session.Id
				};
		}

		public override bool IsAuthorized(IAuthSession session, IAuthTokens tokens, Authenticate request = null)
		{
			return session?.Id != null && _sessions.ContainsKey(session.Id);
		}
	}

	//Define the Web Services AppHost
	public class AppHost : AppSelfHostBase
	{
		public AppHost()
		  : base("HttpListener Self-Host", typeof(HelloService).Assembly) { }

		public override void Configure(Funq.Container container)
		{
			Plugins.Add(new AuthFeature(
				() => new AuthUserSession(),
				new IAuthProvider[]
					{
						new HelloAuthProvider()
					}));
		}
	}

	//Run it!
	static void Main(string[] args)
	{
		var listeningOn = args.Length == 0 ? "http://*:1337/" : args[0];
		var appHost = new AppHost()
			.Init()
			.Start(listeningOn);

		Console.WriteLine("AppHost Created at {0}, listening on {1}",
			DateTime.Now, listeningOn);

		Console.ReadKey();
	}
}
