using Prism.Mvvm;
using System;

namespace OLP.AutoConnector.ViewModels
{
    public class InputDataVM : BindableBase
    {
        private double _railingConnectAlignX;
        public double RailingConnectAlignX 
        { 
            get => _railingConnectAlignX;
            set => SetProperty(ref _railingConnectAlignX, value);
        }

        public InputDataVM() 
        {
            _railingConnectAlignX = Math.Round(Properties.InputData.Default.RailingConnectionAlignX * 304.8);
        }
    }
}
