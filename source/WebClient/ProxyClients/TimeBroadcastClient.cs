namespace WebClient.ProxyClients {
	using System;
	using System.Collections.Generic;
	using System.ComponentModel;
	using System.Configuration;
	using System.Diagnostics;
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using Microsoft.AspNet.SignalR;
	using Microsoft.AspNet.SignalR.Client;
	using Microsoft.AspNet.SignalR.Hubs;
	using Thinktecture.IdentityModel.Client;
	public class TimeBroadcastClient : INotifyPropertyChanged {
		private readonly static Lazy<TimeBroadcastClient> _instance = new Lazy<TimeBroadcastClient>(() => new TimeBroadcastClient(GlobalHost.ConnectionManager.GetHubContext<Hubs.WebHub>().Clients));
		public static TimeBroadcastClient Instance {
			get { return _instance.Value; }
		}

		string _AccessToken;

		string _HardwareProxyUrl = "http://hardwareproxy.local/";
		TimeSpan _DelayBeforeHealing = TimeSpan.FromSeconds(5);

		Timer _HealingTimer;
		Timer _TokenAuthenticatedTimer;
		HubConnection _HardwareProxyConnection;
		IHubProxy _TimeBroadcastProxy;
		string _Status;
		string _ConnectionStatus;



		IHubConnectionContext Clients { get; set; }


		public string Status {
			get { return _Status; }
			set {
				_Status = value;
				Clients.All.UpdateStatus(value);
				FirePropertyChanged("Status");
			}
		}
		public string ConnectionStatus {
			get { return _ConnectionStatus; }
			set {
				_ConnectionStatus = value;
				Clients.All.UpdateConnectionStatus(value);
				FirePropertyChanged("ConnectionStatus");
			}
		}

		private TimeBroadcastClient(IHubConnectionContext clients) {
			Clients = clients;
			EstablishConnection();
		}

		private void EstablishConnection() {

			var oauthClient = new OAuth2Client(new Uri(ConfigurationManager.AppSettings["token:authsvr"]), ConfigurationManager.AppSettings["connect:clientid"], ConfigurationManager.AppSettings["connect:secret"]);
			oauthClient.RequestClientCredentialsAsync("HQ")
				.ContinueWith(x => {

					var token = x.Result;

					if (token.IsHttpError) {
						this.ConnectionStatus = "Having trouble establishing authorization to Hardware Proxy... trying again in 5 seconds.";
						_HealingTimer = new Timer(HealConnection, null, _DelayBeforeHealing, TimeSpan.FromMilliseconds(-1));
						return;
					}

					_AccessToken = token.AccessToken;

					if (_TokenAuthenticatedTimer != null) {
						_TokenAuthenticatedTimer.Dispose();
						_TokenAuthenticatedTimer = null;
					}

					if (_TimeBroadcastProxy != null) {
						_TimeBroadcastProxy = null;
					}

					if (_HardwareProxyConnection != null) {
						_HardwareProxyConnection.Dispose();
						_HardwareProxyConnection = null;
					}

					var queryStringData = new Dictionary<string, string>();
					queryStringData.Add("access_token", token.AccessToken);
					_HardwareProxyConnection = new HubConnection(_HardwareProxyUrl, queryStringData);
					_HardwareProxyConnection.StateChanged += (stateChanged) => {
						ConnectionStatus = stateChanged.NewState.ToString();
					};
					_HardwareProxyConnection.ConnectionSlow += () => {
						ConnectionStatus = "Connection to proxy is slow...";
					};


					_TimeBroadcastProxy = _HardwareProxyConnection.CreateHubProxy("timebroadcast");
					_TimeBroadcastProxy.On("UpdateTime", (string time) => { Clients.All.UpdateTimeDisplay(time); });
					_TimeBroadcastProxy.On("NotifyButtonClicked", () => { Clients.All.ButtonWasClicked(); });

					Trace.WriteLine("Access Token: " + token.AccessToken);

					_HardwareProxyConnection.Start()
						.ContinueWith(connectTask => {
							switch (connectTask.Status) {
								case TaskStatus.Running:
									this.ConnectionStatus = "Connected to proxy.";
									break;
								case TaskStatus.Canceled:
									_AccessToken = string.Empty;
									this.ConnectionStatus = "Connection to proxy has been cancelled.";
									break;
								case TaskStatus.Faulted:
									_AccessToken = string.Empty;
									this.ConnectionStatus = "Connection to proxy has been lost.  Trying to heal in 5 seconds.";
									_HealingTimer = new Timer(HealConnection, null, _DelayBeforeHealing, TimeSpan.FromMilliseconds(-1));
									break;
							}
						});

					_TokenAuthenticatedTimer = new Timer(CheckAuthenticated, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));
				});
		}
		~TimeBroadcastClient() {
			if (_TimeBroadcastProxy != null) {
				_TimeBroadcastProxy = null;
			}
			if (_HardwareProxyConnection.State != ConnectionState.Disconnected) {
				_HardwareProxyConnection.Stop();
			}
			_HardwareProxyConnection.Dispose();
			_HardwareProxyConnection = null;
		}

		public void HealConnection(object state) {
			if (_HardwareProxyConnection.State != ConnectionState.Connected) {
				this.ConnectionStatus = "Attempting to heal connection.";
				EstablishConnection();
			}
		}
		public void CheckAuthenticated(object state) {
			Trace.WriteLine("Checking authenticated status.");
			if (string.IsNullOrWhiteSpace(_AccessToken)) {
				return;
			}

			HttpClient client = new HttpClient {
				 BaseAddress = new Uri(_HardwareProxyUrl)
			};
			client.SetBearerToken(_AccessToken);
			client.GetAsync("api/TestAuthentication")
				.ContinueWith(x => {
					if (x.Result.IsSuccessStatusCode) {
						Trace.WriteLine("Seems to be a successful connection.");
						Status = x.Result.Content.ReadAsStringAsync().Result;
					} else {
						Trace.WriteLine("Connection failed.");
						Status = x.Result.ReasonPhrase;
					}
				});
		}

		public event PropertyChangedEventHandler PropertyChanged;
		private void FirePropertyChanged(string p) {
			var h = PropertyChanged;
			if (h != null) {
				h(this, new PropertyChangedEventArgs(p));
			}
		}
	}
}