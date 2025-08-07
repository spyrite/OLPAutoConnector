using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System.Collections.Generic;

namespace OLP.AutoConnector.Resources
{
    internal static class StructuralFilters
    {
        internal static readonly ElementFilter HostFilter = new LogicalOrFilter(
                [ new ElementCategoryFilter(BuiltInCategory.OST_Walls)
                        , new ElementCategoryFilter(BuiltInCategory.OST_Floors)
                        , new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns)
                        , new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming)
                        , new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation)]);

        internal static readonly List<string> ConcreteMaterailKeys = ["Бетон", "Железобетон"];
        internal static readonly string ConcreteInsertFamilyNameKey = "220_Закладная";
        internal static readonly string ConcreteCapFamilyNameKey = "220_Бетонная заглушка";
        internal static readonly string ConcreteCapMaterialParameterName = "Материал_Бетонная заглушка";
    }
}
