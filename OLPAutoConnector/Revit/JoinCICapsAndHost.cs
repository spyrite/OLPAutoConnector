using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using OLP.AutoConnector.ViewModels;
using OLP.AutoConnector.Views;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OLP.AutoConnector.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class JoinCICapsAndHost : IExternalCommand
    {
        private static readonly ElementFilter _hostFilter = new LogicalOrFilter(
            [ new ElementCategoryFilter(BuiltInCategory.OST_Walls)
            , new ElementCategoryFilter(BuiltInCategory.OST_Floors)
            , new ElementCategoryFilter(BuiltInCategory.OST_StructuralColumns)
            , new ElementCategoryFilter(BuiltInCategory.OST_StructuralFraming)
            , new ElementCategoryFilter(BuiltInCategory.OST_StructuralFoundation)]);
        private static readonly List<string> _concreteMaterailKeys = ["Бетон", "Железобетон"];

        //private static Guid _materialParameterGuid = new Guid("8b5e61a2-b091-491c-8092-0b01a55d4f44");
        private static readonly string _concreteInsertFamilyNameKey = "220_Закладная";
        private static readonly string _concreteCapFamilyNameKey = "220_Бетонная заглушка";
        private static readonly string _concreteCapMaterialParameterName = "Материал_Бетонная заглушка";


        public static UIApplication UIApp;
        public static UIDocument UIDoc;
        public static Autodesk.Revit.ApplicationServices.Application App;
        public static Document Doc;

        private static ActionsVM.NextAction _selectedNextAction;
        private List<ElementId> _selectedElemIds;
        private FilteredElementCollector _ciCollector;
        private List<FamilyInstance> _targetCIs;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elementSet)
        {
            UIApp = commandData.Application;
            UIDoc = commandData.Application.ActiveUIDocument;
            App = commandData.Application.Application;
            Doc = commandData.Application.ActiveUIDocument.Document;

            _selectedElemIds = [.. UIDoc.Selection.GetElementIds()];

            if (_selectedElemIds.Any())
                _ciCollector = new FilteredElementCollector(Doc, _selectedElemIds).OfClass(typeof(FamilyInstance));
            else
            {
                ActionsView actionsView = new(new ActionsVM(ActionsVM.TargetElementKind.CIs, 0));
                _selectedNextAction = actionsView.ShowNextActionsDialog();

                switch ((actionsView.DataContext as ActionsVM).SelectedNextAction)
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
                        break;
                }
;           }

            //Поиск закладных деталей в коллекторе
            _targetCIs = [.. _ciCollector.Cast<FamilyInstance>().Where(inst => inst.Symbol.FamilyName.Contains(_concreteInsertFamilyNameKey))];

            if (!_targetCIs.Any())
            {
                message = "Не выбрано ни одной закладной детали. Команда отменена.";
                return Result.Cancelled;
            }

            using (Transaction tx = new(Doc, "OLP: Соединение бетонных заглушек с основой"))
            {
                FailureHandlingOptions failureHandlingOptions = tx.GetFailureHandlingOptions();
                failureHandlingOptions.SetForcedModalHandling(false);
                failureHandlingOptions.SetDelayedMiniWarnings(false);
                failureHandlingOptions.SetFailuresPreprocessor(new SupressWarnings());
                tx.SetFailureHandlingOptions(failureHandlingOptions);

                tx.Start();

                foreach (FamilyInstance ci in _targetCIs)
                {
                    List<FamilyInstance> concreteCaps = ci.GetSubComponentIds().Select(id => Doc.GetElement(id) as FamilyInstance)
                        .ToList().FindAll(inst => inst.Symbol.FamilyName.Contains(_concreteCapFamilyNameKey));

                    if (_hostFilter.PassesFilter(ci.Host) && concreteCaps.Any())
                    {
                        //Операция "Присоединить элементы геометрии", не выполняется если элементы уже соединены.
                        foreach (FamilyInstance concreteCap in concreteCaps) 
                            if (!JoinGeometryUtils.AreElementsJoined(Doc, ci.Host, concreteCap)) JoinGeometryUtils.JoinGeometry(Doc, ci.Host, concreteCap);

                        //Назначение материала основы бетонной заглушке, выполняется только если материал определяется как "Бетон" 
                        ElementId materialId = ci.Host.GetMaterialIds(false).ToList()
                            .Find(id => _concreteMaterailKeys.Any(key => Doc.GetElement(id).Name.Contains(key)));

                        if (materialId != null) ci.LookupParameter(_concreteCapMaterialParameterName)?.Set(materialId);
                    }
                }

                tx.Commit();
            }

            return Result.Succeeded;
        }


        /*
        private ElementId GetHostMaterialId(Element host)
        {
            ElementId materialId = ElementId.InvalidElementId;
            

            switch((BuiltInCategory)host.Category.Id.IntegerValue)
            {
                case BuiltInCategory.OST_Walls:
                    materialId = host is Wall ? GetConcreteMaterialIdFromLayers((host as Wall).WallType)
                        : GetConcreteMaterialIdFromParameter(host);
                    break;

                case BuiltInCategory.OST_Floors:
                    materialId = host is Floor ? GetConcreteMaterialIdFromLayers((host as Floor).FloorType)
                        : GetConcreteMaterialIdFromParameter(host);
                    break;

                case BuiltInCategory.OST_StructuralColumns:
                    materialId = GetConcreteMaterialIdFromParameter(host);
                    break;

                case BuiltInCategory.OST_StructuralFraming:
                    materialId = GetConcreteMaterialIdFromParameter(host);
                    break;

                case BuiltInCategory.OST_StructuralFoundation:
                    materialId = host is Floor ? GetConcreteMaterialIdFromLayers((host as Floor).FloorType)
                        : GetConcreteMaterialIdFromParameter(host);
                    break;
            }

            return materialId;
        }

        private ElementId GetConcreteMaterialIdFromLayers(HostObjAttributes hostType)
        {
            ElementId materialId = ElementId.InvalidElementId;
            List<Material> materials = hostType.GetCompoundStructure()?
                            .GetLayers().Select(layer => Doc.GetElement(layer.MaterialId) as Material)
                            .ToList();
            if (materials.Any())
                materialId = materials.Find(mat => mat != null && _concreteMaterailKeys.Any(key => mat.Name.Contains(key)))?.Id;
            return materialId ?? ElementId.InvalidElementId;
        }

        private ElementId GetConcreteMaterialIdFromParameter(Element host)
        {           
            ElementType hostType = Doc.GetElement(host.GetTypeId()) as ElementType;
            ElementId materialId = hostType.get_Parameter(_materialParameterGuid)?.AsElementId();
            if (materialId == null || materialId == ElementId.InvalidElementId) 
                materialId = hostType.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM)?.AsElementId();
            if (materialId == null || materialId == ElementId.InvalidElementId) 
                materialId = host.get_Parameter(_materialParameterGuid)?.AsElementId();
            if (materialId == null || materialId == ElementId.InvalidElementId) 
                materialId = host.get_Parameter(BuiltInParameter.STRUCTURAL_MATERIAL_PARAM)?.AsElementId();

            return materialId ?? ElementId.InvalidElementId;
        }
        */
    }
}
