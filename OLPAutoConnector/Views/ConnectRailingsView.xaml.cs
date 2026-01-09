using MahApps.Metro.Controls;
using OLP.AutoConnector.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace OLP.AutoConnector.Views
{
    public partial class ConnectRailingsView : MetroWindow
    {
        public ConnectRailingsView(ConnectRailingsVM inputDataVM)
        {
            InitializeComponent();
            DataContext = inputDataVM;          
            RadioButton1.Content = new Image { Source = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/ConnectRailingsView_RadioButton1.png")) };
            RadioButton2.Content = new Image { Source = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/ConnectRailingsView_RadioButton2.png")) };
            RadioButton3.Content = new Image { Source = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/ConnectRailingsView_RadioButton3.png")) };
            RadioButton4.Content = new Image { Source = new BitmapImage(new Uri(@"pack://application:,,,/AutoConnector;component/Resources/Images/ConnectRailingsView_RadioButton4.png")) };
            if (inputDataVM.AllowConnectionType1 == false) RadioButton1.Visibility = System.Windows.Visibility.Collapsed;
            if (inputDataVM.AllowConnectionType2 == false) RadioButton2.Visibility = System.Windows.Visibility.Collapsed;
            if (inputDataVM.AllowConnectionType3 == false) RadioButton3.Visibility = System.Windows.Visibility.Collapsed;
            if (inputDataVM.AllowConnectionType4 == false) RadioButton4.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            List<bool?> check1 = [RadioButton1.IsChecked, RadioButton2.IsChecked, RadioButton3.IsChecked, RadioButton4.IsChecked];
            if (check1.Any(c => c == true))
            {
                DialogResult = true;
                Properties.ConnectRailings.Default.Save();
                Close();
            }
            else
                new MessageView("Ошибка", "Не выбран тип соединения ограждений. Продолжение команды невозможно.", 150, 300, ButtonsVisibility.Ok).ShowDialog();

        }

        private void TextBox1_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".") && !TextBox1.Text.Contains(".") && TextBox1.Text.Length != 0))
                e.Handled = true;
        }

        private void TextBox23_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".") || (e.Text == "-") && !TextBox1.Text.Contains(".") && TextBox1.Text.Length != 0))
                e.Handled = true;
        }
    }
}
