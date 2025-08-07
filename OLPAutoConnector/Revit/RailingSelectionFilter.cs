using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

using static OLP.AutoConnector.Resources.SupportedFamilyNames;

namespace OLP.AutoConnector.Revit
{
    internal class RailingSelectionFilter : ISelectionFilter
    {
        internal readonly List<string> SupportedFamilyNames =
                                    [
                                        StairsRailing1,
                                        StairsRailing2
                                    ];

        public bool AllowElement(Element elem)
        {
            return elem is FamilyInstance && SupportedFamilyNames.Contains((elem as FamilyInstance).Symbol.FamilyName);
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}
