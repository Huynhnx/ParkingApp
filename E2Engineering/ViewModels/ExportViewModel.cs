using Autodesk.AutoCAD.DatabaseServices;
using MVVMCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingApp.ViewModels
{
    class ExportViewModel : ViewModelBase
    {
        public RelayCommand SelectAdditionPointCmd { get; set; }
        public RelayCommand ExportCmd { get; set; }
        bool parkingpoint;
        public bool ParkingPoint
        {
            get
            {
                return parkingpoint;
            }
            set
            {
                if (value!= parkingpoint)
                {
                    parkingpoint=value;
                    RaisePropertyChanged("ParkingPoint");
                    if (!parkingpoint)
                    {
                        bCombine = false;
                    }
                }
            }
        }
        bool wheelstoppoint;
        public bool WheelStopPoint
        {
            get
            {
                return wheelstoppoint;
            }
            set
            {
                if (value != wheelstoppoint)
                {
                    wheelstoppoint=value;
                    RaisePropertyChanged("WheelStopPoint");
                    if (!wheelstoppoint)
                    {
                        bCombine = false;
                    }
                }
            }
        }
        bool bCombine;
        public bool Combine
        {
            get
            {
                return bCombine;
            }
            set
            {
                if (value != bCombine)
                {
                    bCombine=value;
                    if (bCombine)
                    {
                        wheelstoppoint = true;
                        parkingpoint = true;
                        RaisePropertyChanged("WheelStopPoint");
                        RaisePropertyChanged("ParkingPoint");
                    }
                    RaisePropertyChanged("Combine");
                }
            }
        }
    }
}
