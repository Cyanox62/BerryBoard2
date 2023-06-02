﻿using System.Windows;
using System.Windows.Input;

namespace BerryBoard2.View
{
	public partial class CustomMessageBox : Window
	{
		public CustomMessageBox(string message)
		{
			InitializeComponent();

			MessageTextBlock.Text = message;
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
			Close();
		}
	}

}
