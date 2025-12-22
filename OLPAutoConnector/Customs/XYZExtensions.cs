using Autodesk.Revit.DB;
using System;
using System.Runtime.CompilerServices;

namespace OLP.AutoConnector.Customs
{
    public static class XYZExtensions
    {
        public static XYZ ABS(this XYZ vec) => new(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));

        public static double Multiply(XYZ vec1, XYZ vec2) => vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z;

        public static XYZ Multiply2(this XYZ vec1, XYZ vec2) => new (vec1.X * vec2.X, vec1.Y * vec2.Y, vec1.Z * vec2.Z);

        public static XYZ ProjectOnPlane(this XYZ sourcePoint, Plane plane)
        {
            plane.Project(sourcePoint, out UV uv, out double _);
            return plane.Origin + uv.U * plane.XVec + uv.V * plane.YVec;
        }
        public static XYZ ProjectOnPlane(this XYZ sourcePoint, Plane plane, double angle, XYZ offsetDir)
        {
            plane.Project(sourcePoint, out UV uv, out double dst);
            return plane.Origin + uv.U * plane.XVec + uv.V * plane.YVec + dst * Math.Tan(angle) * offsetDir;
        }

        public static bool IsCollinearAndCounterTo(this XYZ vec1, XYZ vec2, XYZ origin1, XYZ origin2)
        {
            if (!vec1.Normalize().ABS().IsAlmostEqualTo(vec2.Normalize().ABS())) return false;

            Plane midPlane = Plane.CreateByNormalAndOrigin(vec1, Line.CreateBound(origin1, origin2).Evaluate(0.5, true));
            midPlane.Project(origin1, out _, out double dst);

            bool condition1 = origin1.ProjectOnPlane(midPlane)
                .IsAlmostEqualTo(Transform.CreateTranslation(vec1*dst).OfPoint(origin1));
            bool condition2 = origin2.ProjectOnPlane(midPlane)
                .IsAlmostEqualTo(Transform.CreateTranslation(vec2*dst).OfPoint(origin2));

            return condition1 & condition2;
        }
    }
}
