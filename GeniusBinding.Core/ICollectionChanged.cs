using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    /// <summary>
    /// this interface used to notify when an item change in collection
    /// Added, Moved, Changed, Deleted
    /// </summary>
    public interface ICollectionChanged
    {
        event EventHandler<CollectionChangedEventArgs> CollectionChanged;
    }

    /// <summary>
    /// each enum is in the same order as <see cref="NotifyCollectionChangedAction"/> from framework 3.0
    /// 
    /// </summary>
    public enum CollectionChangedAction {
        /// <summary>
        /// One or more items were added to the collection.
        /// </summary>
        Add,
        /// <summary>
        /// One or more items were moved within the collection.  
        /// </summary>
        Move,
        /// <summary>
        /// One or more items were removed from the collection.  
        /// </summary>
        Remove,
        /// <summary>
        /// One or more items were replaced in the collection.  
        /// </summary>
        Replace, 
        /// <summary>
        /// The content of the collection changed dramatically.  
        /// </summary>
        Reset 
    }

    public class CollectionChangedEventArgs : EventArgs
    {
        private CollectionChangedAction _Action;

        public CollectionChangedEventArgs(CollectionChangedAction action, int newindex, int oldindex)
        {
            _Action = action;
            _NewIndex = newindex;
            _OldIndex = oldindex;
        }

        public CollectionChangedAction Action
        {
            get { return _Action; }
        }

        private int _OldIndex;

        public int OldIndex
        {
            get { return _OldIndex; }
        }

        private int _NewIndex;

        public int NewIndex
        {
            get { return _NewIndex; }
        }
    }
}
