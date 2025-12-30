using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OLP.AutoConnectorKR.Views;
using Prism.Commands;
using Revit.Async;
using System.Collections.Generic;
using System.Windows.Input;

namespace OLP.AutoConnectorKR.Models
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
