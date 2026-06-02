using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OLP.AutoConnector.Resources.SupportedFamilyNames;

namespace OLP.AutoConnector.Resources
{
    public enum HandrailPositionZ { Upper, Lower }

    public class HandrailData
    {
        private readonly Document _doc;
        private readonly FamilyInstance _railing;

        public Element Elem { get; }
        public XYZ HandrailOrigin { get; }
        public XYZ HandrailDirX { get; }
        public XYZ HandrailDirY { get; }
        public XYZ HandrailDirZ { get; }
        public double HandrailDiameter { get; }
        public double HandrailAngle { get; }

        public string DisplayHeight { get => Height == 0 ? "Н/Д" : Math.Round(Height * 304.8).ToString(); }
        public double Height { get; private set; }
        public HandrailPositionZ PositionZ { get; private set; }

        public double ConnectDZFromHandrailTop { get; set; }
        public Plane ConnectionXOYPlane { get; set; }
        

        public double HandrailAngleExtend { get; set; }
        public double HandrailHorizontExtend { get; set; }
        public double HandrailAngleIP { get; set; }
        public double HandrailAngleOP { get; set; }
        public double EndAngleIP { get; set; }
        public double EndAngleOP { get; set; }
        public double EndLength { get; set; }


        public Parameter HandrailAngleExtendPar { get; set; }
        public Parameter HandrailHorizontExtendPar { get; set; }
        public Parameter HandrailAngleIPPar { get; set; }
        public Parameter HandrailAngleOPPar { get; set; }
        public Parameter EndIsEnabledPar { get; set; }
        public Parameter EndCapIsEnabledPar { get; set; }
        public Parameter EndAngleIPPar { get; set; }
        public Parameter EndAngleOPPar { get; set; }
        public Parameter EndLengthPar { get; set; }
        public List<Parameter> EndOtherPars { get; set; }
        public Parameter EndNotInRailingPar { get; set; }
        public HandrailData(FamilyInstance railing, FamilyInstance handrail)
        {
            _railing = railing;
            _doc = railing.Document;

            Elem = handrail;
            HandrailOrigin = (handrail.Location as LocationPoint).Point;
            HandrailDirX = handrail.HandOrientation;
            HandrailDirY = handrail.HandOrientation.CrossProduct(handrail.FacingOrientation);
            HandrailDirZ = handrail.FacingOrientation;
        }
        public HandrailData() {}

