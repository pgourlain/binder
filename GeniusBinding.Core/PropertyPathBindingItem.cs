using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.ComponentModel;
using System.Diagnostics;
using System.Collections;
using System.Threading;

namespace GeniusBinding.Core
{
    /// <summary>
    /// Represent a binding
    /// </summary>
    class PropertyPathBindingItem : IPropertyPathBinding
    {
        /// <summary>
        /// le binding entre la dernière section de la source, avec la dernière section de la destination
        /// </summary>
        IRealBinding _CurrentBinding;
        /// <summary>
        /// le converter à utiliser lors du binding
        /// </summary>
        IBinderConverter _Converter;

        OnePropertyPathBinding _SourceBinding;
        OnePropertyPathBinding _DestinationBinding;

        SynchronizationContext _ApplyBindingContext;

        /// <summary>
        /// constructeur par défaut
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyPath"></param>
        /// <param name="destination"></param>
        /// <param name="propertyPathdest"></param>
        /// <param name="converter"></param>
        public PropertyPathBindingItem(object sourceTarget, string propertyPath, 
                                        object destinationTarget, string propertyPathdest, 
                                        IBinderConverter converter, 
                                        SynchronizationContext applyBindingContext)
        {
            _ApplyBindingContext = applyBindingContext;
            _Converter = converter;
            _DestinationBinding = new OnePropertyPathBinding();
            _SourceBinding = new OnePropertyPathBinding();

            _DestinationBinding.Bind(destinationTarget, propertyPathdest,
                delegate(int currentIndex, EqualityWeakReference weakSource)
                {
                    //this delegate it's  : "What to do when an intermediate property changing"
                    return delegate(object value)
                    {
                        if (weakSource.IsAlive)
                        {
                            _DestinationBinding.RemoveNotify(currentIndex);
                            _DestinationBinding.BindPropertyPath(value, currentIndex);
                            if (_CurrentBinding != null)
                            {
                                RecreateRealBinding();
                            }
                            else
                            {
                                //rebind all level from source
                                _SourceBinding.RemoveNotify(0);
                                _SourceBinding.BindPropertyPath(this.Source, 0);
                            }
                        }
                    };
                },
                null
                //delegate(int index, PropertyInfo pi, object src)
                //{
                //    ////Cas de la propriété à bindée
                //    //EqualityWeakReference weakDest = new EqualityWeakReference(src);
                //    //PathItem p = _DestinationBinding.Items[index];
                //    //p.Source = weakDest;
                //}
            );

            ///Connect to the source
            _SourceBinding.Bind(sourceTarget, propertyPath,
                delegate(int currentIndex, EqualityWeakReference weakSource)
                {
                    //this delegate it's  : "What to do when an intermediate property changing"
                    return delegate(object value)
                    {
                        if (weakSource.IsAlive)
                        {
                            _SourceBinding.RemoveNotify(currentIndex);
                            InternalUnBind();
                            _SourceBinding.BindPropertyPath(value, currentIndex);
                        }
                    };
                },
                //this delegate it's  : "What to do when the final property changing"
                delegate(int index, PropertyInfo pi, object src)
                {
                    RecreateRealBinding();
                });
        }

        /// <summary>
        /// Recreate the link between final source property and final destination property
        /// </summary>
        private void RecreateRealBinding()
        {
            if (_CurrentBinding != null)
                _CurrentBinding.UnBind();
            _CurrentBinding = null;

            PathItem destPathItem = _DestinationBinding.LastItem;
            //last item path (property to bind)
            if (destPathItem == null || destPathItem.Source == null)
                return;
            PathItem srcPathItem = _SourceBinding.LastItem;
            if (srcPathItem == null || srcPathItem.Source == null)
                return;

            srcPathItem.ArrayWrapper = null;

            object source, destination;
            PropertyInfo piSource, piDestination;
            source = srcPathItem.Source.Target;
            destination = destPathItem.Source.Target;
            bool arrayWrapperIsValid;
            //special case if it's an array
            if (srcPathItem.IsArray)
            {
                piSource = null;
                srcPathItem.ArrayWrapper = CreateArrayWrapper(srcPathItem, srcPathItem, out arrayWrapperIsValid, delegate()
                {
                    if (_CurrentBinding != null)
                        _CurrentBinding.ForceUpdate();
                    else
                        RecreateRealBinding();
                });
                if (srcPathItem.ArrayWrapper == null)
                    return;
                piSource = srcPathItem.ArrayWrapper.GetType().GetProperty("ArrayValue");
                source = srcPathItem.ArrayWrapper;
                if (!arrayWrapperIsValid)
                    return;
            }
            else
            {
                piSource = source.GetType().GetProperty(srcPathItem.PropertyName);
            }

            if (destPathItem.IsArray)
            {                
                piDestination = null;
                destPathItem.ArrayWrapper = CreateArrayWrapper(srcPathItem, destPathItem, out arrayWrapperIsValid, null);
                if (destPathItem.ArrayWrapper == null)
                    return;
                piDestination = destPathItem.ArrayWrapper.GetType().GetProperty("ArrayValue");
                destination = destPathItem.ArrayWrapper;
            }
            else
            {
                piDestination = destination.GetType().GetProperty(destPathItem.PropertyName);
            }


            Type t = typeof(RealBinding<,>);
            Type realBindingType = t.MakeGenericType(piSource.PropertyType, piDestination.PropertyType);
            _CurrentBinding = (IRealBinding)Activator.CreateInstance(realBindingType);
            _CurrentBinding.Bind(source, piSource, destination, piDestination, _Converter, _ApplyBindingContext);
            _CurrentBinding.ForceUpdate();
        }

