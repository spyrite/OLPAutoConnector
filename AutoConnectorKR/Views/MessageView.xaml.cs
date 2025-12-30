using MahApps.Metro.Controls;
using System.Windows;

namespace OLP.AutoConnectorKR.Views
{
    public enum ButtonsVisibility
    {
        YesNo,
        Ok
    }

    public partial class MessageView : MetroWindow
    {
        public MessageView(string title, string message, double height, double width, ButtonsVisibility buttons)
        {
            InitializeComponent();
            Title = title;
            Height = height;
            Width = width;
            MessageBox.Text = message;

            switch (buttons)
            {
                case ButtonsVisibility.YesNo:
                    Button1.Visibility = Visibility.Visible;
                    Button2.Visibility = Visibility.Visible;
                    Button1.Content = "Да";
                    Button2.Content = "Нет";
                    break;
                case ButtonsVisibility.Ok:
                    Button1.Visibility = Visibility.Visible;
                    Button2.Visibility = Visibility.Collapsed;
                    Button1.Content = "OK";
                    break;
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }
    }
}
