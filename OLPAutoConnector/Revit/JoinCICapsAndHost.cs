using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OLP.AutoConnector.Models;
using OLP.AutoConnector.Resources;
using OLP.AutoConnector.ViewModels;
using OLP.AutoConnector.Views;
using OLP.AutoConnector.Customs;
using System.Collections.Generic;
using System.Linq;

using static OLP.AutoConnector.Resources.StructuralFilters;

namespace OLP.AutoConnector.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class JoinCICapsAndHost : IExternalCommand
    {
        public static UIApplication UIApp;
        public static UIDocument UIDoc;
        public static Autodesk.Revit.ApplicationServices.Application App;
        public static Document Doc;

        private ActionsVM.NextAction _selectedNextAction;
        private List<ElementId> _selectedElemIds;
        private FilteredElementCollector _ciCollector;
        private List<FamilyInstance> _targetCIs;

        private Dictionary<int, FailureModel> _failureModels;
        private ActionsView _actionsView;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            //»нициализаци€
            UIApp = commandData.Application;
            UIDoc = commandData.Application.ActiveUIDocument;
            App = commandData.Application.Application;
            Doc = commandData.Application.ActiveUIDocument.Document;
            _failureModels = [];

            _selectedElemIds = [.. UIDoc.Selection.GetElementIds()];

            if (_selectedElemIds.Any())
                _ciCollector = new FilteredElementCollector(Doc, _selectedElemIds).OfClass(typeof(FamilyInstance));

            //ƒиалоговое окно с выбором дальнейшего действи€
            else
            {
                _actionsView = new(new ActionsVM(ActionsVM.TargetElementKind.CIs, 0));
                _selectedNextAction = _actionsView.ShowNextActionsDialog();

                switch (_selectedNextAction)
                {
                    case ActionsVM.NextAction.Cancel:
                        return Result.Cancelled;
                    case ActionsVM.NextAction.SelectAllInModel:
                        _ciCollector = new FilteredElementCollector(Doc).OfClass(typeof(FamilyInstance));
                        break;
                    case ActionsVM.NextAction.SelectAllOnActiveView:
                        _ciCollector = new FilteredElementCollector(Doc, UIDoc.ActiveView.Id).OfClass(typeof(FamilyInstance));
                        break;
                    case ActionsVM.NextAction.AllowUserSelection:
                        try
                        {
                            _selectedElemIds = [.. UIDoc.Selection.PickObjects(Autodesk.Revit.UI.Selection.ObjectType.Element, new CIsSelectionFilter(),
                            "¬ыберите закладные детали").Select(r => r.ElementId)];
                            _ciCollector = new FilteredElementCollector(Doc, _selectedElemIds).OfClass(typeof(FamilyInstance));
                        }
                        catch { }
                        break;
                }
;           }

            //ѕоиск закладных деталей в коллекторе
            _targetCIs = _ciCollector != null ? [.. _ciCollector.Cast<FamilyInstance>()
                .Select(inst => inst.GetHigherSuperComponent())
                .Where(inst => inst.Symbol.FamilyName.Contains(ConcreteInsertFamilyNameKey))] : [];

            if (!_targetCIs.Any())
            {
                _actionsView = new(new ActionsVM(ActionsVM.TargetElementKind.CIs, -1));
                _actionsView.ShowNextActionsDialog();
                return Result.Cancelled;
            }

            else if (Properties.Actions.Default.AllowShowCountDialog == true)
            {
                _actionsView = new(new ActionsVM(ActionsVM.TargetElementKind.CIs, _targetCIs.Count));
                _selectedNextAction = _actionsView.ShowNextActionsDialog();
                if (_selectedNextAction == ActionsVM.NextAction.Cancel) return Result.Cancelled;
            }
                

            using (Transaction tx = new(Doc, "OLP: —оединение бетонных заглушек с основой"))
                {
                    FailureHandlingOptions failureHandlingOptions = tx.GetFailureHandlingOptions();
                    failureHandlingOptions.SetForcedModalHandling(false);
                    failureHandlingOptions.SetDelayedMiniWarnings(false);
                    failureHandlingOptions.SetFailuresPreprocessor(new SupressWarnings());
                    tx.SetFailureHandlingOptions(failureHandlingOptions);

                    tx.Start();

                    foreach (FamilyInstance ci in _targetCIs)
                    {
                        //ѕроверка наличи€ хоста у закладной детали
                        if (!HostFilter.PassesFilter(ci.Host)) AddFailureId(0, ci.Id);

                        //ѕоиск бетонных заглушек в закладных детал€х
                        List<FamilyInstance> concreteCaps = ci.GetSubComponentIds().Select(id => Doc.GetElement(id) as FamilyInstance)
                            .ToList().FindAll(inst => inst.Symbol.FamilyName.Contains(ConcreteCapFamilyNameKey));
                        if (!concreteCaps.Any()) AddFailureId(1, ci.Id);

                        //ѕроверка наличи€ параметра материала дл€ бетонных заглушек
                        Parameter concreteCapMaterialParameter = ci.LookupParameter(ConcreteCapMaterialParameterName);
                        if (concreteCapMaterialParameter == null) AddFailureId(2, ci.Id);

                        //ѕроверка "бетонности" материала хоста
                        ElementId materialId = ci.Host.GetMaterialIds(false).ToList()
                            .Find(id => ConcreteMaterailKeys.Any(key => Doc.GetElement(id).Name.Contains(key)));
                        if (materialId == null) AddFailureId(3, ci.Id);

                        //ќпераци€ "ѕрисоединить элементы геометрии" с проверкой на возможность выполнени€. Ќе выполн€етс€ если элементы уже соединены.
                        using (SubTransaction subTx = new(Doc))
                        {
                            subTx.Start();

                            foreach (FamilyInstance concreteCap in concreteCaps)
                            {
                                if (!JoinGeometryUtils.AreElementsJoined(Doc, ci.Host, concreteCap))
                                {
                                    try { JoinGeometryUtils.JoinGeometry(Doc, ci.Host, concreteCap); }
                                    catch { AddFailureId(4, ci.Id); subTx.RollBack(); break; }
                                }
                            }

                            if (!subTx.HasEnded()) subTx.Commit();
                        }

                        //ќбработка закладной детали пропускаетс€, если хот€ бы одна из проверок не была пройдена
                        if (_failureModels.Any(fm => fm.Value.Ids.Contains(ci.Id))) continue;

                        //Ќазначение материала основы бетонной заглушке
                        concreteCapMaterialParameter.Set(materialId);
                    }

                    tx.Commit();
                }

            if (_failureModels.Any())
            {
                new FailuresView(new FailuresVM(Doc, [.. _failureModels.Values])).Show();
            }

            return Result.Succeeded;
        }

        private void AddFailureId(int failureKey, ElementId failureId)
        {
            if (!_failureModels.ContainsKey(failureKey))
                switch (failureKey)
                {
                    case 0:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.HostNotFound);
                        break;
                    case 1:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.ConcreteCapsNotFound + $" {ConcreteInsertFamilyNameKey}");
                        break;
                    case 2:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.ConcreteCapMaterialParameterNotFound + $" \"{ConcreteCapMaterialParameterName}\"");
                        break;
                    case 3:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.HostMaterialIsNotConcrete + $" {string.Join(", ", ConcreteMaterailKeys)}");
                        break;
                    case 4:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.CannotJoinCICapsAndHost);
                        break;
                    case 5:
                        _failureModels[failureKey] = new FailureModel(UIDoc, FailureMessages.UnknowFailure);
                        break;
                }

            _failureModels[failureKey].Ids.Add(failureId);
        }
    }
}
