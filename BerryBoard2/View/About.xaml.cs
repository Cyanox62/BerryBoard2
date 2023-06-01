using System.Windows;
using System.Windows.Input;

namespace BerryBoard2.View
{
	public partial class About : Window
	{
		public About()
		{
			InitializeComponent();
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
	}
}
