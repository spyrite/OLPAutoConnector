using Autodesk.Revit.DB;
using System;
using System.Runtime.CompilerServices;

namespace OLP.AutoConnector.Customs
{
    public static class XYZExtensions
    {
        public static XYZ ABS(this XYZ vec) => new(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));

        public static double Multiply(XYZ vec1, XYZ vec2) => vec1.X * vec2.X + vec1.Y * vec2.Y + vec1.Z * vec2.Z;
    }
}
