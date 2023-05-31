using BerryBoard2.Model.Libs;
using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;

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
		private ButtonAction[]? buttons;

		public Controller(Window window)
		{
			this.window = window;
			handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;

			buttons = new ButtonAction[12];

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
			}

			// Setup Serial
			serial.SetupPorts();
			serial.DataReceivedEvent += DataReceived;

			// Setup WebSocket
			ws.SetupWebSocket("ws://localhost:4455");
		}

		private void DataReceived(string msg)
		{
			if (int.TryParse(msg, out int num))
			{
				num--;

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
							Process.Start(data.param);
							break;
					}
				});
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

		public void ClearButton(int button)
		{
			ButtonAction data = buttons[button];
			data.action = Action.None;
			data.param = string.Empty;
		}

		public void Save()
		{
			File.WriteAllText(configFile, JsonConvert.SerializeObject(buttons, Formatting.Indented));
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
