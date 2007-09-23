using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace GeniusBinding.Core.Tests
{
    class DataWithBindingList : BaseData
    {
        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; DoPropertyChanged("Name"); }
        }

        private BindingList<int> _IntList = new BindingList<int>();

        public BindingList<int> IntList
        {
            get { return _IntList; }
            set { _IntList = value; DoPropertyChanged("IntList"); }
        }

        
        private MyBindingList _MyIntList = new MyBindingList();
        /// <summary>
        /// checks when property is replaced, old reference is disposed
        /// </summary>
        public MyBindingList MyIntList
        {
            get { return _MyIntList; }
            set { _MyIntList = value; DoPropertyChanged("MyIntList"); }
        }

    }

    class MyBindingList : BindingList<int>
    {
        public event EventHandler OnFinalized;
        ~MyBindingList()
        {
            Console.WriteLine("~" + this.GetType().Name + "()");
            if (OnFinalized != null)
                OnFinalized(null, EventArgs.Empty);
        }
    }
}
