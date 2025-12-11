using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
