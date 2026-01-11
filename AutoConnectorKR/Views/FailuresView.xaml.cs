using MahApps.Metro.Controls;
using OLP.AutoConnectorKR.ViewModels;

namespace OLP.AutoConnectorKR.Views
{
    /// <summary>
    /// Логика взаимодействия для FailuresView.xaml
    /// </summary>
    public partial class FailuresView : MetroWindow
    {
        public FailuresView(FailuresVM failuresVM)
        {
            InitializeComponent();
            DataContext = failuresVM;
        }

        private void CloseButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Close();
        }
    }
}
