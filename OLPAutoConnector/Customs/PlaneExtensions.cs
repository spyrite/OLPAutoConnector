using Autodesk.Revit.DB;

namespace OLP.AutoConnector.Customs
{
    public static class PlaneExtensions
    {
        public static void ProjectWithToken(this Plane plane, XYZ p0, out double dst)
        {
            plane.Project(p0, out UV uv, out dst);
            XYZ p1 = plane.Origin + uv.U * plane.XVec + uv.V * plane.YVec;
            XYZ dir = p1.IsAlmostEqualTo(p0) ? plane.Normal : Line.CreateBound(p0, p1).Direction.Normalize();
            dst = dst * (dir.IsAlmostEqualTo(plane.Normal) ? -1 : 1);
        }
    }
}
