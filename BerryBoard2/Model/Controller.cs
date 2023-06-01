using BerryBoard2.Model.Libs;
using BerryBoard2.Model.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace BerryBoard2.Model
{
	internal class Controller
	{
		private Window window;
		private IntPtr handle;

		// DLL Imports
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		// App Command Codes
		internal const int WM_APPCOMMAND = 0x319;
		internal const int APPCOMMAND_VOLUME_MUTE = 0x80000;
		internal const int APPCOMMAND_VOLUME_UP = 0xA0000;
		internal const int APPCOMMAND_VOLUME_DOWN = 0x90000;
		internal const int APPCOMMAND_MEDIA_PLAY_PAUSE = 0xE0000;
		internal const int APPCOMMAND_MEDIA_NEXTTRACK = 0xB0000;
		internal const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 0xC0000;
		internal const int APPCOMMAND_BROWSER_BACKWARD = 0x100000;
		internal const int APPCOMMAND_BROWSER_FORWARD = 0x200000;
		internal const int APPCOMMAND_MIC_ON_OFF_TOGGLE = 0x180000;

		// Fields
		private Serial serial = new Serial();
		private WebSocket ws = new WebSocket();
		private const string configFile = "config.json";
		private const string settingsFile = "settings.json";
		private ButtonAction[]? buttons;
		private SettingsData? settings;
		private Dictionary<Action, BitmapImage>? images;

		public Controller(Window window, Grid buttonGrid)
		{
			this.window = window;
			handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;

			buttons = new ButtonAction[12];

			// Load images
			images = new Dictionary<Action, BitmapImage>()
			{
				{ Action.ChangeScene, new BitmapImage(new Uri("/Images/changescene.png", UriKind.Relative))},
				{ Action.StartStreaming, new BitmapImage(new Uri("/Images/startstreaming.png", UriKind.Relative))},
				{ Action.StopStreaming, new BitmapImage(new Uri("/Images/stopstreaming.png", UriKind.Relative))},
				{ Action.StartRecording, new BitmapImage(new Uri("/Images/startrecording.png", UriKind.Relative))},
				{ Action.StopRecording, new BitmapImage(new Uri("/Images/stoprecording.png", UriKind.Relative))},
				{ Action.PauseRecording, new BitmapImage(new Uri("/Images/pauserecording.png", UriKind.Relative))},

				{ Action.VolumeUp, new BitmapImage(new Uri("/Images/volumeup.png", UriKind.Relative))},
				{ Action.VolumeDown, new BitmapImage(new Uri("/Images/volumedown.png", UriKind.Relative))},
				{ Action.MuteAudio, new BitmapImage(new Uri("/Images/muteaudio.png", UriKind.Relative))},
				{ Action.PlayPause, new BitmapImage(new Uri("/Images/pauseplay.png", UriKind.Relative))},
				{ Action.NextTrack, new BitmapImage(new Uri("/Images/next.png", UriKind.Relative))},
				{ Action.PreviousTrack, new BitmapImage(new Uri("/Images/previous.png", UriKind.Relative))},

				{ Action.StartProcess, new BitmapImage(new Uri("/Images/launchprogram.png", UriKind.Relative))},
				{ Action.MuteMicrophone, new BitmapImage(new Uri("/Images/mutemicrophone.png", UriKind.Relative))}
			};

			// Load Config
			if (!File.Exists(configFile))
			{
				for (int i = 0; i < buttons.Length; i++)
				{
					buttons[i] = new ButtonAction();
				}
			}
			else
			{
				buttons = JsonConvert.DeserializeObject<ButtonAction[]>(File.ReadAllText(configFile));

				for (int i = 0; i < buttonGrid.Children.Count; i++)
				{
					UIElement element = buttonGrid.Children[i];
					if (element is Button button)
					{
						ButtonAction data = buttons[i];
						if (data?.action != Action.None)
						{
							button.Content = CreateImage(images[data.action]);
						}
					}
				}
			}

			// Load Settings
			if (!File.Exists(settingsFile))
			{
				settings = new SettingsData();
			}
			else
			{
				settings = JsonConvert.DeserializeObject<SettingsData>(File.ReadAllText(settingsFile));
			}

			// Setup Serial
			serial.SetupPorts();
			serial.DataReceivedEvent += DataReceived;

			// Setup WebSocket
			if (settings?.ObsEnable ?? false)
			{
				ws.WebsocketErrorEvent += OnWebsocketError;
				Task.Run(() => ObsChecker());
			}
		}

		private void OnWebsocketError(Exception x)
		{
			Task.Run(() => ObsChecker());
		}

		private void DataReceived(string msg)
		{
			if (int.TryParse(msg, out int num))
			{
				ButtonAction data = buttons[num];

				SafeWrite(window, () =>
				{
					switch (data.action)
					{
						// OBS
						case Action.ChangeScene:
							ws.SendWebSocketMessage(ObsReqGen.ChangeScene(data.param));
							break;
						case Action.StartStreaming:
							// Not implemented
							break;
						case Action.StopStreaming:
							// Not implemented
							break;
						case Action.StartRecording:
							ws.SendWebSocketMessage(ObsReqGen.StartRecording());
							break;
						case Action.StopRecording:
							ws.SendWebSocketMessage(ObsReqGen.StopRecording());
							break;
						case Action.PauseRecording:
							ws.SendWebSocketMessage(ObsReqGen.PauseRecording());
							break;

						// Media
						case Action.VolumeUp:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_UP);
							break;
						case Action.VolumeDown:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
							break;
						case Action.MuteAudio:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
							break;
						case Action.PlayPause:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
							break;
						case Action.NextTrack:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MEDIA_NEXTTRACK);
							break;
						case Action.PreviousTrack:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MEDIA_PREVIOUSTRACK);
							break;

						// System
						case Action.MuteMicrophone:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MIC_ON_OFF_TOGGLE);
							break;
						case Action.StartProcess:
							string exe = data.param;
							ProcessStartInfo processInfo = new ProcessStartInfo();
							processInfo.WorkingDirectory = Path.GetDirectoryName(data.param);
							processInfo.FileName = Path.GetFileName(exe);
							processInfo.UseShellExecute = true;
							Process.Start(processInfo);
							break;
					}
				});
			}
		}

		private async Task ObsChecker()
		{
			while (settings.ObsEnable ?? false)
			{
				Debug.WriteLine("checking obs");
				if (Process.GetProcesses().Any(p => p.ProcessName.Equals("obs64")))
				{
					Debug.WriteLine("found");
					if (!ws.IsWebSocketOpen())
					{
						await Task.Run(() => ws.SetupWebSocket($"ws://localhost:{settings.ObsPort}"));
					}
				}
				else
				{
					await ws.CloseWebSocket();
				}
				await Task.Delay(5000); // Wait 5 seconds before trying again
			}
		}

		public void ChangeButtonAction(int button, Action action = Action.None, string? param = null)
		{
			ButtonAction data = buttons[button];
			if (action != Action.None) data.action = action;
			if (param != null) data.param = param;
		}

		public ButtonAction GetButtonAction(int button)
		{
			if (button < buttons?.Length)
			{
				return buttons[button];
			}
			return null;
		}

		public SettingsData GetSettings() => settings;

		public void ClearButton(int button)
		{
			ButtonAction data = buttons[button];
			data.action = Action.None;
			data.param = string.Empty;
		}

		public void SaveConfig()
		{
			File.WriteAllText(configFile, JsonConvert.SerializeObject(buttons, Formatting.Indented));
		}

		public void SaveSettings(SettingsData settings)
		{
			File.WriteAllText(settingsFile, JsonConvert.SerializeObject(settings, Formatting.Indented));
			this.settings = settings;

			if (settings.ObsEnable ?? false)
			{
				Task.Run(() => ObsChecker());
			}
			else if (ws.IsWebSocketOpen())
			{
				Task.Run(() => ws.CloseWebSocket());
			}
		}

		public BitmapImage GetImage(Action action)
		{
			if (action != Action.None) return images[action];
			return null;
		}

		public Image CreateImage(BitmapImage source)
		{
			Image image = new Image();
			image.Source = source;
			image.Width = 20;
			image.Height = 20;

			return image;
		}

		private void SafeWrite(Window target, System.Action action)
		{
			try
			{
				System.Action safeWrite = delegate { action(); };
				if (target != null) target.Dispatcher.Invoke(safeWrite);
			}
			catch
			{
				// Ignore
			}
		}
	}
}
