using System;
using System.Diagnostics;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace BerryBoard2.Model.Libs
{
	internal class WebSocket
	{
		public delegate void DataReceivedDelegate(string msg);
		public event DataReceivedDelegate? DataReceivedEvent;

		public delegate void WebsocketErrorDelegate(Exception Exception);
		public event WebsocketErrorDelegate? WebsocketErrorEvent;

		private ClientWebSocket? clientWebSocket;

		public void SetupWebSocket(string url, string auth)
		{
			Task.Run(() => Init(url, auth));
		}

		public bool IsWebSocketOpen()
		{
			return clientWebSocket?.State == WebSocketState.Open;
		}

		public async Task CloseWebSocket()
		{
			if (clientWebSocket?.State == WebSocketState.Open)
			{
				await clientWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
			}
			clientWebSocket = null;
		}

		private async Task Init(string u, string auth)
		{
			var url = new Uri(u);

			try
			{
				clientWebSocket = new ClientWebSocket();
				await clientWebSocket.ConnectAsync(url, CancellationToken.None);

				if (clientWebSocket.State == WebSocketState.Open)
				{
					// Send initial request
					await Send(ObsReqGen.Identify(auth));

					// Handle received messages
					while (clientWebSocket.State == WebSocketState.Open)
					{
						var receivedMessage = await ReceiveWebSocketMessage();
						Debug.WriteLine("Received message: " + receivedMessage);
						DataReceivedEvent?.Invoke(receivedMessage);
					}
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("WebSocket connection error: " + ex.Message);
				WebsocketErrorEvent?.Invoke(ex);
			}
			finally
			{
				if (clientWebSocket != null)
				{
					clientWebSocket.Dispose();
					clientWebSocket = null;
				}
			}
		}

		public void SendWebSocketMessage(string message)
		{
			Task.Run(() => Send(message));
		}

		private async Task Send(string message)
		{
			if (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
			{
				var messageBytes = Encoding.UTF8.GetBytes(message);
				var buffer = new ArraySegment<byte>(messageBytes);
				await clientWebSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

		private async Task<string> ReceiveWebSocketMessage()
		{
			if (clientWebSocket != null && clientWebSocket.State == WebSocketState.Open)
			{
				var buffer = new ArraySegment<byte>(new byte[1024]);
				var receivedResult = await clientWebSocket.ReceiveAsync(buffer, CancellationToken.None);
				var messageBytes = new byte[receivedResult.Count];
				Array.Copy(buffer.Array, messageBytes, receivedResult.Count);
				var message = Encoding.UTF8.GetString(messageBytes);
				return message;
			}

			return string.Empty;
		}
	}
}