        #region case for array a last position in PathItem
        private object CreateArrayWrapper(PathItem srcPathItem, PathItem destPathItem, out bool isValid, CollectionChangedDelegate onCollectionChanged)
        {
            isValid = true;
            PropertyInfo pi = destPathItem.Source.Target.GetType().GetProperty(destPathItem.PropertyName);
            object collection = pi.GetValue(destPathItem.Source.Target, null);

            Type srcType;
            if (srcPathItem.IsArray)
                srcType = GetArrayItemPropertyType(srcPathItem);
            else
            {
                srcType = srcPathItem.Source.Target.GetType().GetProperty(srcPathItem.PropertyName).PropertyType;
            }
            if (srcType == null)
            {
                srcType = typeof(object);
                isValid = false;
            }

            Type tArrayWrapper = typeof(ArrayWrapper<>).MakeGenericType(srcType);
            object aw = Activator.CreateInstance(tArrayWrapper, collection, destPathItem.ArrayIndex, onCollectionChanged);

            return aw;
        }

        private Type GetArrayItemPropertyType(PathItem pathItem)
        {
            PropertyInfo pi = pathItem.Source.Target.GetType().GetProperty(pathItem.PropertyName);
            object collection = pi.GetValue(pathItem.Source.Target, null);
            
            object itemArrayValue = ArrayUtil.GetItem(collection, pathItem.ArrayIndex);
            if (itemArrayValue == null)
                return null;
            return itemArrayValue.GetType();
        }
        #endregion

        #region IPropertyPathBinding Members

        public void UnBind()
        {
            InternalUnBind();
            DataBinder.RemoveBinding(this);
        }

        /// <summary>
        /// path of the property of source to bind (xxx.yyy.zzz etc.)
        /// </summary>
        public string PropertyPathSource
        {
            get
            {
                return _SourceBinding.PropertyPath;
                //return _PropertyPath;
            }
        }

        /// <summary>
        /// path of the property of destination to bind (xxx.yyy.zzz etc.)
        /// </summary>
        public string PropertyPathDestination
        {
            get
            {
                return _DestinationBinding.PropertyPath;
                //return _PropertyPathDest;
            }
        }

        /// <summary>
        /// Enabled or disable binding
        /// </summary>
        public bool Enabled
        {
            get 
            {
                object source = _SourceBinding.Source.Target;
                IOneNotify prop = DataBinder.GetOneNotify(source, _SourceBinding.LastPathItem);
                if (prop != null)
                    return prop.Enabled;
                return false;
            }
            set 
            {
                object source = _SourceBinding.Source.Target;
                IOneNotify prop = DataBinder.GetOneNotify(source, _SourceBinding.LastPathItem);
                if (prop != null)
                    prop.Enabled = value;
            }
        }


        public object Source
        {
            get { return _SourceBinding.Source.Target; }
        }

        public object Destination
        {
            get { return _DestinationBinding.Source.Target; }
        }

        public bool IsAlive 
        {
            get
            {
                return _SourceBinding.Source.IsAlive && _DestinationBinding.Source.IsAlive;
            }
        }

        private void FillInfos(string title, StringBuilder sb, OnePropertyPathBinding binding)
        {
            if (binding != null)
            {
                sb.Append(title);
                foreach (PathItem item in binding.Items)
                {
                    sb.AppendFormat("{0}.", item.IsBind ? item.PropertyName : "?");
                }
            }
        }

        public string GetStateAsString()
        {
            StringBuilder sb = new StringBuilder();
            if (_SourceBinding != null)
            {
                FillInfos("Source :", sb, _SourceBinding);
                sb.Append(",");
            }
            else
                sb.Append("Source : Null,");
            if (_DestinationBinding != null)
            {
                FillInfos("Destination :", sb, _DestinationBinding);
            }
            else
                sb.Append("Destination : Null");
            return sb.ToString();
        }

        public PropertyPathBindingState State
        {
            get
            {
                PropertyPathBindingState Result = PropertyPathBindingState.Ok;
                if (_CurrentBinding != null)
                    return Result;
                if (_DestinationBinding == null || !_DestinationBinding.IsBind)
                    Result = PropertyPathBindingState.DestinationNotBind;
                if (_SourceBinding == null || !_SourceBinding.IsBind)
                    Result = PropertyPathBindingState.SourceNotBind;
                return Result;
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
        #endregion
    }

}
