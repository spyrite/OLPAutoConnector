using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static OLP.AutoConnector.Resources.SupportedFamilyNames;

namespace OLP.AutoConnector.Resources
{
    internal class SupportedFamilyVersions
    {
        internal static Dictionary<string, double> Railings = new()
        {
            {StairsRailing1_1, 6},
            {StairsRailing1_2, 24},
            {StairsRailing1_3, 1},
            {StairsRailing2_1, 22},
            {StairsRailing2_2, 4},
            {StairsRailing2_3, 12},
            {StairsRailing3, 16},
        };
    }
}
