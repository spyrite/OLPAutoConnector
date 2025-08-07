using MahApps.Metro.Controls;
using OLP.AutoConnector.ViewModels;
using System;

namespace OLP.AutoConnector.Views
{
    public partial class InputDataView : MetroWindow
    {
        public InputDataView(InputDataVM inputDataVM)
        {
            InitializeComponent();
            DataContext = inputDataVM;
        }

        private void OKButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            DialogResult = true;
            Properties.InputData.Default.RailingConnectionAlignX = double.Parse(TextBox1.Text) / 304.8;
            Properties.InputData.Default.Save();
            Close();
        }

        private void TextBox1_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            if (!(Char.IsDigit(e.Text, 0) || (e.Text == ".") && (!TextBox1.Text.Contains(".") && TextBox1.Text.Length != 0)))
                e.Handled = true;
        }
    }
}
