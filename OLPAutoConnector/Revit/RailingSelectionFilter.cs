using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using OLP.AutoConnector.Resources;
using System.Collections.Generic;

using static OLP.AutoConnector.Resources.SupportedFamilyNames;

namespace OLP.AutoConnector.Revit
{
    internal class RailingSelectionFilter : ISelectionFilter
    {
        private readonly List<string> _supportedFamilyNames = [.. FamilyParameterNames.Railings.Keys];
        private List<ElementId> _excludingIds;

        public RailingSelectionFilter()
        {
            _excludingIds = [];
        }

        public RailingSelectionFilter(List<ElementId> excludingIds)
        {
            _excludingIds = excludingIds;
        }

        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance 
                && _supportedFamilyNames.Contains((elem as FamilyInstance).Symbol.FamilyName)
                && !_excludingIds.Contains(elem.Id);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
