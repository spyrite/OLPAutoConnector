using Autodesk.Revit.DB;
using MahApps.Metro.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Documents;

namespace OLP.AutoConnector.Views
{
    /// <summary>
    /// Логика взаимодействия для IDsListView.xaml
    /// </summary>
    public partial class IDsListView : MetroWindow
    {
        public IDsListView(List<ElementId> ids)
        {
            InitializeComponent();
            TextBox1.Text = string.Join(", ", ids.Select(id => id.IntegerValue.ToString()));
        }

        private void CopyToClipboard_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Clipboard.SetText(TextBox1.Text);
        }
    }
}
