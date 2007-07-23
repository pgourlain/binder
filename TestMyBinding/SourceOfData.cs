using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;

namespace TestMyBinding
{
    /// <summary>
    /// le code de gestion de INotifyPropertyChanged est volontairement dupliquer dans cette clase et la classe DestinationOfData
    /// il est donc evident que l'on peut mutualiser ce code 
    /// </summary>
    class SourceOfData : INotifyPropertyChanged
    {
    #region INotifyPropertyChanged Members
    protected void DoPropertyChanged(string propName)
    {
        if (PropertyChanged != null)
            PropertyChanged(this, new PropertyChangedEventArgs(propName));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    #endregion

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

        ~SourceOfData()
        {
            Console.WriteLine("~" + this.GetType().Name +"()");
        }

    }
}
