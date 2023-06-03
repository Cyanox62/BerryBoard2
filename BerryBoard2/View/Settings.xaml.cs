using BerryBoard2.Model;
using BerryBoard2.Model.Objects;
using System.Windows;
using System.Windows.Input;

namespace BerryBoard2.View
{
	public partial class Settings : Window
	{
		private MainWindow main;
		private Controller controller;

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

			if (!int.TryParse(PortTextbox.Text.Trim(), out int port))
			{
				new CustomMessageBox("Port must be a number") { Owner = this }.Show();
				return;
			}

			main.UpdateSettings(new SettingsData()
			{
				ObsEnable = ObsCheckbox.IsChecked,
				ObsPort = port,
				ObsAuth = AuthTextbox.Text.Trim(),
				MinimizeToTray = SystemTrayCheckbox.IsChecked
			});
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

			SystemTrayCheckbox.IsChecked = settings.MinimizeToTray;
		}
    }
}
