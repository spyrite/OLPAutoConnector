using Autodesk.Revit.UI;
using MahApps.Metro.Controls;
using OLP.AutoConnector.Resources;
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
            Properties.InputData.Default.Save();
            Close();
            /*if (TextBoxes_InputCheck())
            {
                DialogResult = true;
                Properties.InputData.Default.Save();
                Close();
            }
            else new MessageView("Проверка", FailureMessages.IncoorectInputValue, 150, 250, ButtonsVisibility.Ok).ShowDialog();*/
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

        private bool TextBoxes_InputCheck() => 
            double.TryParse(TextBox1.Text, out _) 
            & double.TryParse(TextBox2.Text, out _)
            & double.TryParse(TextBox3.Text, out _);
    }
}
