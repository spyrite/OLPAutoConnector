using OLP.AutoConnector.Resources;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace OLP.AutoConnector.ViewModels
{
    public class ConnectRailingsVM : BindableBase
    {
        private double _upperRailingConnectionX;
        public string UpperRailingConnectionX 
        { 
            get => _upperRailingConnectionX.ToString();
            set
            {
                double.TryParse(value, out double decimalValue);
                if (SetProperty(ref _upperRailingConnectionX, decimalValue))
                {
                    Properties.ConnectRailings.Default.UpperRailingConnectionX = decimalValue / 304.8;
                }    
            }
        }

        private double _upperRailingConnectionDZ;
        public string UpperRailingConnectionDZ
        {
            get => _upperRailingConnectionDZ.ToString();
            set
            {
                double.TryParse(value, out double decimalValue);
                if (SetProperty(ref _upperRailingConnectionDZ, decimalValue))
                {
                    Properties.ConnectRailings.Default.UpperRailingConnectionDZ = decimalValue / 304.8;
                }
            }
        }

        private double _lowerRailingConnectionDZ;
        public string LowerRailingConnectionDZ
        {
            get => _lowerRailingConnectionDZ.ToString();
            set
            {
                double.TryParse(value, out double decimalValue);
                if (SetProperty(ref _lowerRailingConnectionDZ, decimalValue))
                {
                    Properties.ConnectRailings.Default.LowerRailingConnectionDZ = decimalValue / 304.8;
                }
            }
        }

        public string UpperRailingConnectionXDesctription { get => InputDataDescriptions.UpperRailingConnectionX;  }
        public string UpperRailingConnectionDZDesctription { get => InputDataDescriptions.UpperRailingConnectionDZ; }
        public string LowerRailingConnectionXDesctription { get => InputDataDescriptions.LowerRailingConnectionDZ; }

        private bool _connectionType1Selected;
        public bool ConnectionType1Selected 
        { 
            get => _connectionType1Selected; 
            set
            {
                if (SetProperty(ref _connectionType1Selected, value))
                {
                    UpdateAllowingXDZ();
                    if (value == true) Properties.ConnectRailings.Default.RailingsConnectionType = 0;
                }
            }
        }

        private bool _connectionType2Selected;
        public bool ConnectionType2Selected 
        { 
            get => _connectionType2Selected; 
            set
            {
                if (SetProperty(ref _connectionType2Selected, value))
                {
                    UpdateAllowingXDZ();
                    if (value == true) Properties.ConnectRailings.Default.RailingsConnectionType = 1;
                }
            }
        }

        private bool _connectionType3Selected;
        public bool ConnectionType3Selected
        { 
            get => _connectionType3Selected; 
            set
            {
                if (SetProperty(ref _connectionType3Selected, value))
                {
                    UpdateAllowingXDZ();
                    if (value == true) Properties.ConnectRailings.Default.RailingsConnectionType = 2;
                }
            }
        }

        private bool _connectionType4Selected;
        public bool ConnectionType4Selected
        { 
            get => _connectionType4Selected;
            set
            {
                if (SetProperty(ref _connectionType4Selected, value))
                {
                    UpdateAllowingXDZ();
                    if (value == true) Properties.ConnectRailings.Default.RailingsConnectionType = 3;
                }
            }       
        }

        public bool AllowUpperRailingConnectionX { get => _connectionType1Selected || _connectionType2Selected || _connectionType3Selected || _connectionType4Selected; }
        public bool AllowUpperRailingConnectionDZ { get => _connectionType2Selected || _connectionType4Selected; }
        public bool AllowLowerRailingConnectionDZ { get => _connectionType3Selected || _connectionType4Selected; }

        private void UpdateAllowingXDZ()
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AllowUpperRailingConnectionX"));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AllowUpperRailingConnectionDZ"));   
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AllowLowerRailingConnectionDZ"));
        }

        private readonly string _upperRailingInfo;
        public string UpperRailingInfo { get => _upperRailingInfo; }

        private readonly string _lowerRailingInfo;
        public string LowerRailingInfo { get => _lowerRailingInfo; }



        private readonly List<RailingConnectionType> _allowedConnectionTypes;
        public bool AllowConnectionType1 { get => _allowedConnectionTypes.Contains(RailingConnectionType.AngleAngle); }
        public bool AllowConnectionType2 { get => _allowedConnectionTypes.Contains(RailingConnectionType.HorizontAngle); }
        public bool AllowConnectionType3 { get => _allowedConnectionTypes.Contains(RailingConnectionType.AngleHorizont); }
        public bool AllowConnectionType4 { get => _allowedConnectionTypes.Contains(RailingConnectionType.HorizontHorizont); }



        private readonly bool _allowInputDZ1;
        public bool AllowInputDZ1 { get => _allowInputDZ1; }
        private readonly bool _allowInputDZ2;
        public bool AllowInputDZ2 { get => _allowInputDZ2; }


        private bool? _selectAnymore;
        public bool? SelectAnymore 
        { 
            get => _selectAnymore;
            set
            {
                if (SetProperty(ref _selectAnymore, value))
                {
                    OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AllowSelectAnymoreWithDialog"));
                    if (value == false) SelectAnymoreWithDialog = false;
                    
                    Properties.ConnectRailings.Default.SelectAnymore = value ?? false;
                }
            }
                
        }

        private bool? _selectAnymoreWithDialog;
        public bool? SelectAnymoreWithDialog 
        { 
            get => _selectAnymoreWithDialog; 
            set
            {
                if (SetProperty(ref _selectAnymoreWithDialog, value))
                {
                    Properties.ConnectRailings.Default.SelectAnymoreWithDialog = value ?? false;
                }
            }
                

        }

        
        public bool AllowSelectAnymoreWithDialog { get => _selectAnymore == true; }


        public ConnectRailingsVM (RailingData upperRailingData, RailingData lowerRailingData, List<RailingConnectionType> allowedConnectionTypes, bool allowInputDZ1, bool allowInputDZ2)
        {
            _allowedConnectionTypes = allowedConnectionTypes;
            _allowInputDZ1 = allowInputDZ1;
            _allowInputDZ2 = allowInputDZ2;

            _connectionType1Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 0 & allowedConnectionTypes.Contains(RailingConnectionType.AngleAngle);
            _connectionType2Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 1 & allowedConnectionTypes.Contains(RailingConnectionType.HorizontAngle);
            _connectionType3Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 2 & allowedConnectionTypes.Contains(RailingConnectionType.AngleHorizont);
            _connectionType4Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 3 & allowedConnectionTypes.Contains(RailingConnectionType.HorizontHorizont);

            _upperRailingConnectionX = Math.Round(Properties.ConnectRailings.Default.UpperRailingConnectionX * 304.8);
            if (allowInputDZ1 == false) Properties.ConnectRailings.Default.UpperRailingConnectionDZ = 0;
            if (allowInputDZ2 == false) Properties.ConnectRailings.Default.LowerRailingConnectionDZ = 0;
            _upperRailingConnectionDZ = Math.Round(Properties.ConnectRailings.Default.UpperRailingConnectionDZ * 304.8);
            _lowerRailingConnectionDZ = Math.Round(Properties.ConnectRailings.Default.LowerRailingConnectionDZ * 304.8);

            _upperRailingInfo = $"Высота верхнего ограждения H1 = {Math.Round(upperRailingData.Height * 304.8)} мм; " + (upperRailingData.Mirrored ? "зеркально" : "не зеркально");
            _lowerRailingInfo = $"Высота нижнего ограждения H2 = {Math.Round(lowerRailingData.Height * 304.8)} мм; " + (lowerRailingData.Mirrored ? "зеркально" : "не зеркально");

            _selectAnymore = Properties.ConnectRailings.Default.SelectAnymore;
            _selectAnymoreWithDialog = Properties.ConnectRailings.Default.SelectAnymoreWithDialog;
        }

        private DelegateCommand helpOpen;
        public ICommand HelpOpen => helpOpen ??= new DelegateCommand(PerformHelpOpen);

        private void PerformHelpOpen()
        {
            Process.Start(Properties.AutoConnector.Default.ConnectRailingsHelpURL);
        }
    }
}
