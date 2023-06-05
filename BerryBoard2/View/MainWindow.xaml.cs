using BerryBoard2.Model;
using BerryBoard2.Model.Objects;
using BerryBoard2.View;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BerryBoard2
{
	public partial class MainWindow : Window
	{
		private Controller? controller;
		private const string paramText = "Parameter";

		BitmapImage folder = new BitmapImage(new Uri("/Images/folder.png", UriKind.Relative));
		BitmapImage options = new BitmapImage(new Uri("/Images/options.png", UriKind.Relative));

		public MainWindow()
		{
			InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			ParamText.Text = paramText;
			controller = new Controller(this, ButtonGrid);
		}

		private void MinimizeButton_Click(object sender, RoutedEventArgs e)
		{
			if (controller?.GetSettings().MinimizeToTray ?? false) Hide();
			else WindowState = WindowState.Minimized;
		}

		private void NotifyIcon_TrayMouseDoubleClick(object sender, RoutedEventArgs e)
		{
			WindowState = WindowState.Normal;
			Show();
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
			controller?.SaveConfig();

			var c = new CustomMessageBox("Configuration saved");
			c.Owner = this;
			c.Show();
		}

		private Button? selectedButton = null;
		private ButtonAction? selectedButtonData = null;
		private void SelectButton(Button button)
		{
			if (selectedButton != null)
			{
				selectedButton.Background = (SolidColorBrush)FindResource("ButtonBackground");
			}

			button.Background = Brushes.DarkOrange;
			selectedButton = button;

			// Load data
			selectedButtonData = controller?.GetButtonAction(int.Parse(button.Tag.ToString()));

			if (selectedButtonData?.action != KeyAction.None)
			{
				ClearButton.IsEnabled = true;
				foreach (TreeViewItem item in MainTreeView.Items)
				{
					string header = GetHeaderByAction(item, selectedButtonData?.action);
					if (header != null)
					{
						ActionLabel.Text = header;
						break;
					}
				}

				selectedButton.Content = controller?.CreateImage(controller?.GetImage(selectedButtonData.action));
				ActionImage.Source = controller?.CreateImage(controller?.GetImage(selectedButtonData.action)).Source;
				ActionImage.Visibility = Visibility.Visible;
			}
			else
			{
				ActionLabel.Text = selectedButtonData.action.ToString();
				ClearButton.IsEnabled = false;
				ActionImage.Visibility = Visibility.Collapsed;
			}

			ParamTextbox.Text = selectedButtonData?.param;

			switch (selectedButtonData?.action)
			{
				case KeyAction.ChangeScene:
					ParamText.Text = "Scene Name";
					ParamTextbox.IsEnabled = true;
					break;
				case KeyAction.StartProcess:
					ParamText.Text = "Application Path";
					ParamTextbox.IsEnabled = true;
					ParamButton.IsEnabled = true;
					((Image)ParamButton.Content).Source = folder;
					break;
				case KeyAction.CustomText:
					ParamText.Text = "Text";
					ParamTextbox.IsEnabled = true;
					break;
				case KeyAction.OpenWebsite:
					ParamText.Text = "Website URL";
					ParamTextbox.IsEnabled = true;
					((Image)ParamButton.Content).Source = folder;
					break;
				case KeyAction.PlayAudio:
					ParamText.Text = "Audio Path";
					ParamTextbox.IsEnabled = true;
					ParamButton.IsEnabled = true;
					((Image)ParamButton.Content).Source = folder;
					break;
				case KeyAction.ChangeSpeakers:
				case KeyAction.ChangeMicrophone:
					ParamText.Text = "Desired Device";
					ParamTextbox.IsEnabled = true;
					ParamButton.IsEnabled = true;
					((Image)ParamButton.Content).Source = options;
					break;
				default:
					ParamText.Text = paramText;
					ParamTextbox.IsEnabled = false;
					ParamButton.IsEnabled = false;
					break;
			}
		}

		private string GetHeaderByAction(TreeViewItem i, KeyAction? action)
		{
			foreach (TreeViewItem item in i.Items)
			{
				if (item.Tag.ToString() == action?.ToString())
				{
					if (item.Header is StackPanel panel)
					{
						foreach (var child in panel.Children)
						{
							if (child is TextBlock textBlock)
							{
								return textBlock.Text;
							}
						}
					}
				}
			}
			return null;
		}

		private void Button_Drop(object sender, DragEventArgs e)
		{
			TreeViewItem item = (TreeViewItem)e.Data.GetData(typeof(TreeViewItem));
			Button button = (Button)sender;

			KeyAction action = (KeyAction)Enum.Parse(typeof(KeyAction), item.Tag.ToString());
			controller?.ChangeButtonAction(int.Parse(button.Tag.ToString()), action);

			SelectButton(button);
		}

		private void ParamTextbox_TextChanged(object sender, TextChangedEventArgs e)
		{
			if (selectedButton != null)
			{
				controller?.ChangeButtonAction(int.Parse(selectedButton.Tag.ToString()), KeyAction.None, ParamTextbox.Text);
			}
		}

		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			if (selectedButton != null)
			{
				int button = int.Parse(selectedButton.Tag.ToString());
				controller?.ClearButton(button);
				ActionLabel.Text = "None";
				selectedButton.Content = null;
				ParamText.Text = paramText;
				ParamTextbox.Text = string.Empty;
				ClearButton.IsEnabled = false;
				ParamTextbox.IsEnabled = false;
				ActionImage.Visibility = Visibility.Collapsed;
			}
		}

		public void UpdateParam(string text)
		{
			ParamTextbox.Text = text;
		}

		private void ParamButton_Click(object sender, RoutedEventArgs e)
		{
			if (selectedButtonData?.action == KeyAction.ChangeSpeakers)
			{
				new OptionSelector(this, controller?.GetPlaybackDevices()) { Owner = this }.Show();
			}
			else if (selectedButtonData?.action == KeyAction.ChangeMicrophone)
			{
				new OptionSelector(this, controller?.GetCaptureDevices()) { Owner = this }.Show();
			}
			else
			{
				Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

				if (selectedButtonData?.action == KeyAction.StartProcess)
				{
					dlg.DefaultExt = ".exe";
					dlg.Filter = "Executable Files (*.exe;*.url)|*.exe;*.url";
					dlg.CheckFileExists = false;
				}
				else if (selectedButtonData?.action == KeyAction.PlayAudio)
				{
					dlg.DefaultExt = ".mp3";
					dlg.Filter = "Audio Files (*.mp3;*.wav)|*.mp3;*.wav";
				}

				dlg.InitialDirectory = @"C:\";

				Nullable<bool> result = dlg.ShowDialog();

				if (result == true)
				{
					if (Path.GetExtension(dlg.FileName).ToLower() == ".url")
					{
						try
						{
							using (StreamReader sr = new StreamReader(dlg.FileName))
							{
								string line;
								while ((line = sr.ReadLine()) != null)
								{
									if (line.StartsWith("URL="))
									{
										ParamTextbox.Text = line.Substring(4);
										return;
									}
								}
							}
						}
						catch (Exception ex)
						{
							Console.WriteLine($"Error reading .url file: {ex.Message}");
						}
					}
					else
					{
						ParamTextbox.Text = dlg.FileName;
					}
				}
			}
		}

		private void AboutButton_Click(object sender, RoutedEventArgs e)
		{
			new About().Show();
		}

		private void SettingsButton_Click(object sender, RoutedEventArgs e)
		{
			new Settings(this, controller).Show();
		}

		public void UpdateSettings(SettingsData settings)
		{
			controller?.SaveSettings(settings);
		}
	}
}
