using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;

namespace GeniusBinding.Core
{
    /// <summary>
    /// Représente un binding
    /// </summary>
    class PropertyPathBindingItem : IPropertyPathBinding
    {
        /// <summary>
        /// Path complet pour la source
        /// </summary>
        private string _PropertyPath;
        /// <summary>
        /// path complet pour la destination
        /// </summary>
        private string _PropertyPathDest;

        class PathItem
        {
            public PathItem(EqualityWeakReference source, int index, Delegate onchanged)
            {
                Source = source;
                Index = index;
                OnChanged = onchanged;
            }

            public EqualityWeakReference Source;
            public Delegate OnChanged;
            public int Index;
        }

        /// <summary>
        /// objet source
        /// </summary>
        EqualityWeakReference _Source;
        /// <summary>
        /// objet destination
        /// </summary>
        EqualityWeakReference _Destination;
        /// <summary>
        /// toute les propriétés du path source "splitées"
        /// </summary>
        string[] _PathItems;
        /// <summary>
        /// les objets référence chaque propriété du path
        /// </summary>
        PathItem[] _Items;

        string[] _DestPathItems;
        PathItem[] _DestItems;
        /// <summary>
        /// le binding entre la dernière section de la source, avec la dernière section de la destination
        /// </summary>
        IRealBinding _CurrentBinding;
        /// <summary>
        /// le converter à utiliser lors du binding
        /// </summary>
        IBinderConverter _Converter;

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyPath"></param>
        /// <param name="destination"></param>
        /// <param name="propertyPathdest"></param>
        /// <param name="converter"></param>
        public PropertyPathBindingItem(object source, string propertyPath, object destination, string propertyPathdest, IBinderConverter converter)
        {
            _PropertyPath = propertyPath;
            _PropertyPathDest = propertyPathdest;
            _Converter = converter;
            _Destination = new EqualityWeakReference(destination);
            _DestPathItems = propertyPathdest.Split('.');
            _DestItems = new PathItem[_DestPathItems.Length];
            BindPropertyPathDest(destination, 0);

            _Source = new EqualityWeakReference(source);
            _PathItems = propertyPath.Split('.');
            _Items = new PathItem[_PathItems.Length];
            BindPropertyPath(source, 0);
        }

        #region IPropertyPathBinding Members

        public void UnBind()
        {
            InternalUnBind();
            DataBinder.RemoveBinding(this);
        }

        public string PropertyPathSource
        {
            get
            {
                return _PropertyPath;
            }
        }

        public string PropertyPathDestination
        {
            get
            {
                return _PropertyPathDest;
            }
        }

        public bool Enabled
        {
            get 
            {
                IOneNotify prop = DataBinder.GetOneNotify(this._Source.Target, _PathItems[_PathItems.Length - 1]);
                if (prop != null)
                    return prop.Enabled;
                return false;
            }
            set 
            {
                IOneNotify prop = DataBinder.GetOneNotify(this._Source.Target, _PathItems[_PathItems.Length - 1]);
                if (prop != null)
                    prop.Enabled = value;
            }
        }


        public object Source
        {
            get { return _Source.Target; }
        }

        public object Destination
        {
            get { return _Destination.Target; }
        }

        public bool IsAlive 
        {
            get
            {
                return _Source.IsAlive && _Destination.IsAlive;
            }
        }

        #endregion

        #region méthodes privées
        private void InternalUnBind()
        {
            if (_CurrentBinding != null)
            {
                _CurrentBinding.UnBind();
                _CurrentBinding = null;
            }
        }

        private void BindPropertyPathDest(object destination, int index)
        {
            string pathitem = _DestPathItems[index];
            PropertyInfo pi = destination.GetType().GetProperty(pathitem);
            if (pi == null)
                CheckPropertyInfo(false, _PropertyPathDest, pathitem, destination.GetType());
            if (index == _DestPathItems.Length - 1)
            {
                //Cas de la propriété à bindée
                EqualityWeakReference weakDest = new EqualityWeakReference(destination);
                PathItem p = new PathItem(weakDest, index, null);
                _DestItems[index] = p;
            }
            else
            {
                EqualityWeakReference weakDest = new EqualityWeakReference(destination);
                int currentIndex = index + 1;
                OnChangeDelegate<object> onchanged = delegate(object value)
                {
                    if (weakDest.IsAlive)
                    {
                        RemoveNotify(currentIndex, _DestPathItems, _DestItems);
                        BindPropertyPathDest(value, currentIndex);
                        _CurrentBinding.SetDestination(_DestItems[_DestPathItems.Length - 1].Source.Target);

                    }
                };
                _DestItems[index] = new PathItem(weakDest, index, onchanged);
                AddNotify(destination, pathitem, onchanged);
                destination = pi.GetValue(destination, null);
                BindPropertyPathDest(destination, currentIndex);
            }
        }

        private void BindPropertyPath(object source, int index)
        {
            if (source == null)
                return;
            string pathitem = _PathItems[index];
            PropertyInfo pi = source.GetType().GetProperty(pathitem);
            if (pi == null)
                CheckPropertyInfo(true,_PropertyPath, pathitem, source.GetType());
            if (index == _PathItems.Length - 1)
            {
                object destination = _DestItems[_DestPathItems.Length-1].Source.Target;
                PropertyInfo piSource = pi;
                PropertyInfo piDest = destination.GetType().GetProperty(_DestPathItems[_DestPathItems.Length - 1]);
                Type t = typeof(RealBinding<,>);
                Type realBindingType = t.MakeGenericType(piSource.PropertyType, piDest.PropertyType);
                _CurrentBinding = (IRealBinding)Activator.CreateInstance(realBindingType);
                _CurrentBinding.Bind(source, piSource, destination, piDest, _Converter);
            }
            else
            {
                EqualityWeakReference weakSource = new EqualityWeakReference(source);
                int currentIndex = index + 1;
                OnChangeDelegate<object> onchanged =delegate(object value)
                {
                    if (weakSource.IsAlive)
                    {
                        RemoveNotify(currentIndex, _PathItems, _Items);
                        InternalUnBind();
                        BindPropertyPath(value, currentIndex);
                    }
                };
                _Items[index] = new PathItem(weakSource, index, onchanged);
                AddNotify(source, pathitem, onchanged);
                source = pi.GetValue(source, null);
                BindPropertyPath(source, currentIndex);
            }
        }

        private void CheckPropertyInfo(bool isSource, string propertypath, string pathitem, Type type)
        {
            string format = isSource ? "source" : "destination";
            format += ": invalid propertypath '{0}', property '{1}' doesn't exist in Type : {2}";
            throw new CompiledBindingException(string.Format(format, propertypath, pathitem, type.FullName));
        }

        private void RemoveNotify(int currentIndex, string[] pathitems, PathItem[] items)
        {
            for (int i = currentIndex; i < pathitems.Length; i++)
            {
                PathItem item = items[i];
                if (item != null)
                    DataBinder.RemoveNotify(item.Source.Target, pathitems[item.Index], item.OnChanged);
            }
        }

        private void AddNotify(object current, string pathitem, OnChangeDelegate<object> onchanged)
        {
            PropertyInfo pi = current.GetType().GetProperty(pathitem);

            GetHandlerDelegate<object> gethandler = GetSetUtils.CreateGetHandler<object>(pi);
            DataBinder.AddNotify<object>(current, pathitem, gethandler, onchanged);
        }
        #endregion
    }

}
