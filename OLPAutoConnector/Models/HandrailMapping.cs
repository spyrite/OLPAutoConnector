using Autodesk.Revit.DB;
using OLP.AutoConnector.Resources;
using OLP.AutoConnector.ViewModels;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OLP.AutoConnector.Models
{
    public class HandrailMapping : BindableBase
    {
        private readonly HandrailData _upperRailingHandrailData;
        private readonly int _nColumn;

        public HandrailData UpperRailingHandrailData { get => _upperRailingHandrailData; }


        private HandrailData _lowerRailingHandrailData;
        public HandrailData LowerRailingHandrailData 
        { 
            get => _lowerRailingHandrailData;
            set
            {
                if (SetProperty(ref _lowerRailingHandrailData, value))
                {
                    switch (_nColumn)
                    {
                        case 0:
                            Properties.ConnectRailings.Default.LowerRailingHandrailDataInt_0 = ConnectRailingsVM.AvailableLowerRailingHandrails.IndexOf(value);
                            break;
                        case 1:
                            Properties.ConnectRailings.Default.LowerRailingHandrailDataInt_1 = ConnectRailingsVM.AvailableLowerRailingHandrails.IndexOf(value);
                            break;
                    }
                }
            }
        }

        private double _upperRailingConnectionX;
        public string UpperRailingConnectionX
        {
            get => _upperRailingConnectionX.ToString();
            set
            {
                double.TryParse(value, out double decimalValue);
                if (SetProperty(ref _upperRailingConnectionX, decimalValue))
                {
                    switch (_nColumn)
                    {
                        case 0:
                            Properties.ConnectRailings.Default.UpperRailingConnectionX_0 = decimalValue / 304.8;
                            break;
                        case 1:
                            Properties.ConnectRailings.Default.UpperRailingConnectionX_1 = decimalValue / 304.8;
                            break;
                    }
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
                    switch (_nColumn)
                    {
                        case 0:
                            Properties.ConnectRailings.Default.UpperRailingConnectionDZ_0 = decimalValue / 304.8;
                            break;
                        case 1:
                            Properties.ConnectRailings.Default.UpperRailingConnectionDZ_1 = decimalValue / 304.8;
                            break;
                    }
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
                    switch (_nColumn)
                    {
                        case 0:
                            Properties.ConnectRailings.Default.LowerRailingConnectionDZ_0 = decimalValue / 304.8;
                            break;
                        case 1:
                            Properties.ConnectRailings.Default.LowerRailingConnectionDZ_1 = decimalValue / 304.8;
                            break;

                    }
                }
            }
        }


        public double ConnectXFromEdgeSupport { get; set; }
        public Plane ConnectionYOZPlane { get; set; }
        public double ConnectAngle { get; set; }
        public XYZ ConnectAxisDir { get; set; }


        public HandrailMapping(HandrailData upperRailingHandrailData, int nColumn)
        {
            _upperRailingHandrailData = upperRailingHandrailData;
            _nColumn = nColumn;
            switch (nColumn)
            {
                case 0:
                    _lowerRailingHandrailData = Properties.ConnectRailings.Default.LowerRailingHandrailDataInt_0 < ConnectRailingsVM.AvailableLowerRailingHandrails.Count
                        ? ConnectRailingsVM.AvailableLowerRailingHandrails[Properties.ConnectRailings.Default.LowerRailingHandrailDataInt_0]
                        : ConnectRailingsVM.AvailableLowerRailingHandrails.FirstOrDefault();
                    _upperRailingConnectionX = Math.Round(Properties.ConnectRailings.Default.UpperRailingConnectionX_0 * 304.8);
                    _upperRailingConnectionDZ = Math.Round(Properties.ConnectRailings.Default.UpperRailingConnectionDZ_0 * 304.8);
                    _lowerRailingConnectionDZ = Math.Round(Properties.ConnectRailings.Default.LowerRailingConnectionDZ_0 * 304.8);
                    break;
                case 1:
                    _lowerRailingHandrailData = Properties.ConnectRailings.Default.LowerRailingHandrailDataInt_1 < ConnectRailingsVM.AvailableLowerRailingHandrails.Count
                        ? ConnectRailingsVM.AvailableLowerRailingHandrails[Properties.ConnectRailings.Default.LowerRailingHandrailDataInt_1]
                        : ConnectRailingsVM.AvailableLowerRailingHandrails.FirstOrDefault();
                    _upperRailingConnectionX = Math.Round(Properties.ConnectRailings.Default.UpperRailingConnectionX_1 * 304.8);
                    _upperRailingConnectionDZ = Math.Round(Properties.ConnectRailings.Default.UpperRailingConnectionDZ_1 * 304.8);
                    _lowerRailingConnectionDZ = Math.Round(Properties.ConnectRailings.Default.LowerRailingConnectionDZ_1 * 304.8);
                    break;
            }
            
        }
    }
}