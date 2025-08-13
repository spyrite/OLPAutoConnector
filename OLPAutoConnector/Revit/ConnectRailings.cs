using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using OLP.AutoConnector.Models;
using OLP.AutoConnector.Resources;
using OLP.AutoConnector.ViewModels;
using OLP.AutoConnector.Views;
using System.Collections.Generic;
using System.Linq;
using OLP.AutoConnector.Customs;
using Prism.Dialogs;
using System;

namespace OLP.AutoConnector.Revit
{
    [Transaction (TransactionMode.Manual)]
    public class ConnectRailings : IExternalCommand
    {
        public static UIApplication UIApp;
        public static UIDocument UIDoc;
        public static Autodesk.Revit.ApplicationServices.Application App;
        public static Document Doc;

        private FamilyInstance _primaryRailing;
        private FamilyInstance _secondaryRailing;
        private RailingData _upperRailingData;
        private RailingData _lowerRailingData;


        private Dictionary<int, FailureModel> _failureModels;
        private List<string> _supportedFamilyNames;

        private double _minRailingsDistanceY;
        private double _railingsDistanceY;

        private Plane _connectionPlane;
        private RailingData _railingOnStartPointData;

        private double _railingsEndAngleIP;
        private double _railingsEndAngleOP;
        private double _railingsEndLength;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApp = commandData.Application;
            UIDoc = commandData.Application.ActiveUIDocument;
            App = commandData.Application.Application;
            Doc = commandData.Application.ActiveUIDocument.Document;
            _failureModels = [];
            _supportedFamilyNames = [.. FamilyParameterNames.Railings.Keys];

            try
            {
                _primaryRailing = Doc.GetElement(UIDoc.Selection.PickObject(ObjectType.Element, new RailingSelectionFilter(),
                "Выберите первый экземпляр поддерживаемого семейства ограждения")) as FamilyInstance;
            }
            catch { return Result.Cancelled; }

            try
            {
                _secondaryRailing = Doc.GetElement(UIDoc.Selection.PickObject(ObjectType.Element, new RailingSelectionFilter(),
                "Выберите второй экземпляр поддерживаемого семейства ограждения")) as FamilyInstance;
            }
            catch { return Result.Cancelled; }

            //Проверки выбранных ограждений

            //Поддерживаемость семейств
            if (!_supportedFamilyNames.Contains(_primaryRailing.Symbol.FamilyName)) AddFailureId(0, _primaryRailing.Id);
            if (!_supportedFamilyNames.Contains(_secondaryRailing.Symbol.FamilyName)) AddFailureId(0, _secondaryRailing.Id);

            //Параллельность ограждений
            if (!XYZExtensions.ABS(_primaryRailing.HandOrientation).IsAlmostEqualTo(XYZExtensions.ABS(_secondaryRailing.HandOrientation)))
            {
                AddFailureId(1, _primaryRailing.Id);
                AddFailureId(1, _secondaryRailing.Id);
            }

            //Минимальное межосевое расстояние между ограждениями
            _minRailingsDistanceY = _primaryRailing.Symbol.LookupParameter(FamilyParameterNames.Railings[_primaryRailing.Symbol.FamilyName][16]).AsDouble()
                + _secondaryRailing.Symbol.LookupParameter(FamilyParameterNames.Railings[_secondaryRailing.Symbol.FamilyName][16]).AsDouble();

            Plane.CreateByOriginAndBasis((_primaryRailing.Location as LocationPoint).Point, _primaryRailing.HandOrientation, XYZ.BasisZ)
            .Project((_secondaryRailing.Location as LocationPoint).Point, out _, out _railingsDistanceY);

            if (_railingsDistanceY < _minRailingsDistanceY)
            {
                AddFailureId(2, _primaryRailing.Id);
                AddFailureId(2, _secondaryRailing.Id);
            }

