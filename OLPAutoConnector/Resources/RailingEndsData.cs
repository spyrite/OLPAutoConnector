using Autodesk.Revit.DB;


using static OLP.AutoConnector.Resources.SupportedFamilyNames;

namespace OLP.AutoConnector.Resources
{
    internal enum RailingSide { Left, Right }

    internal class RailingEndData
    {
        //Исходные данные
        internal readonly double EndAxisRadius;
        internal readonly double HandrailDiameter;
        internal readonly double StairsAngle;
        internal readonly double StartEndDistance;

        //Проверяемые данные
        internal readonly double EndCapIsEnabled;

        //Вычисляемые данные
        internal double HandrailExtend;
        internal double EndIsEnabled;
        internal double EndAngleIP;
        internal double EndAngleOP;
        internal double EndLength;

        private readonly FamilyInstance _railing;       

        internal static double ConnectAlignX;
        internal static double ConnectAngle;

        internal Parameter HandrailExtendPar;
        internal Parameter EndIsEnabledPar;
        internal Parameter EndAngleIPPar;
        internal Parameter EndAngleOPPar;
        internal Parameter EndLengthPar;
        internal Parameter EndCapIsEnabledPar;

        internal RailingEndData(FamilyInstance railing)
        {
            _railing = railing;

            EndAxisRadius = railing.LookupParameter(FamilyParameterNames.Railings[railing.Symbol.FamilyName][16]).AsDouble();
            HandrailDiameter = railing.LookupParameter(FamilyParameterNames.Railings[railing.Symbol.FamilyName][17]).AsDouble();
            StairsAngle = railing.LookupParameter(FamilyParameterNames.Railings[railing.Symbol.FamilyName][18]).AsDouble();

            StartEndDistance = GetStartEndDistance();
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

        internal void ExtendData(RailingSide side)
        {
            switch (side)
            {
                case RailingSide.Left:
                    HandrailExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[railing.Symbol.FamilyName][16]).AsDouble();
                    EndIsEnabledPar;
                    EndAngleIPPar;
                    EndAngleOPPar;
                    EndLengthPar;
                    EndCapIsEnabledPar;
                    break;


                case RailingSide.Right:
                    break;
            }
        }


    }
}
