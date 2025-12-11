using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using OLP.AutoConnector.Customs;
using OLP.AutoConnector.Resources;
using System.Collections.Generic;
using System.Linq;

using static OLP.AutoConnector.Resources.SupportedFamilyNames;

namespace OLP.AutoConnector.Revit
{
    [Transaction(TransactionMode.Manual)]
    public class DisconnectRailings : IExternalCommand
    {
        public static UIApplication UIApp;
        public static UIDocument UIDoc;
        public static Autodesk.Revit.ApplicationServices.Application App;
        public static Document Doc;

        private List<string> _supportedFamilyNames;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIApp = commandData.Application;
            UIDoc = commandData.Application.ActiveUIDocument;
            App = commandData.Application.Application;
            Doc = commandData.Application.ActiveUIDocument.Document;
            _supportedFamilyNames = [.. FamilyParameterNames.Railings.Keys];

            List<ElementId> selectedIds = [.. UIDoc.Selection.GetElementIds()];


            if (selectedIds.Any())
            {
                List<FamilyInstance> insts = [.. selectedIds.Select(id => Doc.GetElement(id)).Where(elem => elem is FamilyInstance 
                && _supportedFamilyNames.Contains((elem as FamilyInstance).Symbol.FamilyName)).Cast<FamilyInstance>()];

                if (insts.Any())
                {
                    using (Transaction tx = new(Doc, "OLP: Авторазъединение поручней ограждений"))
                    {
                        FailureHandlingOptions failureHandlingOptions = tx.GetFailureHandlingOptions();
                        failureHandlingOptions.SetForcedModalHandling(false);
                        failureHandlingOptions.SetDelayedMiniWarnings(false);
                        failureHandlingOptions.SetFailuresPreprocessor(new SupressWarnings());
                        tx.SetFailureHandlingOptions(failureHandlingOptions);

                        tx.Start();

                        foreach (FamilyInstance inst in insts)
                        {
                            //Проверка включены ли стойки/кронштейны на прямых участках ограждений
                            bool condition1 = inst.GetParameterFromInstOrSym(FamilyParameterNames.Railings[inst.Symbol.FamilyName][26])?.AsInteger() == 1;
                            bool condition2 = inst.GetParameterFromInstOrSym(FamilyParameterNames.Railings[inst.Symbol.FamilyName][27])?.AsInteger() == 1;
                            bool condition3 = inst.GetParameterFromInstOrSym(FamilyParameterNames.Railings[inst.Symbol.FamilyName][28])?.AsInteger() == 1;
                            bool condition4 = inst.GetParameterFromInstOrSym(FamilyParameterNames.Railings[inst.Symbol.FamilyName][29])?.AsInteger() == 1;
                            
                            for (int i = 0; i <= 15; i++)
                            {

                                switch (inst.Symbol.FamilyName)
                                {
                                    case string when inst.Symbol.FamilyName == StairsRailing2:
                                        if ((condition1 || condition2) & new List<int> { 0, 2, 3, 4, 5 }.Contains(i)) continue;
                                        if ((condition3 || condition4) & new List<int> { 1, 9, 10, 11, 12 }.Contains(i)) continue;
                                        break;

                                    case string when inst.Symbol.FamilyName == StairsRailing3:
                                        if (condition1 & new List<int> { 0, 2, 3, 4, 5 }.Contains(i)) continue;
                                        if (condition3 & new List<int> { 1, 9, 10, 11, 12 }.Contains(i)) continue;
                                        break;
                                }
                                inst.LookupParameter(FamilyParameterNames.Railings[inst.Symbol.FamilyName][i])?.Set(0);
                            }
                        }

                        tx.Commit();
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}
