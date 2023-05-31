using BerryBoard2.Model;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace BerryBoard2
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Controller? controller;

		private const string paramText = "Parameter";

		public MainWindow()
		{
			InitializeComponent();

			ParamText.Text = paramText;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			controller = new Controller(this);
		}

		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Minimized;
		}

		private void MaximizeRestoreButton_Click(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
		}

		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			Application.Current.Shutdown();
		}

		private void Menu_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			DragMove();
		}

		private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.LeftButton == MouseButtonState.Pressed)
			{
				TreeViewItem item = (TreeViewItem)sender;
				DragDrop.DoDragDrop(item, item, DragDropEffects.Move);
			}
		}

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			Button button = (Button)sender;
			SelectButton(button);
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			controller?.Save();
		}

		private Button? selectedButton = null;
		private void SelectButton(Button button)
		{
			if (selectedButton != null)
			{
				selectedButton.Background = (SolidColorBrush)FindResource("ButtonBackground");
			}

			button.Background = Brushes.DarkOrange;
			selectedButton = button;

			// Load data
			ButtonAction? data = controller?.GetButtonAction(int.Parse(button.Tag.ToString()));

			if (data?.action != Model.Action.None)
			{
				ClearButton.IsEnabled = true;
				foreach (TreeViewItem item in MainTreeView.Items)
				{
					string header = GetHeaderByAction(item, data?.action);
					if (header != null)
					{
						ActionLabel.Text = header;
						break;
					}
				}
			}
			else
			{
				ActionLabel.Text = data.action.ToString();
				ClearButton.IsEnabled = false;
			}

			ParamTextbox.Text = data?.param.ToString();

			switch (data?.action)
			{
				case Model.Action.ChangeScene:
					ParamText.Text = "Scene Name";
					ParamTextbox.IsEnabled = true;
					break;
				case Model.Action.StartProcess:
					ParamText.Text = "Filepath";
					ParamTextbox.IsEnabled = true;
					break;
				default:
					ParamText.Text = paramText;
					ParamTextbox.IsEnabled = false;
					break;
			}
		}

		private string GetHeaderByAction(TreeViewItem item, Model.Action? action)
		{
			if ((string)item.Tag == action.ToString())
				return (string)item.Header;

			string foundHeader = null;

			foreach (TreeViewItem child in item.Items)
			{
				foundHeader = GetHeaderByAction(child, action);
				if (foundHeader != null)
					break;
			}

			return foundHeader;
		}

		private void Button_Drop(object sender, DragEventArgs e)
		{
			TreeViewItem item = (TreeViewItem)e.Data.GetData(typeof(TreeViewItem));
			Button button = (Button)sender;

			Model.Action action = (Model.Action)Enum.Parse(typeof(Model.Action), item.Tag.ToString());
			controller?.ChangeButtonAction(int.Parse(button.Tag.ToString()), action, ParamTextbox.Text);

			SelectButton(button);
		}

		private void ParamTextbox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (selectedButton != null)
			{
				controller?.ChangeButtonAction(int.Parse(selectedButton.Tag.ToString()), Model.Action.None, ParamTextbox.Text);
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			if (selectedButton != null)
			{
				controller?.ClearButton(int.Parse(selectedButton.Tag.ToString()));
				ActionLabel.Text = "None";
				ParamText.Text = paramText;
				ParamTextbox.Text = string.Empty;
				ClearButton.IsEnabled = false;
				ParamTextbox.IsEnabled = false;
			}
		}
	}
}
