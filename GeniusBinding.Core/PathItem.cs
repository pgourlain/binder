using System;
using System.Collections.Generic;
using System.Text;

namespace GeniusBinding.Core
{
    class PathItem
    {

        public PathItem(string propName)
        {
            PropertyName = propName;
        }

        public PathItem(EqualityWeakReference source, int index, Delegate onchanged)
        {
            Source = source;
            Index = index;
            OnChanged = onchanged;
            IsArray = false;
        }

        public PathItem(EqualityWeakReference source, int index, Delegate onchanged, bool isArray, int arrayIndex)
        {
            Source = source;
            Index = index;
            OnChanged = onchanged;
            IsArray = isArray;
            ArrayIndex = arrayIndex;
        }
        /// <summary>
        /// property name to 'bind'
        /// </summary>
        public string PropertyName;
        /// <summary>
        /// concerned object
        /// </summary>
        public EqualityWeakReference Source;
        public Delegate OnChanged;
        /// <summary>
        /// index in PathItem list 
        /// </summary>
        public int Index;
        public bool IsArray;
        public int ArrayIndex;
        object _ArrayWrapper;
        public object ArrayWrapper
        {           
            get
            {
                return _ArrayWrapper;
            }
            set
            {
                if (_ArrayWrapper != null && _ArrayWrapper is IDisposable)
                    ((IDisposable)_ArrayWrapper).Dispose();
                _ArrayWrapper = value;
            }
        }

        public bool IsBind;
    }
}
