using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace HardwareProxy.Hubs {
	[HubName("hardware"), Scope("datahub")]
	public class HardwareHub : Hub {
		private IHubConnectionContext WebClients { get; set; }

		public HardwareHub() : this(GlobalHost.ConnectionManager.GetHubContext<TimeBroadcastHub>().Clients) { }

		public HardwareHub(IHubConnectionContext webClients) {
			this.WebClients = webClients;
		}

		public void ButtonClicked() {
			WebClients.All.NotifyButtonClicked(string.Format("Button clicked event fired at: {0}", DateTimeOffset.UtcNow));
		}
	}
}