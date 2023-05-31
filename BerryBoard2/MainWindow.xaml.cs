using BerryBoard2.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BerryBoard2
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private Controller? controller;

		public MainWindow()
		{
			InitializeComponent();
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
            else ClearButton.IsEnabled = true;

            button.Background = Brushes.DarkOrange;
            selectedButton = button;

            // Load data
            ButtonAction? data = controller?.GetButtonAction(int.Parse(button.Tag.ToString()));
            ActionLabel.Text = data.action.ToString();
            ParamTextbox.Text = data.param.ToString();
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
                Button button = (Button)sender;
                controller?.ClearButton(int.Parse(selectedButton.Tag.ToString()));
            }
		}
	}
}
