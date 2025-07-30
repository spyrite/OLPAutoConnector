using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLP.AutoConnector.Models
{
    public class FailureModel
    {
        public string Message { get; set; }
        public List<ElementId> Ids { get; set; }

        public FailureModel(string message)
        {
            Message = message;
            Ids = [];
        }
    }
}
