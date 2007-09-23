using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using GeniusBinding.Core;

namespace TestMyBinding
{
    /// <summary>
    /// le code de gestion de INotifyPropertyChanged est volontairement dupliquer dans cette clase et la classe DestinationOfData
    /// il est donc evident que l'on peut mutualiser ce code 
    /// </summary>
    class SourceOfData : INotifyPropertyChanged
    {

        public SourceOfData()
        {
            //SourceOfData1 data = new SourceOfData1();
            //data.Prop2 = 654321;
            //_List.Add(data);
        }

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

        private IList<SourceOfData1> _List = new MyList<SourceOfData1>();

        public IList<SourceOfData1> List
        {
            get
            {
                return _List;
            }
            set
            {
                _List = value;
                DoPropertyChanged("List");
            }
        }
    }

    class MyList<T> : IList, IList<T>, ICollectionChanged
    {
        List<T> _under = new List<T>();
        #region IList Members

        public int Add(object value)
        {
            int result = ((IList)_under).Add(value);
            DoChanged(CollectionChangedAction.Add, result, -1);
            return result;
        }

        public void Clear()
        {
            _under.Clear();
            DoChanged(CollectionChangedAction.Reset, -1, -1);
        }

        public bool Contains(object value)
        {
            return _under.Contains((T)value);
        }

        public int IndexOf(object value)
        {
            return _under.IndexOf((T)value) ;
        }

        public void Insert(int index, object value)
        {
            _under.Insert(index, (T)value);
        }

        public bool IsFixedSize
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public bool IsReadOnly
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public void Remove(object value)
        {
            int index = _under.IndexOf((T)value);
            _under.RemoveAt(index);
            DoChanged(CollectionChangedAction.Remove, index, index);
        }

        public void RemoveAt(int index)
        {
            _under.RemoveAt(index);
            DoChanged(CollectionChangedAction.Remove, index, index);
        }

        public object this[int index]
        {
            get
            {
                return _under[index];
            }
            set
            {
                _under[index] = (T)value;
                DoChanged(CollectionChangedAction.Replace, index, index);
            }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public int Count
        {
            get { return _under.Count; }
        }

        public bool IsSynchronized
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        public object SyncRoot
        {
            get { throw new Exception("The method or operation is not implemented."); }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion

        private void DoChanged(CollectionChangedAction action, int newIndex, int oldIndex)
        {
            if (CollectionChanged != null)
                CollectionChanged(this, new CollectionChangedEventArgs(action, newIndex, oldIndex));
        }
        #region ICollectionChanged Members

        public event EventHandler<CollectionChangedEventArgs> CollectionChanged;

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public void Insert(int index, T item)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        T IList<T>.this[int index]
        {
            get
            {
                throw new Exception("The method or operation is not implemented.");
            }
            set
            {
                throw new Exception("The method or operation is not implemented.");
            }
        }

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            _under.Add(item);
            DoChanged(CollectionChangedAction.Add, _under.Count - 1, -1);
        }

        public bool Contains(T item)
        {
            return _under.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new Exception("The method or operation is not implemented.");
        }

        public bool Remove(T item)
        {
            int index = _under.IndexOf(item);
            bool result = _under.Remove(item);
            if (result)
            {
                DoChanged(CollectionChangedAction.Remove, index, index);
            }
            return result;
        }

        #endregion

        #region IEnumerable<T> Members

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new Exception("The method or operation is not implemented.");
        }

        #endregion
    }

    class SourceOfData1 : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members
        protected void DoPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
        private int _prop2;

        public int Prop2
        {
            get { return _prop2; }
            set
            {
                Console.WriteLine("Set Prop2 of Source : " + value);
                if (_prop2 != value)
                {
                    _prop2 = value;
                    DoPropertyChanged("Prop2");
                }
            }
        }
    }
}
