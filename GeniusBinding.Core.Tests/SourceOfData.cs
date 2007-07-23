using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace GeniusBinding.Core.Tests
{
    /// <summary>
    /// le code de gestion de INotifyPropertyChanged est volontairement dupliquer dans cette clase et la classe DestinationOfData
    /// il est donc evident que l'on peut mutualiser ce code 
    /// </summary>
    class SourceOfData : BaseData
    {
        private int _prop1;

        public int Prop1
        {
            get { return _prop1; }
            set 
            {
                Console.WriteLine("Set Prop1 of Source : " + value); 
                if (_prop1 != value)
                {
                    _prop1 = value;
                    DoPropertyChanged("Prop1");
                }
            }
        }
    }
}
