using BerryBoard2.Model;
using BerryBoard2.Model.Libs;
using BerryBoard2.Model.Objects;
using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace BerryBoard2.View
{
	public partial class Settings : Window
	{
		private MainWindow main;
		private Controller controller;

		private const string regKey = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run";

		internal Settings(MainWindow main, Controller controller)
		{
			InitializeComponent();

			this.main = main;
			this.controller = controller;
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if ((ObsCheckbox.IsChecked ?? false) && PortTextbox.Text == string.Empty)
			{
				new CustomMessageBox("You must enter a port") { Owner = this }.Show();
				return;
			}

			int port = -1;
			if (PortTextbox.Text != string.Empty)
			{
				if (int.TryParse(PortTextbox.Text.Trim(), out int p))
				{
					port = p;
				}
				else
				{
					new CustomMessageBox("Port must be a number") { Owner = this }.Show();
					return;
				}
			}

			main.UpdateSettings(new SettingsData()
			{
				ObsEnable = ObsCheckbox.IsChecked ?? false,
				ObsPort = port,
				ObsAuth = AuthTextbox.Text.Trim(),
				MinimizeToTray = SystemTrayCheckbox.IsChecked ?? false
			});

			SetStartup(StartupCheckBox.IsChecked ?? false);

			Close();
		}

		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void Menu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		private void ObsCheckbox_Checked(object sender, RoutedEventArgs e)
		{
			PortTextbox.IsEnabled = true;
			AuthTextbox.IsEnabled = true;
		}

		private void ObsCheckbox_Unchecked(object sender, RoutedEventArgs e)
		{
			PortTextbox.IsEnabled = false;
			AuthTextbox.IsEnabled = false;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			SettingsData settings = controller.GetSettings();
			ObsCheckbox.IsChecked = settings.ObsEnable;
			PortTextbox.Text = settings.ObsPort == -1 ? string.Empty : settings.ObsPort.ToString();
			AuthTextbox.Text = settings.ObsAuth;
			string portName = Serial.GetPortName();
			ConnectedText.Text = portName == null ? "Disconnected" : "Connected: " + portName;

			SystemTrayCheckbox.IsChecked = settings.MinimizeToTray;
			StartupCheckBox.IsChecked = DoesRegistryPairExist(regKey, Assembly.GetExecutingAssembly().GetName().Name);
		}

		private void SetStartup(bool startup)
		{
			try
			{
				using (RegistryKey key = Registry.CurrentUser.OpenSubKey(regKey, true))
				{
					Assembly curAssembly = Assembly.GetExecutingAssembly();
					if (startup) key.SetValue(curAssembly.GetName().Name, $"\"{Process.GetCurrentProcess().MainModule.FileName}\" -minimized");
					else key.DeleteValue(curAssembly.GetName().Name);
				}
			}
			catch (Exception ex)
			{
				Debug.WriteLine("Error removing from startup: " + ex.Message);
			}
		}

		public static bool DoesRegistryPairExist(string key, string value)
		{
			return Registry.CurrentUser.OpenSubKey(key, false).GetValueNames().FirstOrDefault(x => x == value) != null;
		}
	}
}
