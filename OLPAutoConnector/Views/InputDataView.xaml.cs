using Autodesk.Revit.UI;
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

            if (RadioButton1.IsChecked == true)
                Properties.InputData.Default.RailingsConnectionType = 0;
            else if (RadioButton2.IsChecked == true)
                Properties.InputData.Default.RailingsConnectionType = 1;
            else if (RadioButton3.IsChecked == true)
                Properties.InputData.Default.RailingsConnectionType = 2;
            else if (RadioButton4.IsChecked == true)
                Properties.InputData.Default.RailingsConnectionType = 3;

            Properties.InputData.Default.UpperRailingConnectionX = double.Parse(TextBox1.Text) / 304.8;
            Properties.InputData.Default.UpperRailingConnectionDZ = double.Parse(TextBox2.Text) / 304.8;
            Properties.InputData.Default.LowerRailingConnectionDZ = double.Parse(TextBox3.Text) / 304.8;
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
