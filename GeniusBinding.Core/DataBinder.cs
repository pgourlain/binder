using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Reflection.Emit;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Threading;

namespace GeniusBinding.Core
{
    /// <summary>
    /// Used by <see cref="DataBinder.CreateConverter"/> to converter a delegate to <see cref="IBinderConverter"/>
    /// </summary>
    /// <typeparam name="TResult">type of result conversion</typeparam>
    /// <typeparam name="TValue">type of value to convert</typeparam>
    /// <param name="value">value to convert</param>
    /// <returns>the converted value</returns>
    public delegate TResult BinderConverterDelegate<TResult, TValue>(TValue value);

    /// <summary>
    /// deleguée utilisée, pour réaliser un "OnChanged" avec la nouvelle valeur
    /// </summary>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="value"></param>
    internal delegate void OnChangeDelegate<TValue>(TValue value);

    public class DataBinder
    {
        #region gestion de la notification d'une propriété
        /// <summary>
        /// classe générique pour le typage fort
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class OneNotify<T> : IOneNotify
        {
            private bool _HasCalled;
            GetHandlerDelegate<T> _OnGet;
            public OnChangeDelegate<T> _OnChanged;
            /// <summary>
            /// name of 'listen' property 
            /// </summary>
            public string PropertyName = string.Empty;
            private bool _BindingEnabled = true;
            /// <summary>
            /// handler when event "PropertyName"Changed exists
            /// </summary>
            public EventHandler _PropertyChangedEvent;

            public SynchronizationContext _ApplyBindingContext;

            public OneNotify(GetHandlerDelegate<T> getter, OnChangeDelegate<T> onchanged)
            {
                _OnGet = getter;
                _OnChanged = onchanged;
            }

            #region IOneNotify
            public void Fire(WeakReference weak)
            {
                if (weak.IsAlive && _BindingEnabled)
                {
                    if (_HasCalled)
                        throw new Exception(string.Format("The current binding has recursion =>{0}('{1}')", weak.Target.GetType().FullName, PropertyName));
                    _HasCalled = true;
                    try
                    {
                        
                        if (_ApplyBindingContext != null)
                        {
                            _ApplyBindingContext.Send(DoNotifyValueChanged, weak.Target);
                        }
                        else
                        {
                            DoNotifyValueChanged(weak.Target);
                        }
                    }
                    finally
                    {
                        _HasCalled = false;
                    }
                }
            }

            private void DoNotifyValueChanged(object target)
            {
                T Value;
                try
                {
                    Value = _OnGet(target);
                }
                catch (Exception ex)
                {
                    throw new CompiledBindingException(string.Format("Get value on property '{0}' failed", PropertyName), ex);
                }
                _OnChanged(Value);
            }

            public bool Enabled
            {
                get
                {
                    return _BindingEnabled;
                }
                set
                {
                    _BindingEnabled = value;
                }
            }

            public void Dispose()
            {
                _OnChanged = null;
            }
            public Delegate OnChanged
            {
                get { return _OnChanged; }
                set
                {
                    _OnChanged = (OnChangeDelegate<T>)value;
                }
            }
            public EventHandler PropertyChangedEvent
            {
                get
                {
                    return _PropertyChangedEvent;
                }
            } 
            #endregion
        }

        #endregion

        /// <summary>
        /// liste des abonnements aux sources de données
        /// </summary>
        static Dictionary<WeakReference, Dictionary<string, IOneNotify>> _SourceChanged = new Dictionary<WeakReference, Dictionary<string, IOneNotify>>();
        static List<IPropertyPathBinding> _Bindings = new List<IPropertyPathBinding>();
        
