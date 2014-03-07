using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace HardwareProxy.Proxies {
	public class TimeBroadcastProxy {
		private readonly static Lazy<TimeBroadcastProxy> _instance = new Lazy<TimeBroadcastProxy>(() => new TimeBroadcastProxy(GlobalHost.ConnectionManager.GetHubContext<Hubs.TimeBroadcastHub>().Clients));
		private IHubConnectionContext Clients { get; set; }

		private readonly TimeSpan _UpdateInterval = TimeSpan.FromSeconds(1);
		private readonly Timer _BroadcastTimeTimer;

		private TimeBroadcastProxy(IHubConnectionContext clients) {
			this.Clients = clients;
			_BroadcastTimeTimer = new Timer(IssueTimeUpdate, null, _UpdateInterval, _UpdateInterval);
		}
		private void IssueTimeUpdate(object state) {
			Clients.All.UpdateTime(DateTimeOffset.UtcNow.ToString());
		}
		public static TimeBroadcastProxy Instance {
			get {
				return _instance.Value;
			}
		}
	}
}