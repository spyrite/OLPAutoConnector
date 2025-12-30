using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using static OLP.AutoConnectorKR.Resources.StructuralFilters;

namespace OLP.AutoConnectorKR.Revit
{
    internal class CIsSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem) => elem is FamilyInstance && (elem as FamilyInstance).Symbol.FamilyName.Contains(ConcreteInsertFamilyNameKey);

        public bool AllowReference(Reference reference, XYZ position) => false;
    }
}
