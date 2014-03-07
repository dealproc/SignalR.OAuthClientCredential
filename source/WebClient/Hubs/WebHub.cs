using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using WebClient.ProxyClients;

namespace WebClient.Hubs {
	[HubName("web")]
	public class WebHub : Hub {
		private readonly TimeBroadcastClient _TimeBroadcastClient;
		public WebHub() : this(TimeBroadcastClient.Instance) { }
		public WebHub(TimeBroadcastClient client) {
			_TimeBroadcastClient = client;
		}
	}
}