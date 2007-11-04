using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;

namespace GeniusBinding.Core
{
    /// <summary>
    /// interface représentant le lien entre un propriété source et une propriété destination
    /// </summary>
    interface IRealBinding
    {
        void Bind(object source, PropertyInfo piSource, object destination, PropertyInfo piDest, IBinderConverter converter, SynchronizationContext applyBindingContext);
        void UnBind();
        void SetDestination(object destination);
        void ForceUpdate();
    }

    /// <summary>
    /// classe concrête
    /// </summary>
    /// <typeparam name="TValueSource"></typeparam>
    /// <typeparam name="TValueDest"></typeparam>
    class RealBinding<TValueSource, TValueDest> : IRealBinding
    {
        WeakReference weakSrc;
        WeakReference weakDst;
        SetHandlerDelegate<TValueSource> sethandler;
        SetHandlerDelegate<TValueDest> sethandlerDest;
        GetHandlerDelegate<TValueSource> gethandler;
        private string _PropNameSource;
        private string _PropNameDest;
        IBinderConverter _Converter;
        OnChangeDelegate<TValueSource> _CurrentChanged;

        public void Bind(object source, PropertyInfo piSource, object destination, 
            PropertyInfo piDest, 
            IBinderConverter converter, 
            SynchronizationContext applyBindingContext)
        {
            weakSrc = new WeakReference(source);
            weakDst = new WeakReference(destination);
            _Converter = converter;
            _PropNameSource = piSource.Name;
            _PropNameDest = piDest.Name;
            gethandler = GetSetUtils.CreateGetHandler<TValueSource>(piSource);

            //le set handler devrait être sur le type destination

            if (_Converter != null)
            {
                _CurrentChanged = OnValueChanged1;
                sethandlerDest = GetSetUtils.CreateSetHandler<TValueDest>(piDest);
            }
            else
            {
                _CurrentChanged = OnValueChanged;
                sethandler = GetSetUtils.CreateSetHandler<TValueSource>(piDest);
            }

            DataBinder.AddNotify<TValueSource>(source, piSource.Name, gethandler, _CurrentChanged, applyBindingContext);
        }

        void OnValueChanged(TValueSource value)
        {
            if (weakDst != null && weakDst.IsAlive)
            {
                try
                {
                    sethandler(weakDst.Target, value);
                }
                catch (Exception ex)
                {
                    throw new CompiledBindingException(string.Format("Set value on property '{0}' failed", _PropNameDest), ex);
                }
            }
        }

        void OnValueChanged1(TValueSource value)
        {
            if (weakDst != null && weakDst.IsAlive)
            {
                if (_Converter != null)
                {
                    IBinderConverter<TValueDest, TValueSource> cv = _Converter as IBinderConverter<TValueDest, TValueSource>;
                    if (cv == null)
                        throw new Exception(string.Format("converter must implement 'IBinderConverter<{0},{1}>'", typeof(TValueDest), typeof(TValueSource)));
                    try
                    {
                        sethandlerDest(weakDst.Target, cv.Convert(value));
                    }
                    catch (Exception ex)
                    {
                        throw new CompiledBindingException(string.Format("Set value on property '{0}' failed", _PropNameDest), ex);
                    }
                }
            }
        }

        public void UnBind()
        {
            DataBinder.RemoveNotify(weakSrc.Target, _PropNameSource, _CurrentChanged);
        }
        
        public void SetDestination(object destination)
        {
            weakDst.Target = destination;
        }

        public void ForceUpdate()
        {
            TValueSource value;
            if (weakSrc.IsAlive && weakDst.IsAlive && weakDst.Target != null)
            {
                try
                {
                    value = gethandler(weakSrc.Target);
                }
                catch (Exception ex)
                {
                    throw new CompiledBindingException(string.Format("Get value on property '{0}' failed", _PropNameSource), ex);
                }
                _CurrentChanged(value);
            }
        }
    }

    abstract class CollectionWrapper<T, TIndex>
    {
        TIndex _Index;

        public CollectionWrapper(TIndex index)
        {
            _Index = index;
        }

        protected TIndex Index
        {
            get
            {
                return _Index;
            }
        }

        public abstract T ArrayValue { get; set;}
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">typeof item at the specified index</typeparam>
    class ArrayWrapper<T> : CollectionWrapper<T, int>, IDisposable
    {
        WeakReference _originalCollection;
        IList<T> _TypedList;
        IList _UnTypedcollection;
        //object _originalCollection;
        CollectionChangedDelegate _onCollectionChanged;

        public ArrayWrapper(object collection, int index, CollectionChangedDelegate onCollectionChanged) : base (index)
        {
            Check.IsNotNull("collection", collection);
            _UnTypedcollection = collection as IList;
            _TypedList = collection as IList<T>;           
            if (_UnTypedcollection == null && _TypedList == null)
                throw new Exception("unknown collection : " + collection.GetType().ToString());
            _onCollectionChanged = onCollectionChanged;
            if (collection is ICollectionChanged)
            {
                ((ICollectionChanged)collection).CollectionChanged += new EventHandler<CollectionChangedEventArgs>(ArrayWrapper_CollectionChanged);
            }
            else if (collection is IBindingList)
            {
                ((IBindingList)collection).ListChanged += new ListChangedEventHandler(ArrayWrapper_ListChanged);
            }
            _originalCollection = new WeakReference(collection);
            //_originalCollection = collection;
        }

        void ArrayWrapper_CollectionChanged(object sender, CollectionChangedEventArgs e)
        {
            if (e.NewIndex == Index || e.OldIndex == Index)
            {
                if (_onCollectionChanged != null)
                {
                    _onCollectionChanged();
                }
            }
        }

        void ArrayWrapper_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (e.NewIndex == Index || e.OldIndex == Index)
            {
                if (_onCollectionChanged != null)
                {
                    _onCollectionChanged();
                }
            }
        }

        public override T ArrayValue
        {
            get
            {
                if (_TypedList != null)
                {
                    return _TypedList[Index];
                }
                return (T)_UnTypedcollection[Index];
            }
            set
            {
                if (_TypedList != null)
                {
                    _TypedList[Index] = value;
                }
                else
                    _UnTypedcollection[Index] = value;
            }
        }

        ~ArrayWrapper()
        {
            Debug.WriteLine("~ArrayWrapper()");
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (_originalCollection != null && _originalCollection.IsAlive)
            {
                if (_originalCollection.Target is ICollectionChanged)
                {
                    ((ICollectionChanged)_originalCollection.Target).CollectionChanged -= ArrayWrapper_CollectionChanged;
                }
                else if (_originalCollection is IBindingList)
                {
                    ((IBindingList)_originalCollection.Target).ListChanged -= new ListChangedEventHandler(ArrayWrapper_ListChanged);
                }
            }
        }

        #endregion
    }
}
