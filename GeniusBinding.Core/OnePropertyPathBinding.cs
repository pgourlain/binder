using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Collections;

namespace GeniusBinding.Core
{
    delegate OnChangeDelegate<object> OnChangeDelegateFactoryDelegate(int currentIndex, EqualityWeakReference weakSource);
    delegate void OnBindLastItem(int index, PropertyInfo pi, object source);
    delegate void CollectionChangedDelegate();


    /// <summary>
    /// Represents source or destination path binding
    /// </summary>
    class OnePropertyPathBinding
    {
        private string _PropertyPath;
        /// <summary>
        /// object source
        /// </summary>
        EqualityWeakReference _Source;
        /// <summary>
        /// references of each property of source path 
        /// </summary>
        List<PathItem> _Items;

        OnChangeDelegateFactoryDelegate _factory;
        OnBindLastItem _OnfinalBind;
        public event EventHandler OnUnbind;

        public string LastPathItem
        {
            get
            {
                return _Items[_Items.Count - 1].PropertyName;
            }
        }

        public List<PathItem> Items
        {
            get
            {
                return _Items;
            }
        }

        public PathItem LastItem
        {
            get
            {
                return _Items[_Items.Count - 1];
            }
        }

        public string PropertyPath
        {
            get
            {
                return _PropertyPath;
            }
        }

        public EqualityWeakReference Source
        {
            get
            {
                return _Source;
            }
        }

        public bool IsBind
        {
            get
            {
                bool Result = true;
                foreach (PathItem item in _Items)
                {
                    Result &= item.IsBind;
                }
                return Result;
            }
        }

        public OnePropertyPathBinding()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rootSource"></param>
        /// <param name="fullpath"></param>
        /// <param name="factory"></param>
        /// <param name="OnfinalBind"></param>
        public void Bind(object rootSource, string fullpath, OnChangeDelegateFactoryDelegate factory, OnBindLastItem OnfinalBind)
        {
            _Items = new List<PathItem>();
            _PropertyPath = fullpath;
            _Source = new EqualityWeakReference(rootSource);
            List<string> pahtitems = new List<string>(fullpath.Split('.'));
            int i = -1;
            while (++i < pahtitems.Count)
            {
                string pathitem = pahtitems[i];
                if (pathitem.EndsWith("]"))
                {
                    pathitem = pathitem.TrimEnd(']');
                    PathItem p = new PathItem(pathitem.Substring(0, pathitem.IndexOf('[')));
                    p.Index = i;
                    _Items.Add(p);
                    string arrayIndex = pathitem.Substring(pathitem.IndexOf('[') + 1);
                    p.IsArray = true;
                    p.ArrayIndex = int.Parse(arrayIndex);
                }
                else
                {
                    PathItem p = new PathItem(pathitem);
                    p.Index = i;
                    _Items.Add(p);
                }
            }
            _OnfinalBind = OnfinalBind;
            _factory = factory;
            BindPropertyPath(rootSource, 0);
        }

        public void RemoveNotify(int currentIndex)
        {
            RemoveNotify(currentIndex, _Items);
        }

        public void BindPropertyPath(object source, int index)
        {
            if (source == null)
                return;
            PathItem pathitem = _Items[index];
            bool isArray = pathitem.IsArray;

            PropertyInfo pi = source.GetType().GetProperty(pathitem.PropertyName);
            if (pi == null)
                CheckPropertyInfo(true, _PropertyPath, pathitem.PropertyName, source.GetType());
            if (index == _Items.Count - 1)
            {
                pathitem.Source = new EqualityWeakReference(source);
                if (_OnfinalBind != null)
                    _OnfinalBind(index, pi, source);
                pathitem.IsBind = true;
                if (pathitem.IsArray && _factory != null)
                {
                    OnChangeDelegate<object> onchanged = delegate(object value)
                    {
                        if (pathitem.Source.IsAlive)
                        {
                            object target = pathitem.Source.Target;
                            RemoveNotify(index);
                            BindPropertyPath(target, index);
                        }
                    };
                    pathitem.OnChanged = onchanged;
                    //Add notification on source changed
                    AddNotify(source, pathitem.PropertyName, onchanged);
                }
            }
            else
            {
                EqualityWeakReference weakSource = new EqualityWeakReference(source);
                int currentIndex = index + 1;

                OnChangeDelegate<object> onchanged = null;
                if (_factory != null)
                    onchanged = _factory(currentIndex, weakSource);
                pathitem.Source = weakSource;
                pathitem.OnChanged = onchanged;
                pathitem.IsBind = true;
                //Add notification on source changed
                AddNotify(source, pathitem.PropertyName, onchanged);
                source = pi.GetValue(source, null);
                if (isArray)
                {
                    source = BindToArray(source, currentIndex, pathitem);
                }
                BindPropertyPath(source, currentIndex);
            }
        }

