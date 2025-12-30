using MahApps.Metro.Controls;
using OLP.AutoConnectorKR.ViewModels;
using System.Windows;

namespace OLP.AutoConnectorKR.Views
{
    /// <summary>
    /// Логика взаимодействия для ActionsView.xaml
    /// </summary>
    public partial class ActionsView : MetroWindow
    {
        public ActionsView(ActionsVM actionsVM)
        {
            InitializeComponent();
            DataContext = actionsVM;
            switch ((DataContext as ActionsVM).SelectedNextAction)
            {
                case ActionsVM.NextAction.AllowUserSelection: radioButton1.IsChecked = true; break;
                case ActionsVM.NextAction.SelectAllOnActiveView: radioButton2.IsChecked = true; break;
                case ActionsVM.NextAction.SelectAllInModel: radioButton3.IsChecked = true; break;
                case ActionsVM.NextAction.Cancel: radioButton1.IsChecked = true; break;
            }
        }

        public ActionsVM.NextAction ShowNextActionsDialog()
        {
            bool? result = ShowDialog();
            ActionsVM vm = DataContext as ActionsVM;
            if (result == null || result == false) vm.SelectedNextAction = ActionsVM.NextAction.Cancel;

            return vm.SelectedNextAction;
        }

        private void radioButton1_Checked(object sender, RoutedEventArgs e)
        {
            (DataContext as ActionsVM).SelectedNextAction = ActionsVM.NextAction.AllowUserSelection;
        }

        private void radioButton2_Checked(object sender, RoutedEventArgs e)
        {
            (DataContext as ActionsVM).SelectedNextAction = ActionsVM.NextAction.SelectAllOnActiveView;
        }

        private void radioButton3_Checked(object sender, RoutedEventArgs e)
        {
            (DataContext as ActionsVM).SelectedNextAction = ActionsVM.NextAction.SelectAllInModel;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            (DataContext as ActionsVM).SelectedNextAction = ActionsVM.NextAction.Cancel;
            Close();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            if (DoNotShowThisDialogCheckBox.IsChecked == true)
            {
                Properties.Actions.Default.AllowShowCountDialog = false;
                Properties.Actions.Default.Save();
            }
            Close();
        }
    }
}
