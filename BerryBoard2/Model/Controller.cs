﻿using BerryBoard2.Model.Libs;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using NAudio.Wave;
using System.Threading;

namespace BerryBoard2.Model
{
	internal class Controller
	{
		private Window window;
		private IntPtr handle;

		// DLL Imports
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		#region App Codes
		// App Codes
		internal const int WM_APPCOMMAND = 0x319;

		// Media
		internal const int APPCOMMAND_VOLUME_MUTE = 0x80000;
		internal const int APPCOMMAND_VOLUME_UP = 0xA0000;
		internal const int APPCOMMAND_VOLUME_DOWN = 0x90000;
		internal const int APPCOMMAND_MEDIA_PLAY_PAUSE = 0xE0000;
		internal const int APPCOMMAND_MEDIA_NEXTTRACK = 0xB0000;
		internal const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 0xC0000;

		// Keyboard
		internal const int APPCOMMAND_CUT = 0x108;
		internal const int APPCOMMAND_COPY = 0x109;
		internal const int APPCOMMAND_PASTE = 0x10A;
		internal const int APPCOMMAND_DELETE = 0x10B;
		internal const int APPCOMMAND_UNDO = 0x10C;
		internal const int APPCOMMAND_REDO = 0x10D;

		// System
		internal const int APPCOMMAND_MIC_ON_OFF_TOGGLE = 0x180000;
		#endregion

		// Fields
		private Serial serial = new Serial();
		private WebSocket ws = new WebSocket();
		private const string configFile = "config.json";
		private const string settingsFile = "settings.json";
		private ButtonAction[]? buttons;
		private SettingsData? settings;
		private Dictionary<KeyAction, BitmapImage>? images;

		public Controller(Window window, Grid buttonGrid)
		{
			this.window = window;
			handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;

			buttons = new ButtonAction[12];

			// Load images
			images = new Dictionary<KeyAction, BitmapImage>()
			{
				{ KeyAction.ChangeScene, new BitmapImage(new Uri("/Images/changescene.png", UriKind.Relative))},
				{ KeyAction.StartStreaming, new BitmapImage(new Uri("/Images/startstreaming.png", UriKind.Relative))},
				{ KeyAction.StopStreaming, new BitmapImage(new Uri("/Images/stopstreaming.png", UriKind.Relative))},
				{ KeyAction.StartRecording, new BitmapImage(new Uri("/Images/startrecording.png", UriKind.Relative))},
				{ KeyAction.StopRecording, new BitmapImage(new Uri("/Images/stoprecording.png", UriKind.Relative))},
				{ KeyAction.PauseRecording, new BitmapImage(new Uri("/Images/pauserecording.png", UriKind.Relative))},

				{ KeyAction.VolumeUp, new BitmapImage(new Uri("/Images/volumeup.png", UriKind.Relative))},
				{ KeyAction.VolumeDown, new BitmapImage(new Uri("/Images/volumedown.png", UriKind.Relative))},
				{ KeyAction.MuteAudio, new BitmapImage(new Uri("/Images/muteaudio.png", UriKind.Relative))},
				{ KeyAction.PlayPause, new BitmapImage(new Uri("/Images/pauseplay.png", UriKind.Relative))},
				{ KeyAction.NextTrack, new BitmapImage(new Uri("/Images/next.png", UriKind.Relative))},
				{ KeyAction.PreviousTrack, new BitmapImage(new Uri("/Images/previous.png", UriKind.Relative))},

				{ KeyAction.Cut, new BitmapImage(new Uri("/Images/cut.png", UriKind.Relative))},
				{ KeyAction.Copy, new BitmapImage(new Uri("/Images/copy.png", UriKind.Relative))},
				{ KeyAction.Paste, new BitmapImage(new Uri("/Images/paste.png", UriKind.Relative))},
				{ KeyAction.CustomText, new BitmapImage(new Uri("/Images/customtext.png", UriKind.Relative))},

				{ KeyAction.StartProcess, new BitmapImage(new Uri("/Images/launchprogram.png", UriKind.Relative))},
				{ KeyAction.PlayAudio, new BitmapImage(new Uri("/Images/playaudio.png", UriKind.Relative))},
				{ KeyAction.OpenWebsite, new BitmapImage(new Uri("/Images/openwebsite.png", UriKind.Relative))},
				{ KeyAction.MuteMicrophone, new BitmapImage(new Uri("/Images/mutemicrophone.png", UriKind.Relative))},
				{ KeyAction.PowerOff, new BitmapImage(new Uri("/Images/poweroff.png", UriKind.Relative))}
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
						if (data?.action != KeyAction.None)
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

		private static AudioFileReader audioFile;
		private static WaveOutEvent outputDevice;
		private static CancellationTokenSource audioCancellationTokenSource;
		private static Task audioTask;

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
						case KeyAction.ChangeScene:
							ws.SendWebSocketMessage(ObsReqGen.ChangeScene(data.param));
							break;
						case KeyAction.StartStreaming:
							ws.SendWebSocketMessage(ObsReqGen.StartStreaming());
							break;
						case KeyAction.StopStreaming:
							ws.SendWebSocketMessage(ObsReqGen.StopStreaming());
							break;
						case KeyAction.StartRecording:
							ws.SendWebSocketMessage(ObsReqGen.StartRecording());
							break;
						case KeyAction.StopRecording:
							ws.SendWebSocketMessage(ObsReqGen.StopRecording());
							break;
						case KeyAction.PauseRecording:
							ws.SendWebSocketMessage(ObsReqGen.PauseRecording());
							break;

						// Media
						case KeyAction.VolumeUp:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_UP);
							break;
						case KeyAction.VolumeDown:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
							break;
						case KeyAction.MuteAudio:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
							break;
						case KeyAction.PlayPause:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
							break;
						case KeyAction.NextTrack:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MEDIA_NEXTTRACK);
							break;
						case KeyAction.PreviousTrack:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MEDIA_PREVIOUSTRACK);
							break;

