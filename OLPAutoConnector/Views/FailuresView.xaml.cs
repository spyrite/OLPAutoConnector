using MahApps.Metro.Controls;
using OLP.AutoConnector.ViewModels;

namespace OLP.AutoConnector.Views
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
    }
}
