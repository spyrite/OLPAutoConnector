using OLP.AutoConnector.Resources;
using Prism.Mvvm;
using System;

namespace OLP.AutoConnector.ViewModels
{
    public class InputDataVM : BindableBase
    {
        private double _upperRailingConnectionX;
        public double UpperRailingConnectionX 
        { 
            get => _upperRailingConnectionX;
            set => SetProperty(ref _upperRailingConnectionX, value);
        }

        private double _upperRailingConnectionDZ;
        public double UpperRailingConnectionDZ
        {
            get => _upperRailingConnectionDZ;
            set => SetProperty(ref _upperRailingConnectionDZ, value);
        }

        private double _lowerRailingConnectionDZ;
        public double LowerRailingConnectionDZ
        {
            get => _lowerRailingConnectionDZ;
            set => SetProperty(ref _lowerRailingConnectionDZ, value);
        }

        public string UpperRailingConnectionXDesctription { get => InputDataDescriptions.UpperRailingConnectionX;  }
        public string UpperRailingConnectionDZDesctription { get => InputDataDescriptions.UpperRailingConnectionDZ; }
        public string LowerRailingConnectionXDesctription { get => InputDataDescriptions.LowerRailingConnectionDZ; }

        private bool _connectionType1Selected;
        public bool ConnectionType1Selected { get => _connectionType1Selected; set => SetProperty(ref _connectionType1Selected, value); }

        private bool _connectionType2Selected;
        public bool ConnectionType2Selected { get => _connectionType2Selected; set => SetProperty(ref _connectionType2Selected, value); }

        private bool _connectionType3Selected;
        public bool ConnectionType3Selected { get => _connectionType3Selected; set => SetProperty(ref _connectionType3Selected, value); }

        private bool _connectionType4Selected;
        public bool ConnectionType4Selected { get => _connectionType4Selected; set => SetProperty(ref _connectionType4Selected, value); }


        public InputDataVM() 
        {
            _connectionType1Selected = Properties.InputData.Default.RailingsConnectionType == 0;
            _connectionType2Selected = Properties.InputData.Default.RailingsConnectionType == 1;
            _connectionType3Selected = Properties.InputData.Default.RailingsConnectionType == 2;
            _connectionType4Selected = Properties.InputData.Default.RailingsConnectionType == 3;

            _upperRailingConnectionX = Math.Round(Properties.InputData.Default.UpperRailingConnectionX * 304.8);
            _upperRailingConnectionDZ = Math.Round(Properties.InputData.Default.UpperRailingConnectionDZ * 304.8);
            _lowerRailingConnectionDZ = Math.Round(Properties.InputData.Default.LowerRailingConnectionDZ * 304.8);
        }
    }
}
