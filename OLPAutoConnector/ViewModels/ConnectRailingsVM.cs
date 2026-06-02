using OLP.AutoConnector.Models;
using OLP.AutoConnector.Resources;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

namespace OLP.AutoConnector.ViewModels
{
    public class ConnectRailingsVM : BindableBase
    {
        #region Сведения
        public string TitleWithVersion { get => "Автосоединение ограждений v" + Assembly.GetExecutingAssembly().GetName().Version.ToString(); }

        private readonly string _upperRailingInfo;
        public string UpperRailingInfo { get => _upperRailingInfo; }

        private readonly string _lowerRailingInfo;
        public string LowerRailingInfo { get => _lowerRailingInfo; }

        #endregion

        #region Тип соединения
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

        private readonly List<RailingConnectionType> _allowedConnectionTypes;
        public bool AllowConnectionType1 { get => _allowedConnectionTypes.Contains(RailingConnectionType.AngleAngle); }
        public bool AllowConnectionType2 { get => _allowedConnectionTypes.Contains(RailingConnectionType.HorizontAngle); }
        public bool AllowConnectionType3 { get => _allowedConnectionTypes.Contains(RailingConnectionType.AngleHorizont); }
        public bool AllowConnectionType4 { get => _allowedConnectionTypes.Contains(RailingConnectionType.HorizontHorizont); }
        #endregion

        #region Настройка окна 

        public double LowerHadrailColumnWidth { get => _handrailMappings.Count > 1 ? 70 : 0; }
        public string UpperRailingH { get => InputDataDescriptions.UpperRailingH; }
        public string LowerRailingH { get => InputDataDescriptions.LowerRailingH; }
        public string UpperRailingConnectionXDesctription { get => InputDataDescriptions.UpperRailingConnectionX;  }
        public string UpperRailingConnectionDZDesctription { get => InputDataDescriptions.UpperRailingConnectionDZ; }
        public string LowerRailingConnectionXDesctription { get => InputDataDescriptions.LowerRailingConnectionDZ; }

        public bool AllowUpperRailingConnectionX { get => _connectionType1Selected || _connectionType2Selected || _connectionType3Selected || _connectionType4Selected; }
        public bool AllowUpperRailingConnectionDZ { get => _connectionType2Selected || _connectionType4Selected; }
        public bool AllowLowerRailingConnectionDZ { get => _connectionType3Selected || _connectionType4Selected; }

        private void UpdateAllowingXDZ()
        {
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AllowUpperRailingConnectionX"));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AllowUpperRailingConnectionDZ"));
            OnPropertyChanged(new System.ComponentModel.PropertyChangedEventArgs("AllowLowerRailingConnectionDZ"));
        }

        private readonly bool _allowInputDZ1;
        public bool AllowInputDZ1 { get => _allowInputDZ1; }
        private readonly bool _allowInputDZ2;
        public bool AllowInputDZ2 { get => _allowInputDZ2; }
        #endregion

        #region Сопоставление поручней
        private ObservableCollection<HandrailMapping> _handrailMappings;
        public ObservableCollection<HandrailMapping> HandrailMappings 
        { 
            get => _handrailMappings;
            set => SetProperty(ref _handrailMappings, value);
        }

        public HandrailMapping HandrailMappingFirst { get => _handrailMappings.First(); }
        public HandrailMapping HandrailMappingLast { get => _handrailMappings.Last(); }

        private static List<HandrailData> _availableLowerRailingHandrails;
        public static List<HandrailData> AvailableLowerRailingHandrails { get => _availableLowerRailingHandrails; }
        #endregion

        #region Выбор
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
        #endregion

        //Конструктор
        public ConnectRailingsVM (RailingData upperRailingData, RailingData lowerRailingData, List<RailingConnectionType> allowedConnectionTypes, bool allowInputDZ1, bool allowInputDZ2)
        {
            
            _allowedConnectionTypes = allowedConnectionTypes;
            _allowInputDZ1 = allowInputDZ1;
            _allowInputDZ2 = allowInputDZ2;

            _connectionType1Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 0 & allowedConnectionTypes.Contains(RailingConnectionType.AngleAngle);
            _connectionType2Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 1 & allowedConnectionTypes.Contains(RailingConnectionType.HorizontAngle);
            _connectionType3Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 2 & allowedConnectionTypes.Contains(RailingConnectionType.AngleHorizont);
            _connectionType4Selected = Properties.ConnectRailings.Default.RailingsConnectionType == 3 & allowedConnectionTypes.Contains(RailingConnectionType.HorizontHorizont);

            _availableLowerRailingHandrails = [new HandrailData()];
            _availableLowerRailingHandrails.AddRange(lowerRailingData.Handrails);
            _handrailMappings = new(upperRailingData.Handrails.Select(h => new HandrailMapping(h, upperRailingData.Handrails.IndexOf(h))));

            _upperRailingInfo = $"Верхнее ограждение {(upperRailingData.Mirrored ? "зеркально" : "не зеркально")}";
            _lowerRailingInfo = $"Нижнее ограждение {(lowerRailingData.Mirrored ? "зеркально" : "не зеркально")}";

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
