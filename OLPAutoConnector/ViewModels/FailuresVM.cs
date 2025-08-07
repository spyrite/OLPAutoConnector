using Autodesk.Revit.DB;
using OLP.AutoConnector.Models;
using Prism.Mvvm;
using System.Collections.Generic;

namespace OLP.AutoConnector.ViewModels
{
    public class FailuresVM : BindableBase
    {
        private readonly Document _doc;
        public string ProjectName { get => _doc.Title; }


        private List<FailureModel> _failureModels;
        public List<FailureModel> FailureModels { get => _failureModels; set => SetProperty(ref _failureModels, value); }

        public FailuresVM(Document doc, List<FailureModel> failureModels)
        {
            _doc = doc;
            _failureModels = failureModels;
        }
    }
}