        private object BindToArray(object source, int currentIndex, PathItem pathitem)
        {
            //in future use, it will be insterresting to keep arrayIndex as string (i.e dictionary) 
            int intArrayIndex = pathitem.ArrayIndex;

            EqualityWeakReference weakSource;
            //Here source is a list, bindinglist, ... or has simply indexer
            if (source is IBindingList)
            {
                BindingListNotificationWrapper wrapper = new BindingListNotificationWrapper((IBindingList)source);
                wrapper.SetIndex(intArrayIndex);
                weakSource = new EqualityWeakReference(source);
                wrapper.CollectionChanged += delegate(object sender, CollectionChangedEventArgs e)
                {
                    if (weakSource.IsAlive)
                    {
                        UnBindReBindListItem(currentIndex, intArrayIndex, weakSource.Target);
                    }
                };
                IList l = ((IList)source);
                if (l.Count > intArrayIndex)
                    source = l[intArrayIndex];
                else
                    source = null;
                pathitem.ArrayWrapper = wrapper;
            }
            else if (source is ICollectionChanged)
            {
                weakSource = new EqualityWeakReference(source);
                ((ICollectionChanged)source).CollectionChanged += delegate(object sender, CollectionChangedEventArgs e)
                {
                    if (weakSource.IsAlive)
                    {
                        if (e.Action == CollectionChangedAction.Reset || e.NewIndex == intArrayIndex || e.OldIndex == intArrayIndex)
                        {
                            UnBindReBindListItem(currentIndex, intArrayIndex, weakSource.Target);
                        }
                    }
                };
                if (((IList)source).Count > intArrayIndex)
                    source = ((IList)source)[intArrayIndex];
                else
                    source = null;
            }
            else
            {
                source = null;
            }
            return source;
        }

        private void UnBindReBindListItem(int currentIndex, int intArrayIndex, object listSource)
        {
            RemoveNotify(currentIndex, _Items);
            InternalUnBind();
            object value = null;
            if (listSource is IList)
            {
                if (((IList)listSource).Count > intArrayIndex)
                    value = ((IList)listSource)[intArrayIndex];
            }
            else if (listSource is IBindingList)
            {
                if (((IBindingList)listSource).Count > intArrayIndex)
                    value = ((IBindingList)listSource)[intArrayIndex];
            }
            BindPropertyPath(value, currentIndex);
        }

        private void InternalUnBind()
        {
            if (OnUnbind != null)
                OnUnbind(this, EventArgs.Empty);
        }

        private void CheckPropertyInfo(bool isSource, string propertypath, string pathitem, Type type)
        {
            string format = isSource ? "source" : "destination";
            format += ": invalid propertypath '{0}', property '{1}' doesn't exist in Type : {2}";
            throw new CompiledBindingException(string.Format(format, propertypath, pathitem, type.FullName));
        }

        private void RemoveNotify(int currentIndex, List<PathItem> items)
        {
            for (int i = currentIndex; i < items.Count; i++)
            {
                PathItem item = items[i];
                item.ArrayWrapper = null;
                item.IsBind = false;
                if (item != null && item.OnChanged != null)
                    DataBinder.RemoveNotify(item.Source.Target, item.PropertyName, item.OnChanged);
                item.Source = null;
            }
        }

        /// <summary>
        /// it for intermediate notifications, to rebind
        /// </summary>
        /// <param name="current">concerned object</param>
        /// <param name="pathitem">real property name</param>
        /// <param name="onchanged">delegate to call, when propertyname changed</param>
        private void AddNotify(object current, string pathitem, OnChangeDelegate<object> onchanged)
        {
            PropertyInfo pi = current.GetType().GetProperty(pathitem);

            GetHandlerDelegate<object> gethandler = GetSetUtils.CreateGetHandler<object>(pi);
            DataBinder.AddNotify<object>(current, pathitem, gethandler, onchanged, null);
        }

    }
}
