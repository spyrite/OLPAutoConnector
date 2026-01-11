using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OLP.AutoConnector.Views;
using Prism.Commands;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace OLP.AutoConnector.Models
{
    public class FailureModel
    {
        private UIDocument _uidoc;

        public string Message { get; private set; }
        public List<ElementId> Ids { get; set; }
        public int IdsCount { get => Ids.Count; }

        public FailureModel(UIDocument uidoc, string message)
        {
            _uidoc = uidoc;
            Message = message;
            Ids = [];
        }

        public ICommand SelectIdsCommand => new DelegateCommand(SelectIds);
        private async void SelectIds()
        {
            await RevitTask.RunAsync((uiApp) => _uidoc.Selection.SetElementIds(Ids));
        }

        public ICommand ShowIdsCommand => new DelegateCommand(ShowIds);
        private void ShowIds()
        {
            new IDsListView(Ids).ShowDialog();
        }
    }
}
