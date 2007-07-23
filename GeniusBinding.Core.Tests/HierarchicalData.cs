using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace GeniusBinding.Core.Tests
{
    /// <summary>
    /// classe de test pour le binding hierarchique
    /// </summary>
    /// <typeparam name="TData"></typeparam>
    public class BaseNode : INotifyPropertyChanged
    {
        public BaseNode(string id)
        {
        }

        private string _Id;
        public string Id
        {
            get
            {
                return _Id;
            }
        }

        private BaseNode _Left;

        public BaseNode Left
        {
            get { return _Left; }
            set { _Left = value; DoChanged("Left"); }
        }

        private BaseNode _Right;

        public BaseNode Right
        {
            get { return _Right; }
            set { _Right = value; DoChanged("Right"); }
        }

        #region INotifyPropertyChanged Members
        protected void DoChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public event EventHandler OnFinalized;

        ~BaseNode()
        {
            if (OnFinalized != null)
                OnFinalized(_Id, EventArgs.Empty);
        }
    }

    class MyNode<TData> : BaseNode
    {

        public MyNode(string id, TData data)
            : base(id)
        {
            _UserData = data;
        }

        private TData _UserData;

        public TData UserData
        {
            get { return _UserData; }
            set
            {
                _UserData = value;
                DoChanged("UserData");
            }
        }
    }

}
