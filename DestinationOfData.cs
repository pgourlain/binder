using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace TestMyBinding
{
    class DestinationOfData : BaseData
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
    }
}
