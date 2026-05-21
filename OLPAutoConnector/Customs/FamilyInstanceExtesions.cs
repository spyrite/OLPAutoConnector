using Autodesk.Revit.DB;
using OLP.AutoConnector.Resources;
using System;

namespace OLP.AutoConnector.Customs
{
    public static class FamilyInstanceExtesions
    {
        public static FamilyInstance GetHigherSuperComponent(this FamilyInstance sourceInst)
        {
            FamilyInstance higherSuperComponent = sourceInst;
            FamilyInstance superComponentToCheck = sourceInst.SuperComponent as FamilyInstance;
            while (superComponentToCheck != null)
            {
                higherSuperComponent = superComponentToCheck;
                superComponentToCheck = superComponentToCheck.SuperComponent as FamilyInstance;
            }

            return higherSuperComponent;
        }

        public static Parameter GetParameterFromInstOrSym(this FamilyInstance sourceInst, string parName) 
            => sourceInst.LookupParameter(parName) ?? sourceInst.Symbol.LookupParameter(parName);


        public static double GetFamilyVersion(this FamilyInstance inst)
            => inst.Symbol.get_Parameter(Guid.Parse(SupportedParameters.OLPFamilyVersion))?.AsDouble() ?? -1;
    }
}
