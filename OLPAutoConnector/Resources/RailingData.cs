using Autodesk.Revit.DB;
using OLP.AutoConnector.Customs;
using System.Collections.Generic;
using System.Linq;
using static OLP.AutoConnector.Customs.XYZExtensions;
using static OLP.AutoConnector.Resources.SupportedFamilyNames;
using static System.Math;

namespace OLP.AutoConnector.Resources
{
    internal enum RailingPositionZ { Upper, Lower }

    internal enum RailingSide { Left, Right }

    internal enum RailingConnectionType
    {
        AngleAngle,
        HorizontAngle,
        AngleHorizont,
        HorizontHorizont
    }

    internal class RailingData
    {
        //Статичные данные
        internal static double MinRailingsDistanceY;
        internal static double RailingsDistanceY;
        internal static double ConnectAngle;
        internal static double ConnectXFromEdgeSupport;
        internal static Plane ConnectionYOZPlane;
        internal static RailingConnectionType ConnectionType;

        private readonly Document _doc;
        private readonly FamilyInstance _railing;

        //Исходные данные
        internal readonly string FamilyName;
        internal readonly ElementId Id;
        internal readonly ElementId EndTypeId;

        internal readonly double EndAxisRadius;
        internal readonly double HandrailDiameter;
        internal readonly double HandrailAngle;
        internal readonly double StartEndRefDistance;

        internal readonly XYZ Origin;
        internal readonly XYZ DirX;
        internal readonly XYZ DirY;
        internal readonly XYZ DirZ;
        internal readonly XYZ HandrailOrigin;

        internal RailingPositionZ RailingPositionZ;

        internal double ConnectXFromRefPlane;
        internal double ConnectDZFromHandrailTop;
        internal Plane ConnectionXOYPlane;

        internal bool Mirrored;

        //Вычисляемые данные
        internal double HandrailAngleExtend;
        internal double HandrailHorizontExtend;
        internal double HandrailAngleIP;
        internal double HandrailAngleOP;
        internal double EndAngleIP;
        internal double EndAngleOP;
        internal double EndLength;

        //Редактируемые параметры
        internal Parameter HandrailAngleExtendPar;
        internal Parameter HandrailHorizontExtendPar;
        internal Parameter HandrailAngleIPPar;
        internal Parameter HandrailAngleOPPar;
        internal Parameter EndTypePar;
        internal Parameter EndIsEnabledPar;
        internal Parameter EndAngleIPPar;
        internal Parameter EndAngleOPPar;
        internal Parameter EndLengthPar;
        internal Parameter EndCapIsEnabledPar;

        internal List<Parameter> EndOtherPars;

        internal RailingData(FamilyInstance railing)
        {
            _railing = railing;
            _doc = railing.Document;

            FamilyName = railing.Symbol.FamilyName;
            Id = railing.Id;
            EndTypeId = new FilteredElementCollector(_doc).OfClass(typeof(FamilySymbol)).Cast<FamilySymbol>()
                .FirstOrDefault(sym => sym.FamilyName == SubFamilyNames.Railings[FamilyName][2])?.Id ?? ElementId.InvalidElementId;

            EndAxisRadius = railing.Symbol.LookupParameter(FamilyParameterNames.Railings[railing.Symbol.FamilyName][16]).AsDouble();
            HandrailDiameter = railing.Symbol.LookupParameter(FamilyParameterNames.Railings[railing.Symbol.FamilyName][17]).AsDouble();
            HandrailAngle = GetHandrailAngle();
            StartEndRefDistance = GetStartEndDistance();
            HandrailOrigin = (railing.GetSubComponentIds().Select(id => _doc.GetElement(id) as FamilyInstance)
                .First(inst => inst.Symbol.FamilyName == SubFamilyNames.Railings[FamilyName][0]).Location as LocationPoint).Point;

            Origin = (railing.Location as LocationPoint).Point;
            DirX = railing.HandOrientation;
            DirY = railing.FacingOrientation;
            DirZ = XYZ.BasisZ;

            Mirrored = railing.Mirrored;

        }

