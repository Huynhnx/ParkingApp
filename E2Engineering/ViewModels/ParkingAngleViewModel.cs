using Autodesk.AutoCAD.DatabaseServices;
using MVVMCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ParkingApp.ViewModels
{
    public class ParkingAngleViewModel: ViewModelBase
    {
        public ObjectId objId = ObjectId.Null;
        public RelayCommand SelectPointCmd { get; set; }
        public RelayCommand CreateArcCmd { get; set; }
        public RelayCommand OkCmd { get; set; }
        public RelayCommand CancelCmd { get; set; }
        int startnumber;
        public int StartNumber
        {
            get
            {
                return startnumber;
            }
            set
            {
                if (startnumber!= value)
                {
                    startnumber = value;
                    RaisePropertyChanged("StartNumber");
                }
            }
        }
        double parkingarclength;
        public double ParkingArcLength
        {
            get
            {
                return parkingarclength;
            }
            set
            {
                if (value != parkingarclength)
                {
                    parkingarclength = value;
                    RaisePropertyChanged("ParkingArcLength");
                }
            }
        }
        double parkingstallwidth;
        public double ParkingStallWidth
        {
            get
            {
                return parkingstallwidth;
            }
            set
            {
                if (value != parkingstallwidth)
                {
                    parkingstallwidth = value;
                    RaisePropertyChanged("ParkingStallWidth");
                }
            }
        }

        int numberofSpot;
        public int NumberOfSpot
        {
            get
            {
                return numberofSpot;
            }
            set
            {
                if (value != numberofSpot)
                {
                    numberofSpot = value;
                    ParkingStallWidth = ParkingArcLength / numberofSpot;
                    RaisePropertyChanged("NumberOfSpot");
                }
            }
        }
        double parkingdepth =16;
        public double ParkingDepth
        {
            get
            {
                return parkingdepth;
            }
            set
            {
                if (value != parkingdepth)
                {
                    parkingdepth = value;
                    RaisePropertyChanged("ParkingDepth");
                }
            }
        }
        double parkingpointoffset =2;
        public double ParkingPointOffset
        {
            get
            {
                return parkingpointoffset;
            }
            set
            {
                if (value != parkingpointoffset)
                {
                    parkingpointoffset = value;
                    RaisePropertyChanged("ParkingPointOffset");
                }
            }
        }
        double wheelstoplength;
        public double WheelStopLength
        {
            get
            {
                return wheelstoplength;
            }
            set
            {
                if (value != wheelstoplength)
                {
                    wheelstoplength = value;
                    RaisePropertyChanged("WheelStopLength");
                }
            }
        }
        double wheelstopdepth = 0.75;
        public double WheelStopDepth
        {
            get
            {
                return wheelstopdepth;
            }
            set
            {
                if (value != wheelstopdepth)
                {
                    wheelstopdepth = value;
                    RaisePropertyChanged("WheelStopDepth");
                }
            }
        }
        double wheelstopoffset = 1.75;
        public double WheelStopOffset
        {
            get
            {
                return wheelstopoffset;
            }
            set
            {
                if (value != wheelstopoffset)
                {
                    wheelstopoffset = value;
                    RaisePropertyChanged("WheelStopOffset");
                }
            }
        }
        bool wheelstoprequired = true;
        public bool WheelStopRequired
        {
            get
            {
                return wheelstoprequired;
            }
            set
            {
                if (value != wheelstoprequired)
                {
                    wheelstoprequired = value;
                    RaisePropertyChanged("WheelStopRequired");
                }
            }
        }
        bool wheelstoppointstart;
        public bool WheelStopPointStart
        {
            get
            {
                return wheelstoppointstart;
            }
            set
            {
                if (value != wheelstoppointstart)
                {
                    wheelstoppointstart = value;
                    RaisePropertyChanged("WheelStopPointStart");
                }
            }
        }
        bool wheelstoppointend;
        public bool WheelStopPointEnd
        {
            get
            {
                return wheelstoppointend;
            }
            set
            {
                if (value != wheelstoppointend)
                {
                    wheelstoppointend = value;
                    RaisePropertyChanged("WheelStopPointEnd");
                }
            }
        }
        bool parkinglinepointstart;
        public bool ParkingLinePointStart
        {
            get
            {
                return parkinglinepointstart;
            }
            set
            {
                if (value != parkinglinepointstart)
                {
                    parkinglinepointstart = value;
                    RaisePropertyChanged("ParkingLinePointStart");
                }
            }
        }
        bool parkinglinepointend;
        public bool ParkingLinePointEnd
        {
            get
            {
                return parkinglinepointend;
            }
            set
            {
                if (value != parkinglinepointend)
                {
                    parkinglinepointend = value;
                    RaisePropertyChanged("ParkingLinePointEnd");
                }
            }
        }
    }
}
