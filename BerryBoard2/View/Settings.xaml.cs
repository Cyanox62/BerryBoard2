using BerryBoard2.Model.Objects;
using System.Windows;
using System.Windows.Input;

namespace BerryBoard2.View
{
	public partial class Settings : Window
	{
		private MainWindow main;

		public Settings(MainWindow main)
		{
			InitializeComponent();

			this.main = main;
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (ObsCheckbox.IsChecked ?? false && PortTextbox.Text == string.Empty)
			{
				var c = new CustomMessageBox("You must enter a port");
				c.Owner = this;
				c.Show();
				return;
			}

			if (!int.TryParse(PortTextbox.Text.Trim(), out int port))
			{
				var c = new CustomMessageBox("You must enter a port");
				c.Owner = this;
				c.Show();
				return;
			}

			main.UpdateSettings(new SettingsData()
			{
				ObsEnable = ObsCheckbox.IsChecked,
				ObsPort = port,
				ObsAuth = AuthText.Text.Trim()
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
	}
}
