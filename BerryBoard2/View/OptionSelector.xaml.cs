using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace BerryBoard2.View
{
	public partial class OptionSelector : Window
	{
		private MainWindow main;
		private string[]? options;

		public string SelectedDevice { get; set; }

		public OptionSelector(MainWindow main, string[]? options)
		{
			InitializeComponent();

			this.main = main;
			this.options = options;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			PopulateAudioDevices();
		}

		private void PopulateAudioDevices()
		{
			foreach (string? option in options)
			{
				ToggleButton toggleButton = new ToggleButton
				{
					Content = option,
					Margin = new Thickness(5)
				};
				toggleButton.Checked += ToggleButton_Checked;

				// Accessing and applying the Style
				if (FindResource("ExtraToggleButtonStyle") is Style toggleButtonStyle)
				{
					toggleButton.Style = toggleButtonStyle;
				}

				DevicePanel.Children.Add(toggleButton);
			}
		}

		private void ToggleButton_Checked(object sender, RoutedEventArgs e)
		{
			if (sender is ToggleButton checkedButton)
			{
				foreach (var child in DevicePanel.Children)
				{
					if (child is ToggleButton toggleButton && toggleButton != checkedButton)
					{
						toggleButton.IsChecked = false;
					}
				}

				SelectedDevice = (string)checkedButton.Content;
			}
		}

		private void Menu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}

		private void OkButton_Click(object sender, RoutedEventArgs e)
		{
			main.UpdateParam(SelectedDevice);
			Close();
		}
	}
}
