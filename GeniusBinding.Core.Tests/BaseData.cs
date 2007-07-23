using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace GeniusBinding.Core.Tests
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

        public event EventHandler OnFinalized;
        ~BaseData()
        {

            Console.WriteLine("~" + this.GetType().Name +"()");
            if (OnFinalized != null)
                OnFinalized(null, EventArgs.Empty);
        }
    }
}
