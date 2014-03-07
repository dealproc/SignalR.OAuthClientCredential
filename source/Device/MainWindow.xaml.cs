using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.AspNet.SignalR.Client;
using Thinktecture.IdentityModel.Client;

namespace Device {
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window {
		string _HardwareProxyUrl = "http://hardwareproxy.local/";

		HubConnection _Connection;
		IHubProxy _HardwareHub;
		Timer _HealingTimer;

		public string Status {
			set {
				Dispatcher.BeginInvoke(new Action(() => this.lbStatus.Content = value));
			}
		}
		public string ConnectionStatus {
			set {
				Dispatcher.BeginInvoke(new Action(() => this.lbConnectionStatus.Content = value));
			}
		}

		public MainWindow() {
			InitializeComponent();

			this.btnToPress.Click += (s, e) => {
				if (_HardwareHub != null) {
					if (_Connection.State == ConnectionState.Connected) {
						_HardwareHub.Invoke("ButtonClicked");
					}
				}
			};

			this.Loaded += (s, e) => {
				_Connection = new HubConnection(_HardwareProxyUrl);
				_Connection.StateChanged += (stateChanged) => {
					Status = stateChanged.NewState.ToString();
				};
				_Connection.ConnectionSlow += () => {
					Status = "Connection to proxy is slow.";
				};

				_HardwareHub = _Connection.CreateHubProxy("hardware");

				EstablishConnection();
			};
		}


		private void EstablishConnection() {
			var oauthClient = new OAuth2Client(new Uri(ConfigurationManager.AppSettings["connect:url"]), ConfigurationManager.AppSettings["connect:clientid"], ConfigurationManager.AppSettings["connect:secret"]);

			oauthClient.RequestClientCredentialsAsync("datahub")
				.ContinueWith(tokenTask => {
					var token = tokenTask.Result;

					if (token.IsHttpError) {
						ConnectionStatus = "Having trouble establishing authorization to Hardware proxy... trying again in 5 seconds.";
						_HealingTimer = new Timer(HealConnection, null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(-1));
						return;
					}

					_Connection.Headers.Clear();
					_Connection.Headers.Add("Authorization", "bearer " + token.AccessToken);

					_Connection.Start()
						.ContinueWith(connectTask => {
							switch (connectTask.Status) {
								case TaskStatus.Running:
									break;
								case TaskStatus.Canceled:
									break;
								case TaskStatus.Faulted:
									ConnectionStatus = "Connection to proxy has been lost.  Trying to heal in 5 seconds.";
									_HealingTimer = new Timer(HealConnection, null, TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(-1));
									break;
							}
						});
				});
		}
		public void HealConnection(object state) {
			if (_Connection.State != ConnectionState.Connected) {
				ConnectionStatus = "Attempting to heal connection.";
				EstablishConnection();
			}
		}
	}
}
