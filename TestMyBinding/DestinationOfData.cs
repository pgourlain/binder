using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using System.ComponentModel;

namespace TestMyBinding
{
    class DestinationOfData : INotifyPropertyChanged
    {
        int _Prop1Dest;

        public int Prop1Dest
        {
            get { return _Prop1Dest; }
            set
            {
                Console.WriteLine("Set Prop1Dest of Destination : " + value); 

                if (_Prop1Dest != value)
                {
                    _Prop1Dest = value;
                    DoPropertyChanged("Prop1Dest");
                }
            }
        }

        double _Prop1Destdouble;

        public double Prop1DestDouble
        {
            get { return _Prop1Destdouble; }
            set
            {
                Console.WriteLine("Set Prop1DestDouble of Destination : " + value);

                if (_Prop1Destdouble != value)
                {
                    _Prop1Destdouble = value;
                    DoPropertyChanged("Prop1DestDouble");
                }
            }
        }

        private Point _Point;

        public Point PropPoint
        {
            get { return _Point; }
            set {
                Console.WriteLine("Set PropPoint of Destination : " + value);
                if (_Point != value)
                {
                    _Point = value;
                    DoPropertyChanged("PropPoint");
                }
            }
        }


        #region INotifyPropertyChanged Members
        protected void DoPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}
