﻿using AudioSwitcher.AudioApi.CoreAudio;
using BerryBoard2.Model.Libs;
using BerryBoard2.Model.Objects;
using CSCore;
using CSCore.Codecs;
using CSCore.SoundOut;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using File = System.IO.File;

namespace BerryBoard2.Model
{
	internal class Controller
	{
		private Window window;
		private IntPtr handle;

		// DLL Imports
		[DllImport("user32.dll")]
		public static extern IntPtr SendMessageW(IntPtr hWnd, int Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		static extern IntPtr SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32")]
		public static extern void LockWorkStation();

		#region Commands
		// App Codes
		internal const int WM_APPCOMMAND = 0x319;
		internal const int WM_SYSCOMMAND = 0x0112;

		// Media
		internal const int APPCOMMAND_VOLUME_MUTE = 0x80000;
		internal const int APPCOMMAND_VOLUME_UP = 0xA0000;
		internal const int APPCOMMAND_VOLUME_DOWN = 0x90000;
		internal const int APPCOMMAND_MEDIA_PLAY_PAUSE = 0xE0000;
		internal const int APPCOMMAND_MEDIA_NEXTTRACK = 0xB0000;
		internal const int APPCOMMAND_MEDIA_PREVIOUSTRACK = 0xC0000;

		// Browser
		internal const int APPCOMMAND_BROWSER_BACKWARD = 0x10000;
		internal const int APPCOMMAND_BROWSER_FORWARD = 0x20000;
		internal const int APPCOMMAND_BROWSER_REFRESH = 0x30000;
		internal const int APPCOMMAND_BROWSER_SEARCH = 0x50000;
		internal const int APPCOMMAND_BROWSER_HOME = 0x70000;

		// System
		internal const int APPCOMMAND_MIC_ON_OFF_TOGGLE = 0x180000;
		internal const int SC_MONITORPOWER = 0xF170;
		#endregion

		// Fields
		private Serial serial;
		private WebSocket ws;
		private CoreAudioController controller;
		private const string configFile = "config.json";
		private const string settingsFile = "settings.json";
		private ButtonAction[] buttons;
		private SettingsData settings;
		private Dictionary<KeyAction, BitmapImage>? images;

		// For shortcut creation
		string startupFolder = Environment.GetFolderPath(Environment.SpecialFolder.Startup);
		string appPath = System.Reflection.Assembly.GetExecutingAssembly().Location;
		string shortcutPath;

		public Controller(Window window, Grid buttonGrid)
		{
			this.window = window;
			handle = new System.Windows.Interop.WindowInteropHelper(window).Handle;

			buttons = new ButtonAction[12];

			serial = new Serial();
			ws = new WebSocket();
			controller = new CoreAudioController();

			shortcutPath = Path.Combine(startupFolder, window.Title + ".lnk");

			// Load images
			images = new Dictionary<KeyAction, BitmapImage>()
			{
				{ KeyAction.StartStreaming, new BitmapImage(new Uri("/Images/startstreaming.png", UriKind.Relative))},
				{ KeyAction.StopStreaming, new BitmapImage(new Uri("/Images/stopstreaming.png", UriKind.Relative))},
				{ KeyAction.StartRecording, new BitmapImage(new Uri("/Images/startrecording.png", UriKind.Relative))},
				{ KeyAction.StopRecording, new BitmapImage(new Uri("/Images/stoprecording.png", UriKind.Relative))},
				{ KeyAction.PauseRecording, new BitmapImage(new Uri("/Images/pauserecording.png", UriKind.Relative))},
				{ KeyAction.ChangeScene, new BitmapImage(new Uri("/Images/changescene.png", UriKind.Relative))},

				{ KeyAction.PlayPause, new BitmapImage(new Uri("/Images/pauseplay.png", UriKind.Relative))},
				{ KeyAction.NextTrack, new BitmapImage(new Uri("/Images/next.png", UriKind.Relative))},
				{ KeyAction.PreviousTrack, new BitmapImage(new Uri("/Images/previous.png", UriKind.Relative))},
				{ KeyAction.VolumeUp, new BitmapImage(new Uri("/Images/volumeup.png", UriKind.Relative))},
				{ KeyAction.VolumeDown, new BitmapImage(new Uri("/Images/volumedown.png", UriKind.Relative))},
				{ KeyAction.MuteAudio, new BitmapImage(new Uri("/Images/muteaudio.png", UriKind.Relative))},
				{ KeyAction.PlayAudio, new BitmapImage(new Uri("/Images/playaudio.png", UriKind.Relative))},

				{ KeyAction.Home, new BitmapImage(new Uri("/Images/home.png", UriKind.Relative))},
				{ KeyAction.Search, new BitmapImage(new Uri("/Images/search.png", UriKind.Relative))},
				{ KeyAction.Refresh, new BitmapImage(new Uri("/Images/refresh.png", UriKind.Relative))},
				{ KeyAction.Forward, new BitmapImage(new Uri("/Images/forward.png", UriKind.Relative))},
				{ KeyAction.Backward, new BitmapImage(new Uri("/Images/backward.png", UriKind.Relative))},

				{ KeyAction.Cut, new BitmapImage(new Uri("/Images/cut.png", UriKind.Relative))},
				{ KeyAction.Copy, new BitmapImage(new Uri("/Images/copy.png", UriKind.Relative))},
				{ KeyAction.Paste, new BitmapImage(new Uri("/Images/paste.png", UriKind.Relative))},
				{ KeyAction.CustomInput, new BitmapImage(new Uri("/Images/customtext.png", UriKind.Relative))},

				{ KeyAction.ChangeSpeakers, new BitmapImage(new Uri("/Images/changespeakers.png", UriKind.Relative))},
				{ KeyAction.ChangeMicrophone, new BitmapImage(new Uri("/Images/changemicrophone.png", UriKind.Relative))},
				{ KeyAction.MuteMicrophone, new BitmapImage(new Uri("/Images/mutemicrophone.png", UriKind.Relative))},

				{ KeyAction.StartProcess, new BitmapImage(new Uri("/Images/launchprogram.png", UriKind.Relative))},
				{ KeyAction.OpenWebsite, new BitmapImage(new Uri("/Images/openwebsite.png", UriKind.Relative))},
				{ KeyAction.CustomScript, new BitmapImage(new Uri("/Images/customscript.png", UriKind.Relative))},
				{ KeyAction.Lock, new BitmapImage(new Uri("/Images/lock.png", UriKind.Relative))},
				{ KeyAction.Sleep, new BitmapImage(new Uri("/Images/sleep.png", UriKind.Relative))},
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

		private IWaveSource audioSource;
		private ISoundOut soundOut;
		private Task audioTask;
		private CancellationTokenSource audioCancellationTokenSource;

		private void DataReceived(string msg)
		{
			if (int.TryParse(msg, out int num))
			{
				ButtonAction data = buttons[num];

				IntPtr curWindow = GetForegroundWindow();
				SafeWrite(window, () =>
				{
					switch (data.action)
					{
						#region OSB Studio
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
						case KeyAction.ChangeScene:
							ws.SendWebSocketMessage(ObsReqGen.ChangeScene(data.param));
							break;
						#endregion

						#region Media
						case KeyAction.PlayPause:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MEDIA_PLAY_PAUSE);
							break;
						case KeyAction.NextTrack:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MEDIA_NEXTTRACK);
							break;
						case KeyAction.PreviousTrack:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MEDIA_PREVIOUSTRACK);
							break;
						case KeyAction.VolumeUp:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_UP);
							break;
						case KeyAction.VolumeDown:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_DOWN);
							break;
						case KeyAction.MuteAudio:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_VOLUME_MUTE);
							break;
						case KeyAction.PlayAudio:
							if (audioTask != null && audioTask.Status == TaskStatus.Running)
							{
								audioCancellationTokenSource?.Cancel();
								soundOut?.Stop();
								audioSource?.Dispose();
								soundOut?.Dispose();
								audioTask = null;
							}
							else
							{
								audioCancellationTokenSource = new CancellationTokenSource();

								audioTask = Task.Run(() =>
								{
									try
									{
										audioSource = CodecFactory.Instance.GetCodec(data.param);
										soundOut = new CSCore.SoundOut.WasapiOut();
										soundOut.Initialize(audioSource);
										soundOut.Play();

										while (soundOut.PlaybackState == CSCore.SoundOut.PlaybackState.Playing)
										{
											Task.Delay(1000).Wait();
											audioCancellationTokenSource.Token.ThrowIfCancellationRequested();
										}
									}
									catch (OperationCanceledException)
									{
										soundOut?.Stop();
									}
									finally
									{
										audioSource?.Dispose();
										soundOut?.Dispose();
									}
								}, audioCancellationTokenSource.Token);
							}
							break;
						#endregion

						#region Browser
						case KeyAction.Home:
							SendMessageW(curWindow, WM_APPCOMMAND, curWindow, (IntPtr)APPCOMMAND_BROWSER_HOME);
							break;
						case KeyAction.Search:
							SendMessageW(curWindow, WM_APPCOMMAND, curWindow, (IntPtr)APPCOMMAND_BROWSER_SEARCH);
							break;
						case KeyAction.Refresh:
							SendMessageW(curWindow, WM_APPCOMMAND, curWindow, (IntPtr)APPCOMMAND_BROWSER_REFRESH);
							break;
						case KeyAction.Forward:
							SendMessageW(curWindow, WM_APPCOMMAND, curWindow, (IntPtr)APPCOMMAND_BROWSER_FORWARD);
							break;
						case KeyAction.Backward:
							SendMessageW(curWindow, WM_APPCOMMAND, curWindow, (IntPtr)APPCOMMAND_BROWSER_BACKWARD);
							break;
						#endregion

						#region Keyboard
						case KeyAction.Cut:
							SendKeys.Send("x", true);
							break;
						case KeyAction.Copy:
							SendKeys.Send("c", true);
							break;
						case KeyAction.Paste:
							SendKeys.Send("v", true);
							break;
						case KeyAction.CustomInput:
							bool shift = data.param.Contains("+");
							bool ctrl = data.param.Contains("^");
							bool alt = data.param.Contains("*");
							SendKeys.Send(data.param.Replace("+", "").Replace("^", "").Replace("*", ""), ctrl, shift, alt);
							break;
						#endregion

						#region Devices
						case KeyAction.ChangeSpeakers:
							controller.GetPlaybackDevices(AudioSwitcher.AudioApi.DeviceState.Active).FirstOrDefault(x => x.FullName == data.param)?.SetAsDefault();
							break;
						case KeyAction.ChangeMicrophone:
							controller.GetCaptureDevices(AudioSwitcher.AudioApi.DeviceState.Active).FirstOrDefault(x => x.FullName == data.param)?.SetAsDefault();
							break;
						case KeyAction.MuteMicrophone:
							SendMessageW(handle, WM_APPCOMMAND, handle, (IntPtr)APPCOMMAND_MIC_ON_OFF_TOGGLE);
							break;
						#endregion

						#region System
						case KeyAction.OpenWebsite:
							OpenUrl(data.param);
							break;
						case KeyAction.StartProcess:
							if (IsValidUrl(data.param))
							{
								OpenUrl(data.param);
							}
							else
							{
								Task.Run(() =>
								{
									string commandLine = data.param.Trim();
									string exe = "", arguments = "";

									int firstQuoteIndex = commandLine.IndexOf('\"');
									int secondQuoteIndex = commandLine.IndexOf('\"', firstQuoteIndex + 1);

									if (firstQuoteIndex != -1 && secondQuoteIndex != -1)
									{
										exe = commandLine.Substring(firstQuoteIndex, secondQuoteIndex - firstQuoteIndex + 1).Trim('\"');
										arguments = commandLine.Substring(secondQuoteIndex + 1).Trim();
									}
									else
									{
										exe = commandLine;
										arguments = "";
									}

									ProcessStartInfo processInfo = new ProcessStartInfo();
									processInfo.WorkingDirectory = Path.GetDirectoryName(exe);
									processInfo.FileName = Path.GetFileName(exe);
									processInfo.Arguments = arguments;
									processInfo.UseShellExecute = true;

									Task.Run(() => StartProcessWithFocus(processInfo));
								});
							}
							break;
						case KeyAction.CustomScript:
							Task.Run(() => RunScriptAsync(data.param));
							break;
						case KeyAction.Lock:
							LockWorkStation();
							break;
						case KeyAction.Sleep:
							SendMessageW(handle, WM_SYSCOMMAND, (IntPtr)SC_MONITORPOWER, (IntPtr)2);
							break;
						case KeyAction.PowerOff:
							var p = new ProcessStartInfo("shutdown", "/s /t 0");
							p.CreateNoWindow = true;
							p.UseShellExecute = false;
							Process.Start(p);
							break;
							#endregion
					}
				});
			}
		}

		private async Task RunScriptAsync(string scriptPath)
		{
			Debug.WriteLine(scriptPath);
			ProcessStartInfo startInfo = new ProcessStartInfo();

			switch (Path.GetExtension(scriptPath))
			{
				case ".py":
					startInfo.FileName = "python";
					break;
				case ".js":
					startInfo.FileName = "node";
					break;
				default:
					Console.WriteLine("Unsupported script type");
					return;
			}
			startInfo.Arguments = scriptPath;

			await Task.Run(() => StartProcessWithFocus(startInfo));
		}

		private void StartProcessWithFocus(ProcessStartInfo processInfo)
		{
			Process? newProcess = Process.Start(processInfo);
			newProcess?.WaitForInputIdle();
			SetForegroundWindow(newProcess.MainWindowHandle);
		}

		private bool IsValidUrl(string url)
		{
			if (string.IsNullOrEmpty(url))
				return false;

			Uri uriResult;
			return Uri.TryCreate(url, UriKind.Absolute, out uriResult)
				&& (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps || uriResult.Scheme == "steam");
		}


		private void OpenUrl(string url)
		{
			Process.Start(new ProcessStartInfo
			{
				FileName = url,
				UseShellExecute = true
			});
		}

		private bool isCheckingObs = false;
		private async Task ObsChecker()
		{
			isCheckingObs = true;
			while (settings?.ObsEnable ?? false)
			{
				if (Process.GetProcesses().Any(p => p.ProcessName.Equals("obs64")))
				{
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

		public string[] GetPlaybackDevices()
		{
			return controller.GetPlaybackDevices(AudioSwitcher.AudioApi.DeviceState.Active).Select(x => x.FullName).ToArray();
		}

		public string[] GetCaptureDevices()
		{
			return controller.GetCaptureDevices(AudioSwitcher.AudioApi.DeviceState.Active).Select(x => x.FullName).ToArray();
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

			if (settings.ObsEnable)
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
