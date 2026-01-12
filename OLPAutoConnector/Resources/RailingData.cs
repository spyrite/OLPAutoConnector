using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using OLP.AutoConnector.Customs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using static OLP.AutoConnector.Customs.XYZExtensions;
using static OLP.AutoConnector.Resources.SupportedFamilyNames;
using static System.Math;
using Frame = Autodesk.Revit.DB.Frame;

namespace OLP.AutoConnector.Resources
{
    internal enum RailingPositionZ { Upper, Lower }

    internal enum RailingSide { Left, Right }

    public enum RailingConnectionType
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
        internal static bool RailingsAreCounter;
        internal static double ConnectAngle;
        internal static XYZ ConnectAxisDir;
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
        internal readonly double BottomTopRefDistance;
        internal readonly double Height;
        internal readonly bool SupportsLeft;
        internal readonly bool SupportsRight;

        internal readonly XYZ Origin;
        internal readonly XYZ DirX;
        internal readonly XYZ DirY;
        internal readonly XYZ DirZ;
        internal readonly XYZ HandrailOrigin;
        internal readonly XYZ HandrailDirX;
        internal readonly XYZ HandrailDirY;
        internal readonly XYZ HandrailDirZ;
        internal XYZ EdgeSupportOrigin;

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
        internal double EdgeSupportAlign;

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
        internal Parameter EndNotInRailingPar;
        internal Parameter EdgeSupportAlignPar;

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
            HandrailAngle = _railing.Symbol.LookupParameter(FamilyParameterNames.Railings[FamilyName][18]).AsDouble();
            StartEndRefDistance = GetStartEndDistance();
            BottomTopRefDistance = StartEndRefDistance * Tan(HandrailAngle);
            Height = _railing.Symbol.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][30]).AsDouble();

            SupportsLeft = _railing.GetParameterFromInstOrSym(FamilyParameterNames.Railings[FamilyName][26])?.AsInteger() == 1;
            SupportsRight = _railing.GetParameterFromInstOrSym(FamilyParameterNames.Railings[FamilyName][28])?.AsInteger() == 1;

            FamilyInstance handrail = railing.GetSubComponentIds().Select(id => _doc.GetElement(id) as FamilyInstance)
                .First(inst => inst.Symbol.FamilyName == SubFamilyNames.Railings[FamilyName][0]);

            HandrailOrigin = (handrail.Location as LocationPoint).Point;
            HandrailDirX = handrail.HandOrientation;
            HandrailDirY = handrail.HandOrientation.CrossProduct(handrail.FacingOrientation);
            HandrailDirZ = handrail.FacingOrientation;
            

            Origin = (railing.Location as LocationPoint).Point;
            DirX = railing.HandOrientation;
            DirY = railing.FacingOrientation;
            DirZ = XYZ.BasisZ;

            Mirrored = railing.Mirrored;
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
            XYZ origin = Origin + (Height + ConnectDZFromHandrailTop - HandrailDiameter / 2) * DirZ;
            XYZ dirX = (Mirrored ? -1 : 1) * DirX;

            switch (RailingPositionZ)
            {
                //Нижнее, тип 2, 3; Верхнее, тип 1
                case RailingPositionZ.Upper when FamilyName == StairsRailing1:
                case RailingPositionZ.Lower when FamilyName == StairsRailing2 || FamilyName == StairsRailing3:
                    break;

                //Верхнее, тип 2, 3; Нижнее, тип 1
                case RailingPositionZ.Lower when FamilyName == StairsRailing1:
                    dirX = -dirX;
                    origin += BottomTopRefDistance * DirZ;
                    break;

                case RailingPositionZ.Upper when FamilyName == StairsRailing2 || FamilyName == StairsRailing3:
                    dirX = -dirX;
                    origin -= BottomTopRefDistance * DirZ;
                    break;
            }

            XYZ dirY = dirX.CrossProduct(DirZ);

            ConnectionXOYPlane = Plane.Create(new Frame(origin, dirX, dirY, DirZ));
        }

        internal XYZ GetEdgeRailingSupportOrigin(RailingSide side, bool includeLRSegments)
        {
            //Поиск всех стоек/кронштейнов
            List<FamilyInstance> supports = [.. _railing.GetSubComponentIds().Select(id => _doc.GetElement(id))
                .Cast<FamilyInstance>().Where(inst => inst.Symbol.FamilyName == SubFamilyNames.Railings[FamilyName][1])];

            //Определение центральных точек
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

            //Исключение стоек/кронштейнов на прямых участках ограждения
            if (SupportsLeft == true & includeLRSegments == false) supportOrigins = [.. supportOrigins.Where(p => Multiply(p, DirZ) != supportOrigins.Select(o => Multiply(o, DirZ)).Max())];
            if (SupportsRight == true & includeLRSegments == false) supportOrigins = [.. supportOrigins.Where(p => Multiply(p, DirZ) != supportOrigins.Select(o => Multiply(o, DirZ)).Min())];

            return side switch
            {
                RailingSide.Left => supportOrigins.First(p => Multiply(p, DirX) == supportOrigins.Select(o => Multiply(o, DirX)).Min()),
                RailingSide.Right => supportOrigins.First(p => Multiply(p, DirX) == supportOrigins.Select(o => Multiply(o, DirX)).Max()),
                _ => null,
            };
        }

        //Определение (инициализация) заполняемых и обнуляемых параметров
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
                    EdgeSupportAlignPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][35]);
                    break;
                case RailingSide.Right:
                    HandrailAngleExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][1]);
                    HandrailHorizontExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][12]);
                    HandrailAngleIPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][10]);
                    HandrailAngleOPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][11]);
                    EndIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][9]);
                    EndCapIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][21]);
                    EndTypePar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][23]);
                    EdgeSupportAlignPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][36]);
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

            switch (side)
            {
                case RailingSide.Left when FamilyName == StairsRailing1:
                    EndOtherPars.Add(_railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][31]));
                    break;
                case RailingSide.Left when FamilyName == StairsRailing2 || FamilyName == StairsRailing3:
                    EndNotInRailingPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][33]);
                    break;
                case RailingSide.Right when FamilyName == StairsRailing1:
                    EndOtherPars.Add(_railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][32]));
                    break;
                case RailingSide.Right when FamilyName == StairsRailing2 || FamilyName == StairsRailing3:
                    EndNotInRailingPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][34]);
                    break;
            }
        }

        internal bool CalculateData(out List<int> failureKeys)
        {
            /*EndAngleOP = -(HandrailDirZ.AngleTo(-ConnectAxisDir));
            EndAngleIP = PI - HandrailDirX.AngleTo(ConnectAxisDir);*/
            failureKeys = [];
            double endPreAngle = 0; //Угол поручня до концевика

            switch (RailingPositionZ, ConnectionType)
            {
                case (RailingPositionZ.Upper, RailingConnectionType.AngleHorizont):
                case (RailingPositionZ.Lower, RailingConnectionType.HorizontAngle):
                case (_, RailingConnectionType.AngleAngle):
                    endPreAngle = HandrailAngle;
                    break;
            }

            //Вычисление угла из плоскости
            double cZ = RailingsDistanceY / 2 * Tan(ConnectAngle); //Превышение стыка поручня из плоскости (проекция Z)
            double cZD = cZ * Cos(endPreAngle); //Проекция превышения на плоскоть сечения поручня
            double OPcor = Atan(cZD / (RailingsDistanceY / 2)); //Корректировка угла из плоскости 90 градусов
            switch (RailingPositionZ, ConnectionType)
            {
                //Семейство ограждения лестницы 1.1
                case (_, RailingConnectionType.AngleHorizont) when FamilyName == StairsRailing1:
                case (RailingPositionZ.Upper, RailingConnectionType.AngleAngle) when FamilyName == StairsRailing1:
                case (RailingPositionZ.Lower, RailingConnectionType.HorizontHorizont) when FamilyName == StairsRailing1:
                    EndAngleOP = PI / 2 - OPcor * (Mirrored ? 1 : -1) - (RailingsAreCounter ? PI : 0);
                    break;

                case (RailingPositionZ.Upper, RailingConnectionType.HorizontAngle) when FamilyName == StairsRailing1:
                case (RailingPositionZ.Upper, RailingConnectionType.HorizontHorizont) when FamilyName == StairsRailing1:
                    EndAngleOP = -PI / 2 + OPcor * (Mirrored ? 1 : -1) + (RailingsAreCounter ? PI : 0);
                    break;

                case (RailingPositionZ.Lower, RailingConnectionType.HorizontAngle) when FamilyName == StairsRailing1:
                case (RailingPositionZ.Lower, RailingConnectionType.AngleAngle) when FamilyName == StairsRailing1:
                    EndAngleOP = PI / 2 + OPcor * (Mirrored ? 1 : -1) - (RailingsAreCounter ? PI : 0);
                    break;



                //Семейство ограждения лестницы 1.3
                case (RailingPositionZ.Upper, RailingConnectionType.AngleHorizont) when FamilyName == StairsRailing2:
                case (RailingPositionZ.Lower, RailingConnectionType.HorizontAngle) when FamilyName == StairsRailing2:
                case (_, RailingConnectionType.AngleAngle) when FamilyName == StairsRailing2:
                    EndAngleOP = PI / 2 + OPcor - (RailingsAreCounter ? PI : 0);
                    break;

                case (RailingPositionZ.Upper, RailingConnectionType.HorizontAngle) when FamilyName == StairsRailing2:
                case (RailingPositionZ.Lower, RailingConnectionType.AngleHorizont) when FamilyName == StairsRailing2:
                case (_, RailingConnectionType.HorizontHorizont) when FamilyName == StairsRailing2:
                    EndAngleOP = PI / 2 - OPcor - (RailingsAreCounter ? PI : 0);
                    break;


                //Семейство поручня
                case (RailingPositionZ.Upper, RailingConnectionType.AngleHorizont) when FamilyName == StairsRailing3:
                case (RailingPositionZ.Lower, RailingConnectionType.HorizontAngle) when FamilyName == StairsRailing3:
                case (_, RailingConnectionType.AngleAngle) when FamilyName == StairsRailing3:
                    EndAngleOP = -PI / 2 + OPcor + (RailingsAreCounter ? 0 : PI);
                    break;

                case (RailingPositionZ.Upper, RailingConnectionType.HorizontAngle) when FamilyName == StairsRailing3:
                case (RailingPositionZ.Upper, RailingConnectionType.HorizontHorizont) when FamilyName == StairsRailing3:
                    EndAngleOP = -PI / 2 + OPcor * (EndNotInRailingPar?.AsInteger() == 1 & SupportsRight == true ? 1 : -1) + (RailingsAreCounter ? 0 : PI);
                    break;


                case (RailingPositionZ.Lower, RailingConnectionType.AngleHorizont) when FamilyName == StairsRailing3:
                case (RailingPositionZ.Lower, RailingConnectionType.HorizontHorizont) when FamilyName == StairsRailing3:
                    EndAngleOP = -PI / 2 + OPcor * (EndNotInRailingPar?.AsInteger() == 1 & SupportsLeft == true ? 1 : -1) + (RailingsAreCounter ? 0 : PI);
                    break;


            }

            //Вычисление угла в плоскости
            double cZhr = cZ * Sin(endPreAngle); //Проекция превышения на ось поручня
            double c2Y2 = (RailingsDistanceY / 2) / Cos(ConnectAngle); //Гипотенуза между превышением и половиной осевого расстояния между ограждениями
            double IPcor = Asin(cZhr / c2Y2); //Корректировка угла в плоскости 90 градусов


            switch (FamilyName)
            {
                case string when FamilyName == StairsRailing1:
                    EndAngleIP = PI / 2 - IPcor * (Mirrored ? 1 : -1) * (RailingsAreCounter ? -1 : 1);
                    break;
                case string when FamilyName == StairsRailing2:
                    EndAngleIP = PI / 2 - IPcor * (RailingsAreCounter ? -1 : 1);
                    break;
                case string when FamilyName == StairsRailing3:
                    EndAngleIP = PI / 2 + IPcor * (RailingsAreCounter ? 1 : -1);
                    break;
            }
            

            //Вычисление удлинения поручня
            double a2Axis = EndAxisRadius * Tan(EndAngleIP / 2);
            switch (RailingPositionZ, ConnectionType)
            {
                case (RailingPositionZ.Upper, RailingConnectionType.HorizontAngle)
                or (RailingPositionZ.Lower, RailingConnectionType.AngleHorizont)
                or (_, RailingConnectionType.HorizontHorizont):
                    CalculateHandrailAngles();
                    int failureKey = CalculateHorizontHandrailExtend(a2Axis);
                    if (failureKey > -1)
                    {
                        failureKeys.Add(failureKey);
                        return false;
                    }
                    break;

                default:
                    if (CalculateAngleHandrailExtend(a2Axis) == false)
                    {
                        failureKeys.Add(7);
                        return false;
                    }
                    break;
            }

            //Вычисление удлинения концевика
            EndLength = (RailingsDistanceY / 2) / Cos(ConnectAngle) - a2Axis;
            return true;
        }

        private int CalculateHorizontHandrailExtend(double a2Axis)
        {
            
            switch (RailingPositionZ)
            {
                case RailingPositionZ.Upper when FamilyName == StairsRailing1:
                    HandrailAngleExtend = -ConnectDZFromHandrailTop / Tan(HandrailAngle);
                    HandrailAngleExtend -= (2 * (EndAxisRadius - HandrailDiameter / 2) * Sin(HandrailAngle / 2) * Sin(HandrailAngle / 2)
                    + HandrailDiameter / 2 * Sin(HandrailAngle) * Tan(HandrailAngle)) / Tan(HandrailAngle);
                    break;
                case RailingPositionZ.Lower when FamilyName == StairsRailing1:
                    HandrailAngleExtend = ConnectDZFromHandrailTop / Tan(HandrailAngle);
                    HandrailAngleExtend -= (2 * (EndAxisRadius + HandrailDiameter / 2) * Sin(HandrailAngle / 2) * Sin(HandrailAngle / 2)
                        - HandrailDiameter / 2 * Sin(HandrailAngle) * Tan(HandrailAngle)) / Tan(HandrailAngle);
                    break;

                case RailingPositionZ.Upper when FamilyName == StairsRailing2 || FamilyName == StairsRailing3:
                    HandrailAngleExtend = - ConnectDZFromHandrailTop / Sin(HandrailAngle);
                    break;
                case RailingPositionZ.Lower when FamilyName == StairsRailing2 || FamilyName == StairsRailing3:
                    HandrailAngleExtend = ConnectDZFromHandrailTop / Sin(HandrailAngle);
                    break;
            }            

            XYZ p0 = HandrailOrigin;
            XYZ p1 = HandrailOrigin.ProjectOnPlane(ConnectionXOYPlane, (PI/2 - HandrailAngle) * (Mirrored ? 1 : -1), ConnectionXOYPlane.XVec);

            //ТЕСТ
            /*Plane railingPlane = Plane.Create(new Frame(HandrailOrigin, DirX, DirZ, DirY));
            using (Transaction tx = new(_doc, "OLP test")) { tx.Start(); _doc.Create.NewModelCurve(Line.CreateBound(p0, p1), SketchPlane.Create(_doc, railingPlane)); tx.Commit(); }*/

            ConnectionYOZPlane.Project(p1, out _, out HandrailHorizontExtend);
            HandrailHorizontExtend -= a2Axis + EndAxisRadius * Tan(HandrailAngle / 2);
            if (HandrailHorizontExtend < 0) return 7;

            if (EdgeSupportOrigin != null)
            {
                ConnectionYOZPlane.ProjectWithToken(EdgeSupportOrigin, out EdgeSupportAlign);
                EdgeSupportAlign = (Mirrored ? -1 : 1) * EdgeSupportAlign - EndAxisRadius;
                if (EdgeSupportAlign < 0) return 5;
            }

            return -1;
        }


        private bool CalculateAngleHandrailExtend(double a2Axis)
        {
            double a2Top = (EndAxisRadius - HandrailDiameter / 2) * Tan(EndAngleIP / 2);

            switch (FamilyName)
            {
                case string when FamilyName == StairsRailing1:
                    HandrailAngleExtend = ConnectXFromRefPlane - a2Axis * Cos(HandrailAngle);
                    break;

                case string when FamilyName == StairsRailing2:
                case string when FamilyName == StairsRailing3:
                    switch (RailingPositionZ)
                    {
                        case RailingPositionZ.Upper:
                            HandrailAngleExtend = ConnectXFromRefPlane / Cos(HandrailAngle) * (Mirrored ? -1 : 1) - (a2Axis - a2Top - HandrailDiameter / 2 * Tan(HandrailAngle));
                            break;
                        case RailingPositionZ.Lower:
                            HandrailAngleExtend = ConnectXFromRefPlane / Cos(HandrailAngle) * (Mirrored ? -1 : 1) + (a2Axis - a2Top - HandrailDiameter / 2 * Tan(HandrailAngle));
                            break;
                    }
                    if (HandrailAngleExtend < 0) return false;
                    break;
            }
            return true;
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