        private double GetHandrailAngle()
        {
            return (ConnectionType, RailingPositionZ) switch
            {
                (RailingConnectionType.HorizontAngle, RailingPositionZ.Upper) => 0,
                (RailingConnectionType.AngleHorizont, RailingPositionZ.Lower) => 0,
                (RailingConnectionType.HorizontHorizont, _) => 0,
                _ => _railing.Symbol.LookupParameter(FamilyParameterNames.Railings[FamilyName][18]).AsDouble(),
            };
        }

        private double GetStartEndDistance()
        {
            return FamilyName switch
            {
                string when FamilyName == StairsRailing1 => _railing.Symbol.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][24]).AsDouble()
                                        - _railing.Symbol.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][25]).AsDouble() / 2,
                string when FamilyName == StairsRailing2 => _railing.Symbol.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][19]).AsDouble(),
                string when FamilyName == StairsRailing3 => _railing.Symbol.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][19]).AsDouble(),
                _ => 0,
            };
        }

        internal void SetConnectionXOYPlane()
        {
            ConnectionXOYPlane = Plane.CreateByOriginAndBasis(Origin +
                (_railing.Symbol.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][30]).AsDouble()
                + ConnectDZFromHandrailTop - HandrailDiameter/2) * DirZ,
                DirX, DirY);
        }

        internal XYZ GetEdgeRailingSupportOrigin(RailingSide side)
        {
            List<FamilyInstance> supports = [.. _railing.GetSubComponentIds().Select(id => _doc.GetElement(id))
                .Cast<FamilyInstance>().Where(inst => inst.Symbol.FamilyName == SubFamilyNames.Railings[FamilyName][1])];
            List<XYZ> supportOrigins = [];

            switch (FamilyName)
            {
                case string when FamilyName == StairsRailing1:
                case string when FamilyName == StairsRailing3:
                    supportOrigins = [.. supports.Select(inst => (inst.Location as LocationPoint).Point)];
                    break;
                case string when FamilyName == StairsRailing2:
                    foreach (FamilyInstance support in supports)
                        supportOrigins.AddRange(SolidExtensions.GetSolids(support).Select(s => s.ComputeCentroid()));
                    break;
            }
            
            return side switch
            {
                RailingSide.Left => supportOrigins.First(p => Multiply(p, DirX) == supportOrigins.Select(o => Multiply(o, DirX)).Min()),
                RailingSide.Right => supportOrigins.First(p => Multiply(p, DirX) == supportOrigins.Select(o => Multiply(o, DirX)).Max()),
                _ => null,
            };
        }

        internal void InitilizeParameters(RailingSide side)
        {
            switch (side)
            {
                case RailingSide.Left:
                    HandrailAngleExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][0]);
                    HandrailHorizontExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][5]);
                    HandrailAngleIPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][3]);
                    HandrailAngleOPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][4]);
                    EndIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][2]);
                    EndCapIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][20]);
                    EndTypePar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][22]);
                    break;
                case RailingSide.Right:
                    HandrailAngleExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][1]);
                    HandrailHorizontExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][12]);
                    HandrailAngleIPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][10]);
                    HandrailAngleOPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][11]);
                    EndIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][9]);
                    EndCapIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][21]);
                    EndTypePar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][23]);
                    break;
            }

            List<int> Ns = [];
            switch (ConnectionType, RailingPositionZ, side)
            {
                case (RailingConnectionType.HorizontAngle, RailingPositionZ.Upper, RailingSide.Left)
                or (RailingConnectionType.AngleHorizont, RailingPositionZ.Lower, RailingSide.Left)
                or (RailingConnectionType.HorizontHorizont, _, RailingSide.Left):
                    Ns = [6, 7, 8, 3, 4, 5];
                    break;

                case (RailingConnectionType.HorizontAngle, RailingPositionZ.Upper, RailingSide.Right)
                or (RailingConnectionType.AngleHorizont, RailingPositionZ.Lower, RailingSide.Right)
                or (RailingConnectionType.HorizontHorizont, _, RailingSide.Right):
                    Ns = [13, 14, 15, 10, 11, 12];
                    break;

                case (_, _, RailingSide.Left):
                    Ns = [3, 4, 5, 6, 7, 8];
                    break;

                case (_, _, RailingSide.Right):
                    Ns = [10, 11, 12, 13, 14, 15];
                    break;
            }

            EndAngleIPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns[0]]);
            EndAngleOPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns[1]]);
            EndLengthPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns[2]]);
            EndOtherPars = [.. new List<int> { Ns[3], Ns[4], Ns[5] }
                    .Select(i => _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][i]))];
        }

        internal void CalculateData()
        {
            double cZ = RailingsDistanceY / 2 * Tan(ConnectAngle);
            double cZD = cZ * Cos(HandrailAngle);
            double OPcor = Atan(cZD / (RailingsDistanceY / 2));

            switch (RailingPositionZ)
            {
                case RailingPositionZ.Upper:
                    switch (FamilyName)
                    {
                        case string when FamilyName == StairsRailing1:
                            EndAngleOP = PI / 2 - OPcor;
                            break;
                        case string when FamilyName == StairsRailing2:
                            EndAngleOP = PI / 2 + OPcor;
                            break;
                        case string when FamilyName == StairsRailing3:
                            EndAngleOP = -PI / 2 - OPcor;
                            break;
                    }
                    break;
                case RailingPositionZ.Lower:
                    switch (FamilyName)
                    {
                        case string when FamilyName == StairsRailing1:
                        case string when FamilyName == StairsRailing2:
                            EndAngleOP = PI / 2 + OPcor;
                            break;
                        case string when FamilyName == StairsRailing3:
                            EndAngleOP = -PI / 2 + OPcor;
                            break;
                    }
                    break;
            }

            double cZhr = cZ * Sin(HandrailAngle);
            double c2Y2 = (RailingsDistanceY / 2) / Cos(ConnectAngle);
            double IPcor = Asin(cZhr / c2Y2);
            EndAngleIP = PI / 2 - IPcor;

            double a2Axis = EndAxisRadius * Tan(EndAngleIP / 2);

            switch (ConnectionType, RailingPositionZ)
            {
                case (RailingConnectionType.HorizontAngle, RailingPositionZ.Upper)
                or (RailingConnectionType.AngleHorizont, RailingPositionZ.Lower)
                or (RailingConnectionType.HorizontHorizont, _):
                    CalculateHandrailAngles();
                    CalculateHorizontHandrailExtend(a2Axis);
                    break;

                default:
                    CalculateAngleHandrailExtend(a2Axis);
                    break;
            }

            EndLength = (RailingsDistanceY / 2) / Cos(ConnectAngle) - a2Axis;
        }

        private void CalculateHorizontHandrailExtend(double a2Axis)
        {   
            HandrailAngleExtend = ConnectDZFromHandrailTop / Tan(HandrailAngle);
            ConnectionYOZPlane.Project(HandrailOrigin.ProjectOnPlane(ConnectionXOYPlane, PI - HandrailAngle, ConnectionXOYPlane.XVec),
                out _, out HandrailHorizontExtend);
            HandrailHorizontExtend -= a2Axis + EndAxisRadius * Tan(HandrailAngle / 2);
        }


        private void CalculateAngleHandrailExtend(double a2Axis)
        {
            double a2Top = (EndAxisRadius - HandrailDiameter / 2) * Tan(EndAngleIP / 2);

            switch (FamilyName)
            {
                case string when FamilyName == StairsRailing1:
                    HandrailAngleExtend = ConnectXFromRefPlane - a2Axis * Cos(HandrailAngle);
                    break;
                case string when FamilyName == StairsRailing2:
                case string when FamilyName == StairsRailing3:
                    HandrailAngleExtend = ConnectXFromRefPlane / Cos(HandrailAngle) - (a2Axis - a2Top - HandrailDiameter / 2 * Tan(HandrailAngle));
                    break;
            }
        }

        private void CalculateHandrailAngles()
        {
            HandrailAngleIP = HandrailAngle;
            switch (FamilyName)
            {
                case string when FamilyName == StairsRailing1:
                    switch (RailingPositionZ)
                    {
                        case RailingPositionZ.Upper:
                            HandrailAngleOP = PI;
                            break;
                        case RailingPositionZ.Lower:
                            HandrailAngleOP = 0;
                            break;
                    }
                    break;
                case string when FamilyName == StairsRailing2:
                case string when FamilyName == StairsRailing3:
                    HandrailAngleOP = 0;
                    break;
            }
        }
    }
}
