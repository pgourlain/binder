using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;

namespace GeniusBinding.Core
{
    /// <summary>
    /// Wrapper a Bindinglist, to notify changes via <see cref="ICollectionChanged"/>
    /// </summary>
    class BindingListNotificationWrapper : ICollectionChanged, IDisposable
    {
        private string _Index;
        private int _intIndex;
        WeakReference _Bindinglist;
        #region ICollectionChanged Members
        public event EventHandler<CollectionChangedEventArgs> CollectionChanged;
        #endregion

        public BindingListNotificationWrapper(IBindingList alist)
        {
            alist.ListChanged += new ListChangedEventHandler(alist_ListChanged);
            _Bindinglist = new WeakReference(alist);
        }

        public void SetIndex(int index)
        {
            //_intIndex = int.Parse(sIndex);
            _intIndex = index;
        }

        public int Index
        {
            get
            {
                return _intIndex;
            }
        }

        void DoChange(CollectionChangedAction e, int newIndex, int oldIndex)
        {
            if (CollectionChanged != null && newIndex == _intIndex)
                CollectionChanged(this, new CollectionChangedEventArgs(e, newIndex, oldIndex));
        }

        void alist_ListChanged(object sender, ListChangedEventArgs e)
        {
            switch (e.ListChangedType)
            {
                case ListChangedType.ItemAdded:
                    DoChange(CollectionChangedAction.Add, e.NewIndex, e.OldIndex);
                    break;

                case ListChangedType.ItemChanged:
                    DoChange(CollectionChangedAction.Replace, e.NewIndex, e.OldIndex);
                    break;

                case ListChangedType.ItemDeleted:
                    DoChange(CollectionChangedAction.Remove, e.NewIndex, e.OldIndex);
                    break;

                case ListChangedType.ItemMoved:
                    DoChange(CollectionChangedAction.Move, e.NewIndex, e.OldIndex);
                    break;

                case ListChangedType.Reset:
                    DoChange(CollectionChangedAction.Reset, -1, -1);
                    break;
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_Bindinglist.IsAlive)
            {
                ((IBindingList)_Bindinglist.Target).ListChanged -= new ListChangedEventHandler(alist_ListChanged);
            }
        }

        #endregion
    }
}
