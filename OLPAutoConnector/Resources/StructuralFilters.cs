using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


        //private static Guid _materialParameterGuid = new Guid("8b5e61a2-b091-491c-8092-0b01a55d4f44");
        internal static readonly List<string> ConcreteMaterailKeys = ["Бетон", "Железобетон"];
        internal static readonly string ConcreteInsertFamilyNameKey = "220_Закладная";
        internal static readonly string ConcreteCapFamilyNameKey = "220_Бетонная заглушка";
        internal static readonly string _concreteCapMaterialParameterName = "Материал_Бетонная заглушка";
    }
}