        #region notification
        internal static IOneNotify GetOneNotify(object source, string propName)
        {
            INotifyPropertyChanged inotify = source as INotifyPropertyChanged;
            if (inotify != null)
            {
                WeakReference weak = new EqualityWeakReference(inotify);
                //le but est ici de ne s'abonner qu'une fois
                Dictionary<string, IOneNotify> dico = null;
                if (_SourceChanged.TryGetValue(weak, out dico))
                {
                    IOneNotify result = null;
                    dico.TryGetValue(propName, out result);
                    return result;
                }
            }
            return null;
        }
        /// <summary>
        /// gestion de la notification sur INotifyPropertyChanged
        /// </summary>
        /// <typeparam name="T">type de la propriété observée</typeparam>
        /// <param name="source">source</param>
        /// <param name="propName">property à observer</param>
        /// <param name="gethandler">"get" a appeler a chaque changements</param>
        /// <param name="OnValueChange">la déléguée à appelée après chaque changed de la source</param>
        internal static void AddNotify<T>(object source, string propName, GetHandlerDelegate<T> gethandler, OnChangeDelegate<T> OnValueChange, SynchronizationContext applyBindingContext)
        {
            INotifyPropertyChanged inotify = source as INotifyPropertyChanged;
            if (inotify != null)
            {
                WeakReference weak = new EqualityWeakReference(inotify);
                //le but est ici de ne s'abonner qu'une fois
                Dictionary<string, IOneNotify> dico = null;
                if (!_SourceChanged.TryGetValue(weak, out dico))
                {
                    dico = new Dictionary<string, IOneNotify>();
                    _SourceChanged.Add(weak, dico);
                    inotify.PropertyChanged += delegate(object sender, PropertyChangedEventArgs e)
                        {
                            //je minimise le nombre de get, en founissant la valeur qui à changé
                            if (dico.ContainsKey(e.PropertyName))
                                dico[e.PropertyName].Fire(weak);
                        };
                }
                ///plusieurs abonnement si nécessaire pour une propriété binding de 1 prop vers plusieurs dest
                OneNotify<T> binding = null;
                if (dico.ContainsKey(propName))
                {
                    binding = (OneNotify<T>)dico[propName];
                    binding._OnChanged = (OnChangeDelegate<T>)Delegate.Combine(binding._OnChanged, OnValueChange);
                }
                else
                {
                    binding = new OneNotify<T>(gethandler, OnValueChange);
                    binding._ApplyBindingContext = applyBindingContext;
                    binding.PropertyName = propName;
                    dico[propName] = binding;
                }
            }
            else if (source != null)
            {
                //try with old mode "PropertyName"Changed event
                string eventName = string.Format("{0}Changed", propName);
                EventInfo evInfo = source.GetType().GetEvent(eventName);
                if (evInfo != null)
                {
                    WeakReference weak = new EqualityWeakReference(source);
                    //le but est ici de ne s'abonner qu'une fois
                    Dictionary<string, IOneNotify> dico = null;
                    if (!_SourceChanged.TryGetValue(weak, out dico))
                    {
                        dico = new Dictionary<string, IOneNotify>();
                        _SourceChanged.Add(weak, dico);
                    }

                    ///plusieurs abonnement si nécessaire pour une propriété binding de 1 prop vers plusieurs dest
                    OneNotify<T> binding = null;
                    if (dico.ContainsKey(propName))
                    {
                        binding = (OneNotify<T>)dico[propName];
                        binding._OnChanged = (OnChangeDelegate<T>)Delegate.Combine(binding._OnChanged, OnValueChange);
                    }
                    else
                    {
                        binding = new OneNotify<T>(gethandler, OnValueChange);
                        binding.PropertyName = propName;
                        binding._ApplyBindingContext = applyBindingContext;
                        binding._PropertyChangedEvent = delegate
                        {
                            binding.Fire(weak);
                        };
                        dico[propName] = binding;
                        evInfo.AddEventHandler(source, binding._PropertyChangedEvent);
                    }
                }
            }
        }

        internal static void RemoveNotify(object source, string propName, Delegate del)
        {
            INotifyPropertyChanged inotify = source as INotifyPropertyChanged;
            if (inotify != null)
            {
                WeakReference weak = new EqualityWeakReference(inotify);
                //le but est ici de ne s'abonner qu'une fois
                Dictionary<string, IOneNotify> dico = null;
                if (_SourceChanged.TryGetValue(weak, out dico))
                {
                    if (dico.ContainsKey(propName))
                    {
                        dico[propName].OnChanged = Delegate.Remove(dico[propName].OnChanged, del);
                        if (dico[propName].OnChanged == null)
                            dico.Remove(propName);
                    }
                }
            }
            else if (source != null)
            {
                //try with old mode "PropertyName"Changed event
                string eventName = string.Format("{0}Changed", propName);
                EventInfo evInfo = source.GetType().GetEvent(eventName);
                if (evInfo != null)
                {
                    WeakReference weak = new EqualityWeakReference(source);
                    //le but est ici de ne s'abonner qu'une fois
                    Dictionary<string, IOneNotify> dico = null;
                    if (_SourceChanged.TryGetValue(weak, out dico))
                    {
                        if (dico.ContainsKey(propName))
                        {
                            dico[propName].OnChanged = Delegate.Remove(dico[propName].OnChanged, del);
                            if (dico[propName].OnChanged == null)
                            {
                                evInfo.RemoveEventHandler(source, dico[propName].PropertyChangedEvent);
                                dico.Remove(propName);
                            }
                        }
                    }
                }
            }
        }

        internal static void RemoveBinding(IPropertyPathBinding binding)
        {
            _Bindings.Remove(binding);
        }
        #endregion

