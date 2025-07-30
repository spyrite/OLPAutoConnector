using MahApps.Metro.Controls;
using OLP.AutoConnector.ViewModels;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace OLP.AutoConnector.Views
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
            ShowDialog();
            return (DataContext as ActionsVM).SelectedNextAction;
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
            if (DoNotShowThisDialogCheckBox.IsChecked == true)
            {
                Properties.Actions.Default.AllowShowCountDialog = false;
                Properties.Actions.Default.Save();
            }

        }
    }
}
