using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(WebClient.Startup))]

namespace WebClient {
	public class Startup {
		public void Configuration(IAppBuilder app) {
			app.MapSignalR();
		}
	}
}