        #region public methods
        public static void AddCompiledBinding(object source, string propertypath, object destination, string destPropertypath)
        {
            Check.IsNotNull("source", source);
            Check.IsNotNull("propertypath", propertypath);
            Check.IsNotNull("destination", destination);
            Check.IsNotNull("destPropertypath", destPropertypath);
            string[] pathItems = propertypath.Split('.');

            
            PropertyPathBindingItem item = new PropertyPathBindingItem(source, propertypath, destination, destPropertypath, null, null);
            _Bindings.Add(item);
        }

        public static void AddCompiledBinding(object source, string propertypath, object destination, string destPropertypath, SynchronizationContext applyBindingContext)
        {
            Check.IsNotNull("source", source);
            Check.IsNotNull("propertypath", propertypath);
            Check.IsNotNull("destination", destination);
            Check.IsNotNull("destPropertypath", destPropertypath);
            string[] pathItems = propertypath.Split('.');


            PropertyPathBindingItem item = new PropertyPathBindingItem(source, propertypath, destination, destPropertypath, null, applyBindingContext);
            _Bindings.Add(item);
        }
        /// <summary>
        /// ajoute un binding
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertypath"></param>
        /// <param name="destination"></param>
        /// <param name="destPropertypath"></param>
        /// <param name="converter"></param>
        public static void AddCompiledBinding(object source, string propertypath, object destination, string destPropertypath, IBinderConverter converter)
        {
            Check.IsNotNull("source", source);
            Check.IsNotNull("propertypath", propertypath);
            Check.IsNotNull("destination", destination);
            Check.IsNotNull("destPropertypath", destPropertypath);
            string[] pathItems = propertypath.Split('.');


            PropertyPathBindingItem item = new PropertyPathBindingItem(source, propertypath, destination, destPropertypath, converter, null);
            _Bindings.Add(item);
        }


        public static void AddCompiledBinding(object source, string propertypath, object destination, string destPropertypath, SynchronizationContext applyBindingContext, IBinderConverter converter)
        {
            Check.IsNotNull("source", source);
            Check.IsNotNull("propertypath", propertypath);
            Check.IsNotNull("destination", destination);
            Check.IsNotNull("destPropertypath", destPropertypath);
            string[] pathItems = propertypath.Split('.');


            PropertyPathBindingItem item = new PropertyPathBindingItem(source, propertypath, destination, destPropertypath, converter, applyBindingContext);
            _Bindings.Add(item);
        }        
      
        /// <summary>
        /// enlève un binding
        /// </summary>
        /// <param name="source">instance de la source concernée</param>
        /// <param name="propertypath"></param>
        /// <param name="destination"></param>
        /// <param name="destPropertypath"></param>
        public static void RemoveCompiledBinding(object source, string propertypath, object destination, string destPropertypath)
        {
            List<IPropertyPathBinding> lToUnbind = new List<IPropertyPathBinding>();
            //find it and unbind
            foreach (IPropertyPathBinding item in Bindings)
            {
                if (item.PropertyPathSource == propertypath && item.PropertyPathDestination == destPropertypath &&
                    item.Source == source && item.Destination == destination)
                {
                    lToUnbind.Add(item);
                }
            }
            //
            foreach (IPropertyPathBinding item in lToUnbind)
                item.UnBind();
        }

        /// <summary>
        /// enlève un binding
        /// </summary>
        /// <param name="binding"></param>
        public static void RemoveCompiledBinding(IPropertyPathBinding binding)
        {
            binding.UnBind();
        }

        /// <summary>
        /// Active/Désctive un binding à partir de sa source
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public static void EnableDisableBinding(object source, string name, bool value)
        {
            IOneNotify prop = GetOneNotify(source, name);
            if (prop != null)
                prop.Enabled = value;
        }

        /// <summary>
        /// Liste complète des bindings
        /// </summary>
        public static ReadOnlyCollection<IPropertyPathBinding> Bindings
        {
            get
            {
                return new ReadOnlyCollection<IPropertyPathBinding>(_Bindings);
            }
        }

        /// <summary>
        /// Creates a converter around a delegate, that can be used in <see cref="AddCompiledBinding"/> method
        /// </summary>
        /// <typeparam name="TResult">type of conversion result</typeparam>
        /// <typeparam name="TValue">typeo of  value to convert</typeparam>
        /// <param name="converterDelegate">delegate that implements the conversion</param>
        /// <returns>an object, that wrap delegate</returns>
        public static IBinderConverter CreateConverter<TResult, TValue>(BinderConverterDelegate<TResult, TValue> converterDelegate)
        {
            return new BinderConverterDelegateWrapper<TResult, TValue>(converterDelegate);
        }
        #endregion
    }
}
