using Autodesk.Revit.DB;
using OLP.AutoConnector.Resources;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace OLP.AutoConnector.ViewModels
{
    public class ConnectRailingsVM : BindableBase
    {
        private RailingData _upperRailingData;
        private RailingData _lowerRailingData;

        #region Сведения
        public string TitleWithVersion { get => "Автосоединение ограждений v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
        public string UpperRailingInfo { get => $"Высота верхнего ограждения H1 = {Math.Round(_upperRailingData.Height * 304.8)} мм; " + (_upperRailingData.Mirrored ? "зеркально" : "не зеркально"); }
        public string LowerRailingInfo { get => $"Высота нижнего ограждения H2 = {Math.Round(_lowerRailingData.Height * 304.8)} мм; " + (_lowerRailingData.Mirrored ? "зеркально" : "не зеркально"); }
        #endregion

        #region Расстояния
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
        #endregion

        #region Типы соединения
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
        #endregion

        #region Поручни

        private bool _upperHandrailSelected;
        public bool UpperHandrailSelected 
        { 
            get => _upperHandrailSelected; 
            set
            {
                if (SetProperty(ref _upperHandrailSelected, value) & value == true)
                {
                    Properties.ConnectRailings.Default.HandrailToConnect = 0;
                    SetHandrailToConnectMode(RailingHandrailToConnect.Upper);
                    RaisePropertyChanged(nameof(UpperRailingInfo));
                    RaisePropertyChanged(nameof(LowerRailingInfo));
                }
            }
        }

        private bool _lowerHandrailSelected;
        public bool LowerHandrailSelected 
        { 
            get => _lowerHandrailSelected; 
            set
            {
                if (SetProperty(ref _lowerHandrailSelected, value) & value == true)
                {
                    Properties.ConnectRailings.Default.HandrailToConnect = 1;
                    SetHandrailToConnectMode(RailingHandrailToConnect.Lower);
                    RaisePropertyChanged(nameof(UpperRailingInfo));
                    RaisePropertyChanged(nameof(LowerRailingInfo));
                }
            }
        }

        #endregion

        #region Разрешения
        public bool AllowSelectHandrail { get => _upperRailingData.FamilyName == SupportedFamilyNames.StairsRailing2_3
                || _lowerRailingData.FamilyName == SupportedFamilyNames.StairsRailing2_3; }
        public bool AllowUpperRailingConnectionX { get => _connectionType1Selected || _connectionType2Selected || _connectionType3Selected || _connectionType4Selected; }
        public bool AllowUpperRailingConnectionDZ { get => _connectionType2Selected || _connectionType4Selected; }
        public bool AllowLowerRailingConnectionDZ { get => _connectionType3Selected || _connectionType4Selected; }

        private void UpdateAllowingXDZ()
        {
            RaisePropertyChanged(nameof(AllowUpperRailingConnectionX));
            RaisePropertyChanged(nameof(AllowUpperRailingConnectionDZ));   
            RaisePropertyChanged(nameof(AllowLowerRailingConnectionDZ));
        }

        private readonly List<RailingConnectionType> _allowedConnectionTypes;
        public bool AllowConnectionType1 { get => _allowedConnectionTypes.Contains(RailingConnectionType.AngleAngle); }
        public bool AllowConnectionType2 { get => _allowedConnectionTypes.Contains(RailingConnectionType.HorizontAngle); }
        public bool AllowConnectionType3 { get => _allowedConnectionTypes.Contains(RailingConnectionType.AngleHorizont); }
        public bool AllowConnectionType4 { get => _allowedConnectionTypes.Contains(RailingConnectionType.HorizontHorizont); }

        private readonly bool _allowInputDZ1;
        public bool AllowInputDZ1 { get => _allowInputDZ1; }
        private readonly bool _allowInputDZ2;
        public bool AllowInputDZ2 { get => _allowInputDZ2; }

        #region Выбрать ещё
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

        public ConnectRailingsVM (ref RailingData upperRailingData, ref RailingData lowerRailingData, List<RailingConnectionType> allowedConnectionTypes, bool allowInputDZ1, bool allowInputDZ2)
        {
            _upperRailingData = upperRailingData;
            _lowerRailingData = lowerRailingData;

            SetHandrailToConnectMode((RailingHandrailToConnect)Properties.ConnectRailings.Default.HandrailToConnect);

            _allowedConnectionTypes = allowedConnectionTypes;
            _allowInputDZ1 = allowInputDZ1;
            _allowInputDZ2 = allowInputDZ2;

            _connectionType1Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 0 & allowedConnectionTypes.Contains(RailingConnectionType.AngleAngle);
            _connectionType2Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 1 & allowedConnectionTypes.Contains(RailingConnectionType.HorizontAngle);
            _connectionType3Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 2 & allowedConnectionTypes.Contains(RailingConnectionType.AngleHorizont);
            _connectionType4Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 3 & allowedConnectionTypes.Contains(RailingConnectionType.HorizontHorizont);

            _upperHandrailSelected = Properties.ConnectRailings.Default.HandrailToConnect == 0;
            _lowerHandrailSelected = Properties.ConnectRailings.Default.HandrailToConnect == 1;

            _upperRailingConnectionX = Math.Round(Properties.ConnectRailings.Default.UpperRailingConnectionX * 304.8);
            if (allowInputDZ1 == false) Properties.ConnectRailings.Default.UpperRailingConnectionDZ = 0;
            if (allowInputDZ2 == false) Properties.ConnectRailings.Default.LowerRailingConnectionDZ = 0;
            _upperRailingConnectionDZ = Math.Round(Properties.ConnectRailings.Default.UpperRailingConnectionDZ * 304.8);
            _lowerRailingConnectionDZ = Math.Round(Properties.ConnectRailings.Default.LowerRailingConnectionDZ * 304.8);          

            _selectAnymore = Properties.ConnectRailings.Default.SelectAnymore;
            _selectAnymoreWithDialog = Properties.ConnectRailings.Default.SelectAnymoreWithDialog;
        }

        private void SetHandrailToConnectMode(RailingHandrailToConnect handrailToConnect)
        {
            FamilyParameterNames.UpdateRailings(handrailToConnect);
            _upperRailingData.UpdatePrimaryHandrailData(handrailToConnect);
            _lowerRailingData.UpdatePrimaryHandrailData(handrailToConnect);
        }

        private DelegateCommand helpOpen;
        public ICommand HelpOpen => helpOpen ??= new DelegateCommand(PerformHelpOpen);

        private void PerformHelpOpen()
        {
            Process.Start(Properties.AutoConnector.Default.ConnectRailingsHelpURL);
        }
    }
}
