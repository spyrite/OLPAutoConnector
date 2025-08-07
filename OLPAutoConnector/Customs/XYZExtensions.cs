using Autodesk.Revit.DB;
using System;

namespace OLP.AutoConnector.Customs
{
    public static class XYZExtensions
    {
        public static XYZ VecABS(XYZ vec) => new(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));
    }
}
