using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace OLPAutoConnector.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class ConnectCIsToHost : IExternalCommand
    {
        public static UIApplication UIApp;
        public static UIDocument UIDoc;
        public static Autodesk.Revit.ApplicationServices.Application App;
        public static Document Doc;

        private List<FamilyInstance> _selectedFIs;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            UIApp = commandData.Application;
            UIDoc = commandData.Application.ActiveUIDocument;
            App = commandData.Application.Application;
            Doc = commandData.Application.ActiveUIDocument.Document;

            _selectedFIs = [.. new FilteredElementCollector(Doc, UIDoc.Selection.GetElementIds()).OfClass(typeof(FamilyInstance))
                .Cast<FamilyInstance>().Where(inst => inst.Symbol.FamilyName.Contains("220_"))];

            if (_selectedFIs.Any())
            {
                foreach (FamilyInstance ci in _selectedFIs)
                {
                    if (ci.Host is Wall || ci.Host is FamilyInstance || ci.Host is Floor)
                    {
                        JoinGeometryUtils.JoinGeometry(Doc, ci, ci.Host);
                    }
                        
                }

                return Result.Succeeded;
            }

            else
            {
                message = "Не выбрано ни одного элемента. Команда отменена.";
                return Result.Cancelled;
            }
        }

        private ElementId GetHostMaterialId(Element host)
        {
            ElementId materialId = ElementId.InvalidElementId;

            

            return materialId;
        }
    }
}
