using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using OLP.AutoConnector.Customs;
using OLP.AutoConnector.Models;
using OLP.AutoConnector.Resources;
using OLP.AutoConnector.ViewModels;
using OLP.AutoConnector.Views;
using System;
using System.Collections.Generic;
using System.Linq;

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
                _secondaryRailing = Doc.GetElement(UIDoc.Selection.PickObject(ObjectType.Element, new RailingSelectionFilter([ _primaryRailing.Id ]), 
                "Выберите второй экземпляр поддерживаемого семейства ограждения")) as FamilyInstance;
            }
            catch { return Result.Cancelled; }

            //Проверка на поддерживаемость семейств
            if (!_supportedFamilyNames.Contains(_primaryRailing.Symbol.FamilyName)) AddFailureId(0, _primaryRailing.Id);
            if (!_supportedFamilyNames.Contains(_secondaryRailing.Symbol.FamilyName)) AddFailureId(0, _secondaryRailing.Id);

            if (!_failureModels.Any())
            {
                //Ввод исходных данных
                if (new InputDataView(new InputDataVM()).ShowDialog() == false) return Result.Cancelled;
                RailingData.ConnectionType = (RailingConnectionType)Properties.InputData.Default.RailingsConnectionType;
                RailingData.ConnectXFromEdgeSupport = Properties.InputData.Default.UpperRailingConnectionX;

                //Определение вышестоящего и нижестоящего ограждений
                switch ((_primaryRailing.Location as LocationPoint).Point.Z > (_secondaryRailing.Location as LocationPoint).Point.Z)
                {
                    case true:
                        _upperRailingData = new(_primaryRailing) { RailingPositionZ = RailingPositionZ.Upper, 
                            ConnectDZFromHandrailTop = Properties.InputData.Default.UpperRailingConnectionDZ };
                        _lowerRailingData = new(_secondaryRailing) { RailingPositionZ = RailingPositionZ.Lower,
                            ConnectDZFromHandrailTop = Properties.InputData.Default.LowerRailingConnectionDZ };
                        break;

                    case false:
                        _upperRailingData = new(_secondaryRailing) { RailingPositionZ = RailingPositionZ.Upper,
                            ConnectDZFromHandrailTop = Properties.InputData.Default.UpperRailingConnectionDZ };
                        _lowerRailingData = new(_primaryRailing) { RailingPositionZ = RailingPositionZ.Lower,
                            ConnectDZFromHandrailTop = Properties.InputData.Default.LowerRailingConnectionDZ };
                        break;
                }

                //Проверки геометрии выбранных ограждений
                //Параллельность ограждений
                if (!XYZExtensions.ABS(_upperRailingData.DirX).IsAlmostEqualTo(XYZExtensions.ABS(_lowerRailingData.DirX)))
                {
                    AddFailureId(1, _upperRailingData.Id);
                    AddFailureId(1, _lowerRailingData.Id);
                }

                //Минимальное межосевое расстояние между ограждениями
                RailingData.MinRailingsDistanceY = _upperRailingData.EndAxisRadius + _lowerRailingData.EndAxisRadius;
                //Фактическое межосевое расстояние между ограждениями
                Plane.CreateByOriginAndBasis(_upperRailingData.HandrailOrigin, _upperRailingData.DirX, _upperRailingData.DirZ)
                .Project(_lowerRailingData.HandrailOrigin, out _, out RailingData.RailingsDistanceY);

                if (RailingData.RailingsDistanceY < RailingData.MinRailingsDistanceY)
                {
                    AddFailureId(2, _upperRailingData.Id);
                    AddFailureId(2, _lowerRailingData.Id);
                }

                //Диаметры поручней
                if (_upperRailingData.HandrailDiameter != _lowerRailingData.HandrailDiameter)
                {
                    AddFailureId(3, _upperRailingData.Id);
                    AddFailureId(3, _lowerRailingData.Id);
                }

                if (!_failureModels.Any())
                {
                    ExtendRailingsData();
                    RailingData.ConnectAngle = GetConnectionAngle(out RailingData.ConnectAxisDir);

                    //Вычисления значений заполняемых параметров
                    _upperRailingData.CalculateData();
                    _lowerRailingData.CalculateData();

                    using (Transaction tx = new(Doc, "OLP: Автосоединение поручней ограждений"))
                    {
                        FailureHandlingOptions failureHandlingOptions = tx.GetFailureHandlingOptions();
                        failureHandlingOptions.SetForcedModalHandling(false);
                        failureHandlingOptions.SetDelayedMiniWarnings(false);
                        failureHandlingOptions.SetFailuresPreprocessor(new SupressWarnings());
                        tx.SetFailureHandlingOptions(failureHandlingOptions);

                        tx.Start();

                        _upperRailingData.HandrailAngleExtendPar.Set(_upperRailingData.HandrailAngleExtend);
                        _upperRailingData.EndTypePar?.Set(_upperRailingData.EndTypeId);
                        _upperRailingData.EndIsEnabledPar.Set(1);
                        _upperRailingData.EndAngleIPPar.Set(_upperRailingData.EndAngleIP);
                        _upperRailingData.EndAngleOPPar.Set(_upperRailingData.EndAngleOP);
                        _upperRailingData.EndLengthPar.Set(_upperRailingData.EndLength);
                        _upperRailingData.EndCapIsEnabledPar.Set(0);

                        _lowerRailingData.HandrailAngleExtendPar.Set(_lowerRailingData.HandrailAngleExtend);
                        _lowerRailingData.EndTypePar?.Set(_upperRailingData.EndTypeId);
                        _lowerRailingData.EndIsEnabledPar.Set(1);
                        _lowerRailingData.EndAngleIPPar.Set(_lowerRailingData.EndAngleIP);
                        _lowerRailingData.EndAngleOPPar.Set(_lowerRailingData.EndAngleOP);
                        _lowerRailingData.EndLengthPar.Set(_lowerRailingData.EndLength);
                        _lowerRailingData.EndCapIsEnabledPar.Set(0);

                        switch (RailingData.ConnectionType)
                        {
                            case RailingConnectionType.AngleAngle:
                                foreach (Parameter par in _upperRailingData.EndOtherPars) par.Set(0);
                                foreach (Parameter par in _lowerRailingData.EndOtherPars) par.Set(0);
                                break;

                            case RailingConnectionType.HorizontHorizont:
                                _upperRailingData.HandrailHorizontExtendPar.Set(_upperRailingData.HandrailHorizontExtend);
                                _upperRailingData.HandrailAngleIPPar.Set(_upperRailingData.HandrailAngleIP);
                                _upperRailingData.HandrailAngleOPPar.Set(_upperRailingData.HandrailAngleOP);
                                _lowerRailingData.HandrailHorizontExtendPar.Set(_lowerRailingData.HandrailHorizontExtend);
                                _lowerRailingData.HandrailAngleIPPar.Set(_lowerRailingData.HandrailAngleIP);
                                _lowerRailingData.HandrailAngleOPPar.Set(_lowerRailingData.HandrailAngleOP);
                                break;

                            case RailingConnectionType.AngleHorizont:
                                foreach (Parameter par in _upperRailingData.EndOtherPars) par.Set(0);
                                _lowerRailingData.HandrailHorizontExtendPar.Set(_lowerRailingData.HandrailHorizontExtend);
                                _lowerRailingData.HandrailAngleIPPar.Set(_lowerRailingData.HandrailAngleIP);
                                _lowerRailingData.HandrailAngleOPPar.Set(_lowerRailingData.HandrailAngleOP);
                                break;

                            case RailingConnectionType.HorizontAngle:
                                _upperRailingData.HandrailHorizontExtendPar.Set(_upperRailingData.HandrailHorizontExtend);
                                _upperRailingData.HandrailAngleIPPar.Set(_upperRailingData.HandrailAngleIP);
                                _upperRailingData.HandrailAngleOPPar.Set(_upperRailingData.HandrailAngleOP);
                                foreach (Parameter par in _lowerRailingData.EndOtherPars) par.Set(0);
                                break;
                        }

                        tx.Commit();
                    }
                }


            }

            if (_failureModels.Any())
            {
                new FailuresView(new FailuresVM(Doc, [.. _failureModels.Values])).Show();
            }

            return Result.Succeeded;
        }


        //Метод определеляет плоскости стыка,
        //расстояние от плоскости стыка до начальной/конечной опорной плоскости ограждния,
        //производит выбор заполняемых параметров для ограждений
        private void ExtendRailingsData()
        {
            _upperRailingData.SetConnectionXOYPlane();
            _lowerRailingData.SetConnectionXOYPlane();

            switch (_upperRailingData.FamilyName)
            {
                case string familyName when familyName == SupportedFamilyNames.StairsRailing1:
                    RailingData.ConnectionYOZPlane = Plane.Create(new Frame(_upperRailingData.GetEdgeRailingSupportOrigin(RailingSide.Left)
                        - RailingData.ConnectXFromEdgeSupport * _upperRailingData.DirX,
                        _upperRailingData.DirY, _upperRailingData.DirZ, (_upperRailingData.Mirrored ? 1 : -1) * _upperRailingData.DirX));

                    RailingData.ConnectionYOZPlane.Project(_upperRailingData.Origin, out _, out _upperRailingData.ConnectXFromRefPlane);

                    _upperRailingData.InitilizeParameters(RailingSide.Left);
                    break;

                case string familyName when familyName == SupportedFamilyNames.StairsRailing2:
                    RailingData.ConnectionYOZPlane = Plane.Create(new Frame(_upperRailingData.GetEdgeRailingSupportOrigin(RailingSide.Right)
                        + RailingData.ConnectXFromEdgeSupport * _upperRailingData.DirX,
                        _upperRailingData.DirY, _upperRailingData.DirZ, (_upperRailingData.Mirrored ? 1 : -1) * _upperRailingData.DirX));

                    RailingData.ConnectionYOZPlane.Project(_upperRailingData.Origin + _upperRailingData.StartEndRefDistance * _upperRailingData.DirX
                        , out _, out _upperRailingData.ConnectXFromRefPlane);

                    _upperRailingData.InitilizeParameters(RailingSide.Right);
                    break;

                case string familyName when familyName == SupportedFamilyNames.StairsRailing3:
                    RailingData.ConnectionYOZPlane = Plane.Create(new Frame(_upperRailingData.GetEdgeRailingSupportOrigin(RailingSide.Right)
                        + RailingData.ConnectXFromEdgeSupport * _upperRailingData.DirX,
                        _upperRailingData.DirY, _upperRailingData.DirZ, (_upperRailingData.Mirrored ? 1 : -1) * _upperRailingData.DirX));

                    RailingData.ConnectionYOZPlane.Project(_upperRailingData.Origin + _upperRailingData.StartEndRefDistance * _upperRailingData.DirX
    ,                   out _, out _upperRailingData.ConnectXFromRefPlane);

                    _upperRailingData.InitilizeParameters(RailingSide.Right);
                    break;
            }

            switch (_lowerRailingData.FamilyName)
            {
                case string familyName when familyName == SupportedFamilyNames.StairsRailing1:
                    RailingData.ConnectionYOZPlane.Project(_lowerRailingData.Origin + _lowerRailingData.StartEndRefDistance * _lowerRailingData.DirX
                        , out _, out _lowerRailingData.ConnectXFromRefPlane);

                    _lowerRailingData.InitilizeParameters(RailingSide.Right);

                    break;

                case string familyName when familyName == SupportedFamilyNames.StairsRailing2:
                    RailingData.ConnectionYOZPlane.Project(_lowerRailingData.Origin, out _, out _lowerRailingData.ConnectXFromRefPlane);

                    _lowerRailingData.InitilizeParameters(RailingSide.Left);
                    break;

                case string familyName when familyName == SupportedFamilyNames.StairsRailing3:
                    RailingData.ConnectionYOZPlane.Project(_lowerRailingData.Origin, out _, out _lowerRailingData.ConnectXFromRefPlane);

                    _lowerRailingData.InitilizeParameters(RailingSide.Left);
                    break;
            } 
        }

        //Метод определяет угол наклона оси стыка ограждений
        private double GetConnectionAngle(out XYZ connectionAxisDir)
        {
            XYZ p0 = _lowerRailingData.HandrailOrigin.ProjectOnPlane(RailingData.ConnectionYOZPlane, _lowerRailingData.HandrailAngle, _lowerRailingData.DirZ);
            XYZ p1 = _upperRailingData.HandrailOrigin.ProjectOnPlane(RailingData.ConnectionYOZPlane, _upperRailingData.HandrailAngle, -_upperRailingData.DirZ);
            connectionAxisDir = Line.CreateBound(p0, p1).Direction;

            //using (Transaction tx = new(Doc, "OLP test")) { tx.Start(); Doc.Create.NewModelCurve(Line.CreateBound(p0, p1), SketchPlane.Create(Doc, RailingData.ConnectionYOZPlane)); tx.Commit(); }

            return connectionAxisDir.AngleOnPlaneTo(connectionAxisDir.Multiply2(RailingData.ConnectionYOZPlane.XVec.ABS()), RailingData.ConnectionYOZPlane.Normal);
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
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.RailingsTooClose + $"{Math.Round(RailingData.MinRailingsDistanceY*304.8)} мм");
                        break;
                    case 3:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.HandrailDiametersAreDifferent);
                        break;
                    case 4:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.HandrailEndCapIsEnabled);
                        break;
                }

            _failureModels[failureKey].Ids.Add(failureId);
        }
    }
}

    
