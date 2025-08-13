using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static OLP.AutoConnector.Resources.SupportedFamilyNames;

namespace OLP.AutoConnector.Resources
{
    internal static class SubFamilyNames
    {
        internal static Dictionary<string, List<string>> Railings = new()
        {
            {StairsRailing1,
                [
                    "312_Вложенное_Труба_ГОСТ 10704-91(ГОСТ 8732-78) (ОбщМод_РабПлоск)",
                    "313_Вложенное_Ряд труб_ГОСТ 8639-82 (ОбщМод_РабПлоск)",
                ]
            },
            {StairsRailing2, 
                [
                    "312_Вложенное_Труба_ГОСТ 10704-91(ГОСТ 8732-78) (ОбщМод_РабПлоск)",
                    "313_Вложенное_Ряд труб_ГОСТ 8639-82 (ОбщМод_РабПлоск)",
                ]
            },
            {StairsRailing3,
                [
                    "312_Вложенное_Труба_ГОСТ 10704-91(ГОСТ 8732-78) (ОбщМод_РабПлоск)",
                    "312_Вложенное_Круг_Г(ГОСТ 2590-2006) (ОбщМод_РабПлоск)",
                ]
            },
        };
    }
}