            //Диаметры поручней
            if (_primaryRailing.Symbol.LookupParameter(FamilyParameterNames.Railings[_primaryRailing.Symbol.FamilyName][17]).AsDouble()
                != _secondaryRailing.Symbol.LookupParameter(FamilyParameterNames.Railings[_secondaryRailing.Symbol.FamilyName][17]).AsDouble())
            {
                AddFailureId(3, _primaryRailing.Id);
                AddFailureId(3, _secondaryRailing.Id);
            }

            if (!_failureModels.Any())
            {
                if (new InputDataView(new InputDataVM()).ShowDialog() == false) return Result.Cancelled;

                switch ((_primaryRailing.Location as LocationPoint).Point.Z > (_secondaryRailing.Location as LocationPoint).Point.Z)
                {
                    case true:
                        _upperRailingData = new(_primaryRailing);
                        _lowerRailingData = new(_secondaryRailing);
                        break;
                    case false:
                        _upperRailingData = new(_secondaryRailing);
                        _lowerRailingData = new(_primaryRailing);
                        break;
                }



                RailingData.ConnectAlignX = Properties.InputData.Default.RailingConnectionAlignX;
                ExtendRailingsData();



                //RailingEndData.ConnectAngle = GetRailingsConnectAngle();

            }




            if (_failureModels.Any())
            {
                new FailuresView(new FailuresVM(Doc, [.. _failureModels.Values])).Show();
            }

            return Result.Succeeded;
        }






        private void ExtendRailingsData()
        {
            switch (_upperRailingData.FamilyName)
            {
                case string familyName when familyName == SupportedFamilyNames.StairsRailing1:
                    _connectionPlane = Plane.Create(new Frame(_upperRailingData.GetEdgeRailingSupportOrigin(RailingSide.Left)
                        - RailingData.ConnectAlignX * _upperRailingData.DirX, _upperRailingData.DirY, _upperRailingData.DirZ, -_upperRailingData.DirX));

                    _upperRailingData.ExtendData(RailingSide.Left);
                    break;
                case string familyName when familyName == SupportedFamilyNames.StairsRailing2:


                    _connectionPlane = Plane.Create(new Frame(_upperRailingData.GetEdgeRailingSupportOrigin(RailingSide.Right)
                        - RailingData.ConnectAlignX * _upperRailingData.DirX, _upperRailingData.DirY, _upperRailingData.DirZ, _upperRailingData.DirX));

                    _upperRailingData.ExtendData(RailingSide.Right);
                    break;
            }
            switch (_lowerRailingData.FamilyName)
            {
                case string familyName when familyName == SupportedFamilyNames.StairsRailing1:
                    _lowerRailingData.ExtendData(RailingSide.Right);
                    break;
                case string familyName when familyName == SupportedFamilyNames.StairsRailing2:
                    _lowerRailingData.ExtendData(RailingSide.Left);
                    break;
            } 
        }

        private void GetConnectionAngle()
        {
            _connectionPlane.Project(_lowerRailingData.HandrailOrigin, out UV uv, out double dst);
            XYZ p0 = _connectionPlane.Origin + uv.U * _connectionPlane.XVec + uv.V * _connectionPlane.YVec + dst * Math.Atan(_lowerRailingData.StairsAngle) * XYZ.BasisZ;

            _connectionPlane.Project(_upperRailingData.HandrailOrigin, out uv, out dst);
            XYZ p1 = _connectionPlane.Origin + uv.U * _connectionPlane.XVec + uv.V * _connectionPlane.YVec - dst * Math.Atan(_upperRailingData.StairsAngle) * XYZ.BasisZ;

            

            return Line.CreateBound(p0, p1).Direction.AngleOnPlaneTo();
        }

        

        private void AddFailureId(int failureKey, ElementId failureId)
        {
            if (!_failureModels.ContainsKey(failureKey))
                switch (failureKey)
                {
                    case 0:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.RailingFamilyNotSupported);
                        break;
                    case 1:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.RailingsNotParallel);
                        break;
                    case 2:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.RailingsTooClose);
                        break;
                    case 3:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.HandrailDiametersAreDifferent);
                        break;
                }

            _failureModels[failureKey].Ids.Add(failureId);
        }
    }
}

    