						// Keyboard
						case KeyAction.Cut:
							SendKeys.Send("x", true);
							break;
						case KeyAction.Copy:
							SendKeys.Send("c", true);
							break;
						case KeyAction.Paste:
							SendKeys.Send("v", true);
							break;
						case KeyAction.CustomText:
							SendKeys.Send(data.param);
							break;

						// System
						case KeyAction.PlayAudio:
							// If audio is currently playing, cancel the task and stop the playback
							if (audioTask != null && audioTask.Status == TaskStatus.Running)
							{
								audioCancellationTokenSource?.Cancel();
								outputDevice?.Stop();
								audioFile?.Dispose();
								outputDevice?.Dispose();
								audioTask = null;
							}
							// If no audio is playing, start playing
							else
							{
								// Start a new cancellation token source for the new audio task
								audioCancellationTokenSource = new CancellationTokenSource();

								audioTask = Task.Run(() =>
								{
									try
									{
										audioFile = new AudioFileReader(data.param);
										outputDevice = new WaveOutEvent();
										outputDevice.Init(audioFile);
										outputDevice.Play();

										// We use SpinWait to hold the Task open until the PlaybackStopped event is fired.
										while (outputDevice.PlaybackState == PlaybackState.Playing)
										{
											Task.Delay(1000).Wait();
											// Throw if cancellation is requested
											audioCancellationTokenSource.Token.ThrowIfCancellationRequested();
										}
									}
									catch (OperationCanceledException)
									{
										// Handle the cancellation
										outputDevice?.Stop();
									}
									finally
									{
										audioFile?.Dispose();
										outputDevice?.Dispose();
									}
								}, audioCancellationTokenSource.Token);
							}
							break;
						case KeyAction.OpenWebsite:
							Process.Start(new ProcessStartInfo
							{
								FileName = data.param,
								UseShellExecute = true
							});
							break;
						case KeyAction.StartProcess:
							string exe = data.param;
							ProcessStartInfo processInfo = new ProcessStartInfo();
							processInfo.WorkingDirectory = Path.GetDirectoryName(data.param);
							processInfo.FileName = Path.GetFileName(exe);
							processInfo.UseShellExecute = true;
							Process.Start(processInfo);
							break;
						case KeyAction.MuteMicrophone:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MIC_ON_OFF_TOGGLE);
							break;
						case KeyAction.PowerOff:
							var p = new ProcessStartInfo("shutdown", "/s /t 0");
							p.CreateNoWindow = true;
							p.UseShellExecute = false;
							Process.Start(p);
							break;
					}
				});
			}
		}

		private bool isCheckingObs = false;
		private async Task ObsChecker()
		{
			isCheckingObs = true;
			while (settings?.ObsEnable ?? false)
			{
				Debug.WriteLine("checking obs");
				if (Process.GetProcesses().Any(p => p.ProcessName.Equals("obs64")))
				{
					Debug.WriteLine("found");
					if (!ws.IsWebSocketOpen())
					{
						await Task.Run(() => ws.SetupWebSocket($"ws://localhost:{settings.ObsPort}", settings.ObsAuth));
					}
				}
				else
				{
					await ws.CloseWebSocket();
				}
				await Task.Delay(5000); // Wait 5 seconds before trying again
			}
			isCheckingObs = false;
		}

		public void ChangeButtonAction(int button, KeyAction action = KeyAction.None, string? param = null)
		{
			ButtonAction data = buttons[button];
			if (action != KeyAction.None) data.action = action;
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
			data.action = KeyAction.None;
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

			if ((settings.ObsEnable ?? false))
			{
				if (!isCheckingObs) Task.Run(() => ObsChecker());
			}
			else if (ws.IsWebSocketOpen())
			{
				Task.Run(() => ws.CloseWebSocket());
			}
		}

		public BitmapImage GetImage(KeyAction action)
		{
			if (action != KeyAction.None) return images[action];
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