        public void ExtendData(HandrailPositionZ positionZ)
        {
            PositionZ = positionZ;
            switch (positionZ)
            {
                case HandrailPositionZ.Upper:
                    Height = _railing.Symbol.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][30])?.AsDouble() ?? 0;
                    break;
                case HandrailPositionZ.Lower:
                    Height = _railing.Symbol.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][56])?.AsDouble() ?? 0;
                    break;
            }    
             
        }

        public void InitilizeParameters(RailingSide side, RailingPositionZ railingPositionZ)
        {
            List<int> Ns1 = [];
            switch (side, PositionZ)
            {
                case (RailingSide.Left, HandrailPositionZ.Upper):
                    Ns1 = [0, 5, 3, 4, 2, 20];
                    break;
                case (RailingSide.Right, HandrailPositionZ.Upper):
                    Ns1 = [1, 12, 10, 11, 9, 21];
                    break;

                case (RailingSide.Left, HandrailPositionZ.Lower):
                    Ns1 = [37, 42, 40, 41, 39, 54];
                    break;

                case (RailingSide.Right, HandrailPositionZ.Lower):
                    Ns1 = [38, 49, 47, 48, 46, 55];
                    break;
            }
            HandrailAngleExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns1[0]]);
            HandrailHorizontExtendPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns1[1]]);
            HandrailAngleIPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns1[2]]);
            HandrailAngleOPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns1[3]]);
            EndIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns1[4]]);
            EndCapIsEnabledPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns1[5]]);


            List<int> Ns2 = [];
            switch (RailingData.ConnectionType, railingPositionZ, side, PositionZ)
            {
                case (RailingConnectionType.HorizontAngle, RailingPositionZ.Upper, RailingSide.Left, HandrailPositionZ.Upper)
                or (RailingConnectionType.AngleHorizont, RailingPositionZ.Lower, RailingSide.Left, HandrailPositionZ.Upper)
                or (RailingConnectionType.HorizontHorizont, _, RailingSide.Left, HandrailPositionZ.Upper):
                    Ns2 = [6, 7, 8, 3, 4, 5];
                    break;

                case (RailingConnectionType.HorizontAngle, RailingPositionZ.Upper, RailingSide.Right, HandrailPositionZ.Upper)
                or (RailingConnectionType.AngleHorizont, RailingPositionZ.Lower, RailingSide.Right, HandrailPositionZ.Upper)
                or (RailingConnectionType.HorizontHorizont, _, RailingSide.Right, HandrailPositionZ.Upper):
                    Ns2 = [13, 14, 15, 10, 11, 12];
                    break;

                case (_, _, RailingSide.Left, HandrailPositionZ.Upper):
                    Ns2 = [3, 4, 5, 6, 7, 8];
                    break;

                case (_, _, RailingSide.Right, HandrailPositionZ.Upper):
                    Ns2 = [10, 11, 12, 13, 14, 15];
                    break;


                case (RailingConnectionType.HorizontAngle, RailingPositionZ.Upper, RailingSide.Left, HandrailPositionZ.Lower)
                or (RailingConnectionType.AngleHorizont, RailingPositionZ.Lower, RailingSide.Left, HandrailPositionZ.Lower)
                or (RailingConnectionType.HorizontHorizont, _, RailingSide.Left, HandrailPositionZ.Lower):
                    Ns2 = [43, 44, 45, 40, 41, 42];
                    break;

                case (RailingConnectionType.HorizontAngle, RailingPositionZ.Upper, RailingSide.Right, HandrailPositionZ.Lower)
                or (RailingConnectionType.AngleHorizont, RailingPositionZ.Lower, RailingSide.Right, HandrailPositionZ.Lower)
                or (RailingConnectionType.HorizontHorizont, _, RailingSide.Right, HandrailPositionZ.Lower):
                    Ns2 = [13, 14, 15, 10, 11, 12];
                    break;

                case (_, _, RailingSide.Left, HandrailPositionZ.Lower):
                    Ns2 = [40, 41, 42, 43, 44, 45];
                    break;

                case (_, _, RailingSide.Right, HandrailPositionZ.Lower):
                    Ns2 = [47, 48, 49, 50, 51, 52];
                    break;
            }

            EndAngleIPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns2[0]]);
            EndAngleOPPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns2[1]]);
            EndLengthPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][Ns2[2]]);
            EndOtherPars = [.. new List<int> { Ns2[3], Ns2[4], Ns2[5] }
                    .Select(i => _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][i]))];

            switch (side)
            {
                case RailingSide.Left when _railing.Symbol.FamilyName == StairsRailing1:
                    EndOtherPars.Add(_railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][31]));
                    break;
                case RailingSide.Left when _railing.Symbol.FamilyName == StairsRailing2_1 || _railing.Symbol.FamilyName == StairsRailing2_2 || _railing.Symbol.FamilyName == StairsRailing2_3
                                            || _railing.Symbol.FamilyName == StairsRailing3:
                    EndNotInRailingPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][33]);
                    break;
                case RailingSide.Right when _railing.Symbol.FamilyName == StairsRailing1:
                    EndOtherPars.Add(_railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][32]));
                    break;
                case RailingSide.Right when _railing.Symbol.FamilyName == StairsRailing2_1 || _railing.Symbol.FamilyName == StairsRailing2_2 || _railing.Symbol.FamilyName == StairsRailing2_3
                                            || _railing.Symbol.FamilyName == StairsRailing3:
                    EndNotInRailingPar = _railing.LookupParameter(FamilyParameterNames.Railings[_railing.Symbol.FamilyName][34]);
                    break;
            }
        }
    }
}
