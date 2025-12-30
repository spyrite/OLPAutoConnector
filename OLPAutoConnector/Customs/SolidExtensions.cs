using Autodesk.Revit.DB;
using OLP.AutoConnector.Resources;
using System.Collections.Generic;
using System.Linq;
using static OLP.AutoConnector.Customs.XYZExtensions;
using static OLP.AutoConnector.Properties.ConnectRailings;

namespace OLP.AutoConnector.Customs
{
    internal static class SolidExtensions
    {
        /*internal static Solid CreateCrossoverSolid(XYZ startPoint, RailingData endOnPointData)
        {
            XYZ p0 = startPoint - endOnPointData.DirX.ABS() * Default.CrossoverSolidX / 304.8 / 2
                - endOnPointData.DirY.ABS() * Default.CrossoverSolidY / 304.8 / 2
                - endOnPointData.DirZ.ABS() * Default.CrossoverSolidZ / 304.8 / 2;
            XYZ p1 = p0 + endOnPointData.DirZ.ABS() * Default.CrossoverSolidZ / 304.8;
            XYZ p2 = p1 + endOnPointData.DirY.ABS() * Default.CrossoverSolidY / 304.8;
            XYZ p3 = p2 - endOnPointData.DirZ.ABS() * Default.CrossoverSolidZ / 304.8;

            CurveLoop cp = new();
            cp.Append(Line.CreateBound(p0, p1));
            cp.Append(Line.CreateBound(p1, p2));
            cp.Append(Line.CreateBound(p2, p3));
            cp.Append(Line.CreateBound(p3, p0));

            try { return GeometryCreationUtilities.CreateExtrusionGeometry([cp], endOnPointData.DirX.ABS(), Default.CrossoverSolidX / 304.8); }
            catch { return null;  }
        }*/

        internal static Solid GetSolid(FamilyInstance familyInstance, bool cloneSolid)
        {
            Solid solid = familyInstance.get_Geometry(new Options())?.Cast<GeometryInstance>()
                .First().GetInstanceGeometry()?.FirstOrDefault(geom => geom is Solid && (geom as Solid).Volume > 0) as Solid;

            if (solid != null & cloneSolid) return SolidUtils.Clone(solid);
            return solid;
        }

        internal static List<Solid> GetSolids(FamilyInstance familyInstance)
        {
            List<Solid> solids = familyInstance.get_Geometry(new Options())?.Cast<GeometryInstance>()
                .First().GetInstanceGeometry()?.Where(geom => geom is Solid && (geom as Solid).Volume > 0)
                .Cast<Solid>().ToList();

            return solids;
        }
    }
}
