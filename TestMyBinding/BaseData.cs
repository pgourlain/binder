using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace TestMyBinding
{
    class BaseData : INotifyPropertyChanged
    {

        protected void DoPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        ~BaseData()
        {
            Console.WriteLine("~" + this.GetType().Name +"()");
        }
    }
}
