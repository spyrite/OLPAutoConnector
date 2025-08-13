using Autodesk.Revit.DB;
using OLP.AutoConnector.Customs;
using System.Collections.Generic;
using System.Linq;
using static OLP.AutoConnector.Resources.SupportedFamilyNames;
using static OLP.AutoConnector.Customs.XYZExtensions;
using System;
using Autodesk.Revit.DB.Architecture;

namespace OLP.AutoConnector.Resources
{
    internal enum RailingSide { Left, Right }

    internal class RailingData
    {
        private readonly Document _doc;
        private readonly FamilyInstance _railing;

        //Исходные данные
        internal readonly string FamilyName;

        internal readonly double EndAxisRadius;
        internal readonly double HandrailDiameter;
        internal readonly double StairsAngle;
        internal readonly double StartEndDistance;

        internal readonly XYZ Origin;
        internal readonly XYZ DirX;
        internal readonly XYZ DirY;
        internal readonly XYZ DirZ;
        internal readonly XYZ HandrailOrigin;

        //Проверяемые данные
        internal readonly double EndCapIsEnabled;

        //Вычисляемые данные
        internal double HandrailExtend;
        internal double EndIsEnabled;
        internal double EndAngleIP;
        internal double EndAngleOP;
        internal double EndLength;

        internal static double ConnectAlignX;
        internal static double ConnectAngle;

        //Редактируемые параметры
        internal Parameter HandrailExtendPar;
        internal Parameter EndIsEnabledPar;
        internal Parameter EndAngleIPPar;
        internal Parameter EndAngleOPPar;
        internal Parameter EndLengthPar;
        internal Parameter EndCapIsEnabledPar;

        internal RailingData(FamilyInstance railing)
        {
            _railing = railing;
            _doc = railing.Document;

            FamilyName = railing.Symbol.FamilyName;

            EndAxisRadius = railing.LookupParameter(FamilyParameterNames.Railings[railing.Symbol.FamilyName][16]).AsDouble();
            HandrailDiameter = railing.LookupParameter(FamilyParameterNames.Railings[railing.Symbol.FamilyName][17]).AsDouble();
            StairsAngle = railing.LookupParameter(FamilyParameterNames.Railings[railing.Symbol.FamilyName][18]).AsDouble();
            StartEndDistance = GetStartEndDistance();
            HandrailOrigin = (railing.GetSubComponentIds().Select(id => _doc.GetElement(id) as FamilyInstance)
                .First(inst => inst.Symbol.FamilyName == SubFamilyNames.Railings[FamilyName][0]).Location as LocationPoint).Point;

            Origin = (railing.Location as LocationPoint).Point;
            DirX = railing.HandOrientation;
            DirY = railing.FacingOrientation;
            DirZ = XYZ.BasisZ;
        }

        private double GetStartEndDistance()
        {
            switch (_railing.Symbol.FamilyName)
            {
                case string when _railing.Symbol.FamilyName == StairsRailing1:
                    return _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][24]).AsDouble()
                        - _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][25]).AsDouble()/2;

                case string when _railing.Symbol.FamilyName == StairsRailing2:
                    return _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][19]).AsDouble();

                default:
                    return 0;
            }
        }

        internal XYZ GetEdgeRailingSupportOrigin(RailingSide side)
        {
            List<XYZ> supportOrigins = [.. _railing.GetSubComponentIds().Select(id => _doc.GetElement(id))
                .Cast<FamilyInstance>().Where(inst => inst.Symbol.FamilyName == SubFamilyNames.Railings[FamilyName][1])
                .Select(inst => (inst.Location as LocationPoint).Point)];

            switch (side)
            {
                case RailingSide.Left:
                    return supportOrigins.First(p => Multiply(p, DirX) == supportOrigins.Select(o => Multiply(o, DirX)).Min());
                case RailingSide.Right:
                    return supportOrigins.First(p => Multiply(p, DirX) == supportOrigins.Select(o => Multiply(o, DirX)).Max());
                default:
                    return null;
            }
        }

        internal void ExtendData(RailingSide side)
        {
            switch (side)
            {
                case RailingSide.Left:
                    HandrailExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][0]);
                    EndIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][2]);
                    EndAngleIPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][3]);
                    EndAngleOPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][4]);
                    EndLengthPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][5]);
                    EndCapIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][20]);
                    break;

                case RailingSide.Right:
                    HandrailExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][1]);
                    EndIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][9]);
                    EndAngleIPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][10]);
                    EndAngleOPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][11]);
                    EndLengthPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][12]);
                    EndCapIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][21]);
                    break;
            }
        }


    }
}
