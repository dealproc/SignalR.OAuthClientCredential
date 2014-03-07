using HardwareProxy.Proxies;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;

namespace HardwareProxy.Hubs {
	[HubName("timebroadcast"), Scope("HQ")]
	public class TimeBroadcastHub : Hub {
		private readonly TimeBroadcastProxy _TimeBroadcaster;
		public TimeBroadcastHub() : this(TimeBroadcastProxy.Instance) {

		}
		public TimeBroadcastHub(TimeBroadcastProxy proxy) {
			_TimeBroadcaster = proxy;
		}
	}
}